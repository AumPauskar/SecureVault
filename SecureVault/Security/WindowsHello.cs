using System;
using System.Threading.Tasks;
using Windows.Security.Credentials.UI;

namespace SecureVault;

/// <summary>
/// Windows Hello gate (PIN, fingerprint or face, whichever the user has set up).
/// Fails closed: if Hello isn't available, the secret is NOT released.
/// </summary>
internal static class WindowsHello
{
    public static (bool Ok, string Message) Verify(string reason)
    {
        try
        {
            // Run on a thread-pool thread so waiting here can't block the host's message loop.
            return Task.Run<(bool Ok, string Message)>(async () =>
            {
                var availability = await UserConsentVerifier.CheckAvailabilityAsync();
                if (availability != UserConsentVerifierAvailability.Available)
                {
                    return (false, "Windows Hello (PIN, fingerprint or face) isn't set up on this PC.");
                }

                var result = await UserConsentVerifier.RequestVerificationAsync(reason);
                return result == UserConsentVerificationResult.Verified
                    ? (true, string.Empty)
                    : (false, "Verification failed or was cancelled.");
            }).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            return (false, "Couldn't start Windows Hello: " + ex.Message);
        }
    }
}