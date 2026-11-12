using Microsoft.CommandPalette.Extensions.Toolkit;

namespace SecureVault;

internal sealed partial class CopySecretCommand : InvokableCommand
{
    private readonly string _value;
    private readonly string _title;
    private readonly bool _isPassword;

    public CopySecretCommand(VaultEntry entry)
    {
        _value = entry.Secret;
        _title = entry.Title;
        _isPassword = entry.Kind == EntryKind.Password;
        Name = "Copy to clipboard";
        Icon = new IconInfo(""); // Copy
    }

    public override CommandResult Invoke()
    {
        if (!_isPassword)
        {
            SecureClipboard.CopyText(_value);
            return CommandResult.ShowToast($"Copied note \"{_title}\"");
        }

        var (ok, message) = WindowsHello.Verify($"Copy the password for \"{_title}\"");
        if (!ok)
        {
            return CommandResult.ShowToast(message);
        }

        SecureClipboard.CopySecret(_value);
        return CommandResult.ShowToast($"Copied password for \"{_title}\" (clears in 30s)");
    }
}