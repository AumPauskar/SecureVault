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