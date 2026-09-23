<img width="1920" height="1080" alt="Zrzut ekranu z 2026-08-23 11-56-58" src="https://github.com/user-attachments/assets/ab01e90e-5e46-417d-888f-8b2ef8cad087" />

# phonetolinux

An application that integrates Android with the Linux environment... allowing you to manage your phone from your computer.

### What works...

- **Making / receiving calls** directly from Linux with notification overlay and native dialer.
- **Auto-detection of new IP addresses** and seamless automatic IP resolution (`DeviceIpResolver`) for remote file storage browsing without manual IP entry.
- **Sending / reading SMS messages** with real-time thread synchronization.
- **Securing configuration files** and session data with 256-bit AES encryption & key derivation.
- **Browsing, copying, and downloading files** to and from the phone via WebDAV.
- **Self-update function** via GitHub releases (`UpdateService`).
- **Security vulnerabilities patched** (upgraded `Tmds.DBus.Protocol` to `0.21.3` addressing GHSA-xrw6-gwf8-vvr9).

### What doesn't work...

- **MMS attachments** (under active development).
- **Emojis** in native notification popups.

---

### Installation (.deb Package)

From now on you can download a `.deb` installer file from https://drive.google.com/file/d/10sup1_GX17QD7ToKqYIRILAKscH8B0L9/view?usp=sharing. This is the last package you install manually. On future versions, you can use the "Update" function directly in settings.