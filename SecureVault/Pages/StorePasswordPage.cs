using System.Text.Json.Nodes;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace SecureVault;

// Step 2 of storing a password. Uses a form so the input is masked
// (a normal search box can't hide what you type).
internal sealed partial class StorePasswordPage : ContentPage
{
    private readonly PasswordForm _form;

    public StorePasswordPage(string title)
    {
        _form = new PasswordForm(title);
        Icon = new IconInfo("");
        Title = title;
        Name = "Open";
    }

    public override IContent[] GetContent() => [_form];
}

internal sealed partial class PasswordForm : FormContent
{
    private readonly string _title;

    public PasswordForm(string title)
    {
        _title = title;

        TemplateJson = """
        {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.6",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Password for ${title}",
                    "weight": "Bolder",
                    "wrap": true
                },
                {
                    "type": "Input.Text",
                    "id": "secret",
                    "style": "Password",
                    "placeholder": "Type the password",
                    "isRequired": true,
                    "errorMessage": "A password is required"
                }
            ],
            "actions": [
                { "type": "Action.Submit", "title": "Save" }
            ]
        }
        """;

        DataJson = new JsonObject { ["title"] = title }.ToJsonString();
    }

    public override CommandResult SubmitForm(string payload)
    {
        var input = JsonNode.Parse(payload)?.AsObject();
        var secret = input?["secret"]?.ToString() ?? string.Empty;
        return SaveEntryCommand.Save(EntryKind.Password, _title, secret);
    }
}