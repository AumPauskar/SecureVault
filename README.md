# SecureVault

SecureVault is a local vault extension for Command Palette. It lets you save and retrieve notes and passwords by title.

## Storing entries

- **Notes:** Choose **Store note**, enter a title, then type the note in the Command Palette search box and select the save action. The note text appears in the save suggestion, so avoid entering sensitive passwords as notes.
    ![Storing notes](https://raw.githubusercontent.com/AumPauskar/repo-media/main/SecureVault/store_note.gif)
- **Passwords:** Choose **Store password**, enter a title, then enter the password in the masked password form and submit it. The password is not displayed in the suggestion.
    ![Storing passwords](https://raw.githubusercontent.com/AumPauskar/repo-media/main/SecureVault/store_password.gif)
- Saving an entry with an existing title of the same type replaces it. Note and password titles are treated separately and compared without regard to letter case.

## Retrieving entries

- Notes can be previewed in the results and copied to the clipboard.
    ![Viewing notes](https://raw.githubusercontent.com/AumPauskar/repo-media/main/SecureVault/view_note.gif)
- Passwords are displayed masked and require Windows Hello verification before they can be copied. Deleting a password also requires Windows Hello.
    ![Viewing passwords](https://raw.githubusercontent.com/AumPauskar/repo-media/main/SecureVault/view_password.gif)
- Password copies are excluded from clipboard history and roaming when supported, and the clipboard is cleared after 30 seconds if its contents have not changed. If that protected clipboard operation fails, the app falls back to a regular clipboard copy.

## Local storage and protection

Notes and passwords are stored together in the local vault, encrypted with AES-256-GCM. The encryption key is protected with Windows DPAPI for the current Windows user. This protects the stored vault on disk; note contents are still visible while entering or previewing them in Command Palette.

## Creating a GitHub release

The **Build and release** workflow creates signed x64 and ARM64 MSIX packages and publishes them as a GitHub Release. Run it from **Actions → Build and release → Run workflow** and enter a four-part version such as `1.0.0.0`. The workflow creates the corresponding `v1.0.0.0` tag.

Before running it, configure these repository Actions secrets:

- `WINDOWS_SIGNING_CERTIFICATE_BASE64`: the signing `.pfx` file encoded as a single-line Base64 string.
- `WINDOWS_SIGNING_CERTIFICATE_PASSWORD`: the `.pfx` password.

For a self-signed certificate suitable for sideloading, run this in PowerShell, choose a strong PFX password, then add the clipboard contents and password as the two secrets above:

```powershell
$cert = New-SelfSignedCertificate -Type Custom -Subject "CN=SecureVault" -KeyUsage DigitalSignature -CertStoreLocation "Cert:\CurrentUser\My" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
$password = Read-Host "PFX password" -AsSecureString
Export-PfxCertificate -Cert $cert -FilePath .\SecureVaultSigning.pfx -Password $password
[Convert]::ToBase64String([IO.File]::ReadAllBytes(".\SecureVaultSigning.pfx")) | Set-Clipboard
```

The signing certificate subject must be `CN=SecureVault`, matching the package identity in `SecureVault/Package.appxmanifest`. The release includes the public `SecureVaultSigning.cer` certificate, but never the private `.pfx`. For sideloading, each recipient must trust that certificate on their Windows PC before installing the matching MSIX; only trust certificates obtained through a source you trust. A certificate trusted by Windows does not mean the package is Microsoft Store certified.

The workflow only distributes the app. Vault entries remain encrypted for the original Windows user and are not included in the release.
