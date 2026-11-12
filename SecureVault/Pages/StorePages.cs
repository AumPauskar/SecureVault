using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace SecureVault;

// Step 1 of storing: the user types a title, presses Enter.
internal sealed partial class StoreTitlePage : DynamicListPage
{
    private readonly string _kind;
    private IListItem[] _items;

    public StoreTitlePage(string kind)
    {
        _kind = kind;
        var label = kind == EntryKind.Password ? "password" : "note";
        Icon = new IconInfo(kind == EntryKind.Password ? "" : "");
        Title = $"Store {label}";
        Name = "Open";
        PlaceholderText = $"Type a title for the {label}, then press Enter";
        _items = Build(string.Empty);
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        _items = Build(newSearch.Trim());
        RaiseItemsChanged();
    }

    public override IListItem[] GetItems() => _items;

    private IListItem[] Build(string title)
    {
        if (title.Length == 0)
        {
            return
            [
                new ListItem(new NoOpCommand())
                {
                    Title = "Type a title...",
                    Subtitle = "For example: Netflix, Wi-Fi, Locker code",
                },
            ];
        }

        IPage next = _kind == EntryKind.Password
            ? new StorePasswordPage(title)
            : new StoreNotePage(title);

        return
        [
            new ListItem(next)
            {
                Title = title,
                Subtitle = Vault.Exists(_kind, title)
                    ? "Already exists. Continuing will replace it"
                    : "Press Enter to continue",
            },
        ];
    }
}

// Step 2 of storing a note: the user types the note, presses Enter to save.
internal sealed partial class StoreNotePage : DynamicListPage
{
    private readonly string _title;
    private IListItem[] _items;

    public StoreNotePage(string title)
    {
        _title = title;
        Icon = new IconInfo("");
        Title = title;
        Name = "Open";
        PlaceholderText = "Type the note, then press Enter to save";
        _items = Build(string.Empty);
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        _items = Build(newSearch);
        RaiseItemsChanged();
    }

    public override IListItem[] GetItems() => _items;

    private IListItem[] Build(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [new ListItem(new NoOpCommand()) { Title = "Type your note...", Subtitle = _title }];
        }

        return
        [
            new ListItem(new SaveEntryCommand(EntryKind.Note, _title, text))
            {
                Title = $"Save note \"{_title}\"",
                Subtitle = text.Length > 120 ? text[..120] + "..." : text,
                Icon = new IconInfo(""),
            },
        ];
    }
}