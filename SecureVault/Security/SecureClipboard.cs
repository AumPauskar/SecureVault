using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.ApplicationModel.DataTransfer;

namespace SecureVault;

internal static class SecureClipboard
{
    private static readonly TimeSpan ClearAfter = TimeSpan.FromSeconds(30);

    [DllImport("user32.dll")]
    private static extern uint GetClipboardSequenceNumber();

    /// <summary>Plain copy, used for notes.</summary>
    public static void CopyText(string text) => ClipboardHelper.SetText(text);

    /// <summary>
    /// Copies a secret so it is kept out of Win+V clipboard history and cloud sync,
    /// then clears it after 30 seconds if nothing else has been copied since.
    /// </summary>
    public static void CopySecret(string text)
    {
        try
        {
            var package = new DataPackage();
            package.SetText(text);
            Clipboard.SetContentWithOptions(package, new ClipboardContentOptions
            {
                IsAllowedInHistory = false,
                IsRoamable = false,
            });
            Clipboard.Flush();
        }
        catch
        {
            // Fallback: still copies, but this path can appear in clipboard history.
            ClipboardHelper.SetText(text);
        }

        var sequence = GetClipboardSequenceNumber();
        _ = Task.Run(async () =>
        {
            await Task.Delay(ClearAfter);
            if (GetClipboardSequenceNumber() != sequence)
            {
                return; // the user copied something else; leave it alone
            }

            try
            {
                Clipboard.Clear();
            }
            catch
            {
                try
                {
                    ClipboardHelper.SetText(string.Empty);
                }
                catch
                {
                    // best effort
                }
            }
        });
    }
}