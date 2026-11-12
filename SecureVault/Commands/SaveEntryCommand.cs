using System;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace SecureVault;

internal sealed partial class SaveEntryCommand : InvokableCommand
{
    private readonly string _kind;
    private readonly string _title;
    private readonly string _secret;

    public SaveEntryCommand(string kind, string title, string secret)
    {
        _kind = kind;
        _title = title;
        _secret = secret;
        Name = "Save";
        Icon = new IconInfo(""); // Save
    }

    public override CommandResult Invoke() => Save(_kind, _title, _secret);

    // Shared with the password form.
    public static CommandResult Save(string kind, string title, string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            return CommandResult.ShowToast("Nothing to save");
        }

        var label = kind == EntryKind.Password ? "password" : "note";
        try
        {
            var replaced = Vault.Upsert(kind, title, secret);
            return CommandResult.ShowToast(replaced ? $"Updated {label} \"{title}\"" : $"Saved {label} \"{title}\"");
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast("Couldn't save: " + ex.Message);
        }
    }
}