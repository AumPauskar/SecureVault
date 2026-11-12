using System;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace SecureVault;

// Step 1 of retrieving: type to filter titles, Enter opens the entry.
internal sealed partial class RetrievePage : DynamicListPage
{
    private readonly string _kind;
    private IListItem[] _items = [];

    public RetrievePage(string kind)
    {
        _kind = kind;
        var label = kind == EntryKind.Password ? "password" : "note";
        Icon = new IconInfo(kind == EntryKind.Password ? "" : "");
        Title = $"Retrieve {label}";
        Name = "Open";
        PlaceholderText = $"Type the title of the {label}...";
        Rebuild(string.Empty);
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        Rebuild(newSearch);
        RaiseItemsChanged();
    }

    public override IListItem[] GetItems() => _items;

    private void Rebuild(string filter)
    {
        try
        {
            filter = filter.Trim();
            var matches = Vault.List(_kind)
                .Where(e => filter.Length == 0 || e.Title.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 0)
            {
                _items =
                [
                    new ListItem(new NoOpCommand())
                    {
                        Title = filter.Length == 0 ? "Nothing stored yet" : "No matches",
                        Subtitle = filter.Length == 0 ? "Use Store to add one" : $"Nothing titled \"{filter}\"",
                    },
                ];
                return;
            }

            _items = matches.Select(ToItem).ToArray();
        }
        catch (Exception ex)
        {
            _items = [new ListItem(new NoOpCommand()) { Title = "Couldn't open the vault", Subtitle = ex.Message }];
        }
    }

    private IListItem ToItem(VaultEntry entry)
    {
        var isPassword = entry.Kind == EntryKind.Password;
        return new ListItem(new RevealPage(entry))
        {
            Title = entry.Title,
            Subtitle = isPassword ? "Password" : "Note",
            Icon = new IconInfo(isPassword ? "" : ""),
            MoreCommands = [new CommandContextItem(DeleteCommand(entry))],
        };
    }

    private ICommand DeleteCommand(VaultEntry entry)
    {
        var confirm = new ConfirmationArgs
        {
            Title = $"Delete \"{entry.Title}\"?",
            Description = "This can't be undone.",
            PrimaryCommand = new AnonymousCommand(() =>
            {
                if (entry.Kind == EntryKind.Password)
                {
                    var (ok, message) = WindowsHello.Verify($"Delete the password for \"{entry.Title}\"");
                    if (!ok)
                    {
                        new ToastStatusMessage(message).Show();
                        return;
                    }
                }

                try
                {
                    Vault.Delete(entry.Kind, entry.Title);
                    Rebuild(SearchText);
                    RaiseItemsChanged();
                    new ToastStatusMessage($"Deleted \"{entry.Title}\"").Show();
                }
                catch (Exception ex)
                {
                    new ToastStatusMessage("Couldn't delete: " + ex.Message).Show();
                }
            })
            {
                Name = "Delete",
                Result = CommandResult.KeepOpen(),
            },
        };

        return new AnonymousCommand(null)
        {
            Name = "Delete",
            Icon = new IconInfo(""), // Delete
            Result = CommandResult.Confirm(confirm),
        };
    }
}

// Step 2 of retrieving: shows the data, Enter copies it.
// Passwords are shown masked and need Windows Hello before they're copied.
internal sealed partial class RevealPage : ListPage
{
    private readonly VaultEntry _entry;

    public RevealPage(VaultEntry entry)
    {
        _entry = entry;
        var isPassword = entry.Kind == EntryKind.Password;
        Icon = new IconInfo(isPassword ? "" : "");
        Title = entry.Title;
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        if (_entry.Kind == EntryKind.Password)
        {
            return
            [
                new ListItem(new CopySecretCommand(_entry))
                {
                    Title = new string('•', 12),
                    Subtitle = "Press Enter to verify with Windows Hello and copy",
                    Icon = new IconInfo(""),
                },
            ];
        }

        var flat = _entry.Secret.ReplaceLineEndings(" ");
        return
        [
            new ListItem(new CopySecretCommand(_entry))
            {
                Title = flat.Length > 200 ? flat[..200] + "..." : flat,
                Subtitle = "Press Enter to copy to clipboard",
                Icon = new IconInfo(""),
            },
        ];
    }
}