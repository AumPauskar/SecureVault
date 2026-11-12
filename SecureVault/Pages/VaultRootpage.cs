using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace SecureVault;

// The page you land on after typing the alias (for example "#") and pressing Space/Enter.
internal sealed partial class VaultRootPage : ListPage
{
    public VaultRootPage()
    {
        Icon = new IconInfo(""); // Lock
        Title = "Vault";
        Name = "Open";
        PlaceholderText = "Choose an action...";
    }

    public override IListItem[] GetItems() =>
    [
        new ListItem(new StoreTitlePage(EntryKind.Note))
        {
            Title = "Store note",
            Subtitle = "Save a new encrypted note",
            Icon = new IconInfo(""),
        },
        new ListItem(new RetrievePage(EntryKind.Note))
        {
            Title = "Retrieve note",
            Subtitle = "Find a saved note and copy it",
            Icon = new IconInfo(""),
        },
        new ListItem(new StoreTitlePage(EntryKind.Password))
        {
            Title = "Store password",
            Subtitle = "Save a new password",
            Icon = new IconInfo(""),
        },
        new ListItem(new RetrievePage(EntryKind.Password))
        {
            Title = "Retrieve password",
            Subtitle = "Find a password and copy it (Windows Hello required)",
            Icon = new IconInfo(""),
        },
    ];
}