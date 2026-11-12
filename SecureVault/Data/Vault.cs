using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SecureVault;

internal static class EntryKind
{
    public const string Note = "note";
    public const string Password = "password";
}

internal sealed class VaultEntry
{
    public string Kind { get; set; } = "";
    public string Title { get; set; } = "";
    public string Secret { get; set; } = "";
    public DateTime UpdatedUtc { get; set; }
}

// Source-generated JSON so this keeps working if the project is published with trimming/AOT.
[JsonSerializable(typeof(List<VaultEntry>))]
internal sealed partial class VaultJson : JsonSerializerContext
{
}

/// <summary>
/// Encrypted local store.
///  - vault.dat : the whole vault (titles AND contents) encrypted with AES-256-GCM.
///  - vault.key : the AES key, wrapped with Windows DPAPI for the current user.
/// Nothing is ever written to disk in plain text.
/// </summary>
internal static class Vault
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private static readonly object Gate = new();
    private static readonly byte[] Entropy = System.Text.Encoding.UTF8.GetBytes("SecureVault.v1");

    private static string Dir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SecureVault");

    private static string KeyPath => Path.Combine(Dir, "vault.key");

    private static string DataPath => Path.Combine(Dir, "vault.dat");

    public static IReadOnlyList<VaultEntry> List(string kind)
    {
        lock (Gate)
        {
            return Load()
                .Where(e => e.Kind == kind)
                .OrderByDescending(e => e.UpdatedUtc)
                .ToList();
        }
    }

    public static bool Exists(string kind, string title)
    {
        try
        {
            lock (Gate)
            {
                return Load().Any(e => Matches(e, kind, title));
            }
        }
        catch
        {
            return false;
        }
    }

    /// <returns>true if an existing entry was replaced.</returns>
    public static bool Upsert(string kind, string title, string secret)
    {
        lock (Gate)
        {
            var all = Load();
            var existing = all.FirstOrDefault(e => Matches(e, kind, title));
            var replaced = existing != null;
            if (existing != null)
            {
                all.Remove(existing);
            }

            all.Add(new VaultEntry
            {
                Kind = kind,
                Title = title,
                Secret = secret,
                UpdatedUtc = DateTime.UtcNow,
            });
            Save(all);
            return replaced;
        }
    }

    public static void Delete(string kind, string title)
    {
        lock (Gate)
        {
            var all = Load();
            all.RemoveAll(e => Matches(e, kind, title));
            Save(all);
        }
    }

    private static bool Matches(VaultEntry e, string kind, string title) =>
        e.Kind == kind && string.Equals(e.Title, title, StringComparison.OrdinalIgnoreCase);

    // Throws if the vault exists but can't be decrypted, so a bad read can never
    // cause a later Save() to overwrite the real data with an empty vault.
    private static List<VaultEntry> Load()
    {
        if (!File.Exists(DataPath))
        {
            return new List<VaultEntry>();
        }

        var blob = File.ReadAllBytes(DataPath);
        if (blob.Length < NonceSize + TagSize)
        {
            throw new CryptographicException("Vault file is corrupted.");
        }

        var nonce = blob.AsSpan(0, NonceSize);
        var tag = blob.AsSpan(NonceSize, TagSize);
        var cipher = blob.AsSpan(NonceSize + TagSize);
        var plain = new byte[cipher.Length];
        var key = GetKey();
        try
        {
            using var aes = new AesGcm(key, TagSize);
            aes.Decrypt(nonce, cipher, tag, plain);
            return JsonSerializer.Deserialize(plain, VaultJson.Default.ListVaultEntry) ?? new List<VaultEntry>();
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plain);
            CryptographicOperations.ZeroMemory(key);
        }
    }

    private static void Save(List<VaultEntry> entries)
    {
        Directory.CreateDirectory(Dir);
        var json = JsonSerializer.SerializeToUtf8Bytes(entries, VaultJson.Default.ListVaultEntry);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[json.Length];
        var tag = new byte[TagSize];
        var key = GetKey();
        try
        {
            using var aes = new AesGcm(key, TagSize);
            aes.Encrypt(nonce, json, cipher, tag);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(json);
            CryptographicOperations.ZeroMemory(key);
        }

        var blob = new byte[NonceSize + TagSize + cipher.Length];
        nonce.CopyTo(blob, 0);
        tag.CopyTo(blob, NonceSize);
        cipher.CopyTo(blob, NonceSize + TagSize);

        var tmp = DataPath + ".tmp";
        File.WriteAllBytes(tmp, blob);
        File.Move(tmp, DataPath, overwrite: true);
    }

    private static byte[] GetKey()
    {
        Directory.CreateDirectory(Dir);
        if (File.Exists(KeyPath))
        {
            return ProtectedData.Unprotect(File.ReadAllBytes(KeyPath), Entropy, DataProtectionScope.CurrentUser);
        }

        var key = RandomNumberGenerator.GetBytes(32);
        File.WriteAllBytes(KeyPath, ProtectedData.Protect(key, Entropy, DataProtectionScope.CurrentUser));
        return key;
    }
}