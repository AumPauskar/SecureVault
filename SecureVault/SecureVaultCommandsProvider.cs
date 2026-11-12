using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace SecureVault;

// Replaces the generated provider. The scaffold's Program.cs / SecureVault.cs reference this class by name.
public partial class SecureVaultCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public SecureVaultCommandsProvider()
    {
        DisplayName = "Secure Vault";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        _commands =
        [
            new CommandItem(new VaultRootPage())
            {
                Title = "Vault",
                Subtitle = "Store and retrieve notes and passwords",
            },
        ];
    }

    public override ICommandItem[] TopLevelCommands() => _commands;
}