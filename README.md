<p align="center">
<a href="https://chivatyys.netlify.app/">
<img src="https://github.com/lokcme/ChivaStrap/blob/main/src/Chivas.App/Chivas.png" alt="preview" width="100px"/>
</a>
</p>

<h1 align="center"><b>Chivas</b></h1>

<p align="center">
  <img src="https://raw.githubusercontent.com/lokcme/ChivaStrap/main/assets/save.png" alt="preview" width="85%"/>
</p>

<p align="center">
  <a href="https://github.com/lokcme/ChivaStrap/releases/latest">Latest release</a> |
  <a href="https://chivatyys.netlify.app/pages/documentation">Documentation</a> |
  <a href="https://discord.gg/5tJBqBH8ck">Discord</a>
</p>

<div align="center">

[![Total Downloads][shield-repo-total]][repo-releases]
[![Latest Downloads][shield-repo-downloads]][repo-latest]
[![Latest Release][shield-repo-latest]][repo-latest]
[![Discord][shield-discord-server]][discord-invite]
[![Stars][shield-repo-stars]][repo-stargazers]
[![Sponsors][shield-repo-sponsors]][sponsor-link]

</div>

<h5 align="center">
  Leave a star if you like the project! ⭐️
</h5>

<p align="center">
  <img src="https://raw.githubusercontent.com/devicons/devicon/master/icons/windows8/windows8-original.svg" alt="Windows" width="32" height="32"/>
  &nbsp;&nbsp;
  <img src="https://raw.githubusercontent.com/devicons/devicon/master/icons/android/android-original.svg" alt="Android" width="32" height="32"/>
  &nbsp;&nbsp;
  <img src="https://raw.githubusercontent.com/devicons/devicon/master/icons/linux/linux-original.svg" alt="Linux" width="32" height="32"/>
  &nbsp;&nbsp;

  <!-- macOS
  <img src="https://raw.githubusercontent.com/devicons/devicon/master/icons/apple/apple-original.svg" alt="macOS" width="32" height="32"/>
  -->
</p>

## Quick Install

```powershell
irm https://chivatyys.netlify.app/quick-install | iex
```

---

> [!IMPORTANT]
> Chivas currently supports **Windows 10 and above**, **Android**, and **Linux**.  
> **macOS support is currently in development** and will be available in a future release.
>
> If you're looking for a MacOS Bootstrapper in the meantime:
> - **macOS:** [AppleBlox](https://github.com/AppleBlox/appleblox)

> [!WARNING]
> Chivas is not an exploit and never will be. We are not considered an exploit. We are here to give users more freedom, features, and support for Roblox.
>
> As of Chivas Version `1.1.2.3`, Multi-Instance Launching has been removed from the app and will not be added back in the future.

## FAQ

<details>
  <summary><picture><source media="(prefers-color-scheme: dark)" srcset="assets/readme/faq-banned-dark.svg"><img src="assets/readme/faq-banned-light.svg" alt="Can Chivas get me banned?" width="82%" height="40" align="left"></picture><picture><img src="assets/readme/spacer.svg" alt="" width="1" height="40" align="middle"></picture></summary>

  Chivas does not inject cheats, exploit Roblox, or bypass Roblox security. It functions as a launcher and configuration manager.
</details>

<details>
  <summary><picture><source media="(prefers-color-scheme: dark)" srcset="assets/readme/faq-virus-dark.svg"><img src="assets/readme/faq-virus-light.svg" alt="Is Chivas a virus?" width="82%" height="40" align="left"></picture><picture><img src="assets/readme/spacer.svg" alt="" width="1" height="40" align="middle"></picture></summary>

  Chivas is fully open source, allowing anyone to inspect and review its source code.
  If your antivirus flags Chivas, it's a false positive caused by how Windows detects and handles unsigned applications.
  
  You can review the complete source code [here](https://github.com/lokcme/ChivaStrap).
</details>

## Installation

1. Download the [latest version](https://github.com/lokcme/ChivaStrap/releases/latest).
2. Run the `.exe` and finish the setup.
3. Launch Chivas.
4. Enjoy a simpler Roblox!

<details>
  <summary><picture><source media="(prefers-color-scheme: dark)" srcset="assets/readme/android-setup-dark.svg"><img src="assets/readme/android-setup-light.svg" alt="Android setup" width="82%" height="40" align="left"></picture><picture><img src="assets/readme/spacer.svg" alt="" width="1" height="40" align="middle"></picture></summary>

Chivas for Android needs Android 7 or newer. Download `Chivas-Android-direct-<version>.apk` from the [latest release](https://github.com/lokcme/ChivaStrap/releases/latest) and install it.

Android doesn't let apps change Roblox's files by themselves, so Chivas needs extra access to apply your FastFlags. There are three ways to give it that access. You can start any of them from **Settings > Chivas helper** in the app, which also shows your exact command with a copy button.

| Method | Needs a computer | After your phone restarts |
| --- | --- | --- |
| Shizuku (easiest) | No | Start Shizuku again, and Chivas takes care of the rest |
| ADB | Yes | Run the start command again |
| Root | No | Nothing, it keeps working |

Mods only work with root.

<details>
  <summary><picture><source media="(prefers-color-scheme: dark)" srcset="assets/readme/android-shizuku-dark.svg"><img src="assets/readme/android-shizuku-light.svg" alt="Shizuku" width="82%" height="40" align="left"></picture><picture><img src="assets/readme/spacer.svg" alt="" width="1" height="40" align="middle"></picture></summary>

1. Install [Shizuku](https://shizuku.rikka.app/) and start it by following its own guide.
2. In Chivas, open **Settings > Chivas helper** and tap **Start with Shizuku**.
3. Allow Chivas when Shizuku asks.

</details>

<details>
  <summary><picture><source media="(prefers-color-scheme: dark)" srcset="assets/readme/android-adb-usb-dark.svg"><img src="assets/readme/android-adb-usb-light.svg" alt="ADB with a USB cable" width="82%" height="40" align="left"></picture><picture><img src="assets/readme/spacer.svg" alt="" width="1" height="40" align="middle"></picture></summary>

1. Turn on Developer options: open **Settings > About phone** and tap **Build number** seven times. On some phones, Build number is inside **Software information**, or is called **OS version** instead.
2. Open **Developer options** (usually under **Settings > System**) and turn on **USB debugging**.
3. On your computer, download Google's [SDK Platform Tools](https://developer.android.com/tools/releases/platform-tools) and unzip it.
4. Open a terminal inside the unzipped `platform-tools` folder.
5. Plug your phone into the computer with a USB cable, then run:

   ```bash
   adb devices
   ```

6. Unlock your phone and tap **Allow** on the USB debugging prompt.
7. Run the start command:

   ```bash
   adb shell "content read --uri content://com.chivas.android.direct.start/start.sh | sh"
   ```

In PowerShell, type `.\adb` instead of `adb`. If `adb devices` shows nothing, try another cable, since some cables can only charge.

</details>

<details>
  <summary><picture><source media="(prefers-color-scheme: dark)" srcset="assets/readme/android-adb-wifi-dark.svg"><img src="assets/readme/android-adb-wifi-light.svg" alt="ADB over WiFi (Android 11 or newer)" width="82%" height="40" align="left"></picture><picture><img src="assets/readme/spacer.svg" alt="" width="1" height="40" align="middle"></picture></summary>

1. Follow steps 1 to 4 of the USB cable guide.
2. Connect your phone and your computer to the same WiFi network.
3. In **Developer options**, turn on **Wireless debugging**, open it, and tap **Pair device with pairing code**.
4. Pair using the IP address, port and code shown in that popup:

   ```bash
   adb pair IP:PORT CODE
   ```

5. Connect using the IP address and port shown on the main Wireless debugging screen. This port is different from the pairing port.

   ```bash
   adb connect IP:PORT
   ```

6. Run the start command from step 7 of the USB cable guide.

</details>

<details>
  <summary><picture><source media="(prefers-color-scheme: dark)" srcset="assets/readme/android-root-dark.svg"><img src="assets/readme/android-root-light.svg" alt="Root" width="82%" height="40" align="left"></picture><picture><img src="assets/readme/spacer.svg" alt="" width="1" height="40" align="middle"></picture></summary>

In Chivas, open **Settings**, turn on **Use root**, and allow Chivas in your root manager.

</details>

</details>

### Gentoo Linux

Chivas is packaged for Gentoo in the [Chivas overlay](https://github.com/Pedrodroks34/chivas-overlay), maintained by [@Pedrodroks34](https://github.com/Pedrodroks34).

The overlay provides two ebuilds:

- `games-action/chivas`: builds the latest stable release from source.
- `games-action/chivas-9999`: builds the latest code from the upstream `main` branch.

The versioned package is recommended for normal installations. The `9999` live ebuild is only needed if you want to test the latest development code; it is **not required** for installing Chivas.

Run these commands as root:

1. Add the overlay:

   ```bash
   emerge --ask app-eselect/eselect-repository dev-vcs/git
   eselect repository add Chivas-Overlay git https://github.com/Pedrodroks34/chivas-overlay.git
   emaint sync --repo Chivas-Overlay
   ```

2. Allow Chivas and its .NET SDK dependency:

   ```bash
   mkdir -p /etc/portage/package.accept_keywords
   echo 'games-action/chivas ~amd64' > /etc/portage/package.accept_keywords/chivas
   echo 'dev-dotnet/dotnet-sdk-bin ~amd64' > /etc/portage/package.accept_keywords/dotnet-sdk-bin
   ```

   If `/etc/portage/package.accept_keywords` is a file on your system, add both lines to that file instead.

3. Build and install Chivas:

   ```bash
   emerge --ask games-action/chivas
   ```

To update to a newer stable release, sync the overlay and run step 3 again:

```bash
emaint sync --repo Chivas-Overlay
emerge --ask games-action/chivas
```

#### Live version

The `games-action/chivas-9999` ebuild tracks the upstream `main` branch. It is intended for testing the latest development changes and is **not necessary for normal installation**.

To use the live version:

```bash
echo 'games-action/chivas **' > /etc/portage/package.accept_keywords/chivas
emerge --ask =games-action/chivas-9999
```

The live ebuild may contain unreleased changes and can require newer dependencies than the latest stable release.

## Built With

[![C#][shield-csharp]][link-csharp] [![.NET][shield-dotnet]][link-dotnet] [![WPF][shield-wpf]][link-wpf] [![WPF UI][shield-wpfui]][link-wpfui] [![WebView2][shield-webview2]][link-webview2]

[![Java][shield-java]][link-java] [![Rust][shield-rust]][link-rust] [![Android][shield-android]][link-android]

## Building

| App | Build it on |
| --- | --- |
| Windows | Windows |
| Linux | Linux or Windows |
| Android | Windows or Linux |

Start by cloning the repository with [Git](https://git-scm.com/downloads):

```bash
git clone https://github.com/lokcme/ChivaStrap.git
cd Chivas
```

> [!TIP]
> On Windows, clone into a short folder like `C:\src\Chivas`. Windows limits file paths to 260 characters, so a deeply nested folder can make the clone or the build fail.

### Windows app

**You need:** Windows 10 or 11 (64 bit) and the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), version 10.0.300 or a newer 10.0 release.

```powershell
dotnet build Chivas.sln -c Release
```

The app is saved to `src\Chivas.App\bin\Release\net10.0-windows\win-x64\Chivas.exe`.

To build the single `Chivas.exe` file that releases use:

```powershell
powershell -ExecutionPolicy Bypass -File .\publish-all.ps1 -Only windows
```

It is saved to `PublishedBuilds\Windows\Chivas.exe`. Like the release version, it needs the [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) to run.

### Linux app

**You need:** the [.NET 10 SDK](https://learn.microsoft.com/en-us/dotnet/core/install/linux), version 10.0.300 or a newer 10.0 release. Check it with `dotnet --version`. If your distro only offers an older version, use Microsoft's [install script](https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual).

```bash
dotnet publish src/Chivas.Cross/Chivas.Cross.csproj \
  -c Release -r linux-x64 --self-contained true \
  -o PublishedBuilds/Linux-x64 \
  -p:EnableWindowsTargeting=true \
  -p:DebugType=none -p:DebugSymbols=false
```

The app is saved to `PublishedBuilds/Linux-x64/Chivas`. It includes .NET, so there is nothing else to install. Build only this project on Linux, because the full solution also contains the Windows app, which only builds on Windows.

For other systems, change `linux-x64` (and the output folder) to:

* `linux-arm64` for ARM64
* `linux-musl-x64` or `linux-musl-arm64` for musl based distros like Alpine

**Release packages (optional)**

To make `.tar.gz`, `.deb`, `.rpm`, `.AppImage` and `.flatpak` files, install [PowerShell 7](https://learn.microsoft.com/en-us/powershell/scripting/install/linux-overview) and the packaging tools. On Ubuntu or Debian:

```bash
sudo apt-get install curl ca-certificates binutils dpkg rpm flatpak flatpak-builder
```

Then run:

```bash
pwsh -NoProfile -File ./publish-all.ps1 -Only linux-x64 -LinuxPackages -NoPause
```

The packages are saved to `PublishedBuilds/Linux`. The first run takes longer because it downloads the AppImage and Flatpak tools.

* For an AppImage only, use `-AppImage` instead of `-LinuxPackages`. This skips the RPM and Flatpak tools.
* musl builds only produce a `.tar.gz`.

### Android app

**You need:**

* [JDK 17](https://adoptium.net/temurin/releases/?version=17)
* The [Android SDK](https://developer.android.com/studio) with SDK Platform 37, Build Tools 37.0.0 and NDK 29.0.14206865 (install them from the SDK Manager in Android Studio)
* [Rust](https://rustup.rs/) 1.85 or newer

Set `JAVA_HOME` to your JDK folder and `ANDROID_HOME` to your Android SDK folder. Then add the Rust targets (only needed once):

```bash
rustup target add aarch64-linux-android armv7-linux-androideabi x86_64-linux-android
```

Build the debug APKs:

```bash
cd android
./gradlew assembleDebug
```

On Windows, use `.\gradlew.bat assembleDebug` instead. The APKs are saved to `android/app/build/outputs/apk/`, in two versions:

* `play`: the Google Play version
* `direct`: the GitHub version, which updates itself and includes the server matchmaker

**Signed release APKs**

Release APKs are signed with your own keystore. Add these lines to `~/.gradle/gradle.properties`:

```properties
chivas.storeFile=/path/to/your.keystore
chivas.storePassword=your_store_password
chivas.keyAlias=your_key_alias
chivas.keyPassword=your_key_password
```

Then run this from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\publish-all.ps1 -Only android
```

On Linux, use `pwsh -NoProfile -File ./publish-all.ps1 -Only android` instead. The signed APKs are saved to `PublishedBuilds/Android`.

> [!TIP]
> The publish script reuses build caches to save time. Add `-Clean` to any `publish-all.ps1` command to rebuild from scratch.

## Forking

To create your own copy of Chivas:

1. Open the [Chivas repository](https://github.com/lokcme/ChivaStrap).
2. Click **Fork**.
3. Select your GitHub account.

## Credits

Chivas is built on [Bloxstrap](https://github.com/bloxstraplabs/bloxstrap) by pizzaboxer.

## License

[![License][shield-license]][link-license]

Chivas is released under the [MIT License](LICENSE.CHIVAS). It is built on Bloxstrap and Fishstrap, and their licenses are kept alongside it:

| Project | License | Copyright | File |
| --- | --- | --- | --- |
| Chivas | MIT | © 2026 Bratic | [LICENSE.CHIVAS](LICENSE.CHIVAS) |
| Bloxstrap | MIT | © 2022 pizzaboxer | [LICENSE.BLOXSTRAP](LICENSE.BLOXSTRAP) |
| Fishstrap | MIT | © 2025 returnrqt | [LICENSE.FISHSTRAP](LICENSE.FISHSTRAP) |

<details>
  <summary><picture><source media="(prefers-color-scheme: dark)" srcset="assets/readme/third-party-licenses-dark.svg"><img src="assets/readme/third-party-licenses-light.svg" alt="Third party licenses" width="82%" height="40" align="left"></picture><picture><img src="assets/readme/spacer.svg" alt="" width="1" height="40" align="middle"></picture></summary>

Components included in this repository keep their own licenses:

| Component | Used in | License | File |
| --- | --- | --- | --- |
| [WPF UI](https://github.com/lepoco/wpfui) | Windows app | MIT | [License](external/wpfui/LICENSE) |
| WPF UI docs template by Singulink | WPF UI docs | MIT | [License](external/wpfui/docs/templates/LICENSE) |
| Fluent System Icons by Microsoft | Windows app | MIT | [License](external/wpfui/src/Wpf.Ui/License%20-%20Fluent%20System%20Icons.txt) |
| Segoe Fluent Icons by Microsoft | Windows app | Microsoft font license | [License](external/wpfui/src/Wpf.Ui/License%20-%20Segoe%20Fluent%20Icons.txt) |
| VirtualizingWrapPanel | Windows app | MIT | [License](external/wpfui/src/Wpf.Ui/License%20-%20VirtualizingWrapPanel.txt) |
| Fira Code font | Windows app | SIL Open Font License 1.1 | [License](external/wpfui/src/Wpf.Ui/License%20-%20Fira%20Code.txt) |
| Inter font | Windows app | SIL Open Font License 1.1 | [License](src/Chivas.App/Resources/Fonts/OFL-Inter.txt) |
| Selawik font by Microsoft | Windows app | SIL Open Font License 1.1 | [License](src/Chivas.App/Resources/Fonts/OFL-Selawik.txt) |
| Montserrat font | Windows app | SIL Open Font License 1.1 | [License](src/Chivas.App/Resources/Fonts/OFL-Montserrat.txt) |
| Noto Sans Thai font | Windows app | SIL Open Font License 1.1 | [License](src/Chivas.App/Resources/Fonts/OFL-NotoSansThai.txt) |
| Rubik font | Windows app | SIL Open Font License 1.1 | [License](src/Chivas.App/Resources/Fonts/OFL-Rubik.txt) |
| Chivas Emoji font, built from [Twemoji Mozilla](https://github.com/mozilla/twemoji-colr) 0.7.0 by `build/Packaging/Linux/build-emoji-font.py`, art by [Twemoji](https://github.com/twitter/twemoji) | Linux app | CC BY 4.0 art, Apache 2.0 build code | [License](src/Chivas.App/Resources/Fonts/LICENSE-TwemojiMozilla.txt) |
| [Bibata Modern Ice cursors](https://github.com/ful1e5/Bibata_Cursor) | Windows and Android apps | GPL 3.0 | [License](src/Chivas.App/Resources/Mods/Cursor/BibataModernIce/LICENSE.txt) |
| ppmd-rust | Android app | CC0 1.0 or MIT No Attribution | [CC0](android/rust/vendor/ppmd-rust/LICENSE-CC0.md), [MIT-0](android/rust/vendor/ppmd-rust/LICENSE-MIT-0.md) |

</details>

---

<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/lokcme/ChivaStrap/output/star-chart-dark.svg">
    <source media="(prefers-color-scheme: light)" srcset="https://raw.githubusercontent.com/lokcme/ChivaStrap/output/star-chart-light.svg">
    <img src="https://raw.githubusercontent.com/lokcme/ChivaStrap/output/star-chart-light.svg" alt="Chivas star history" width="100%">
  </picture>
</p>

<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/lokcme/ChivaStrap/output/github-contribution-grid-snake-dark.svg">
    <source media="(prefers-color-scheme: light)" srcset="https://raw.githubusercontent.com/lokcme/ChivaStrap/output/github-contribution-grid-snake.svg">
    <img src="https://raw.githubusercontent.com/lokcme/ChivaStrap/output/github-contribution-grid-snake.svg" alt="GitHub contribution graph">
  </picture>
</p>

<p align="center">
  <a href="https://discord.gg/5tJBqBH8ck">
    <img src="https://discord.com/api/guilds/1327967202015580223/widget.png?style=banner2" alt="Join the Chivas Discord Bro">
  </a>
</p>

[shield-repo-downloads]:  https://img.shields.io/github/downloads/lokcme/ChivaStrap/latest/total?color=981bfe
[shield-repo-total]:      https://img.shields.io/github/downloads/lokcme/ChivaStrap/total?color=8a2be2
[shield-repo-latest]:     https://img.shields.io/github/v/release/lokcme/ChivaStrap?color=7a39fb
[shield-repo-stars]:      https://img.shields.io/github/stars/lokcme/ChivaStrap?color=ffd700
[shield-discord-server]:  https://img.shields.io/discord/1327967202015580223?logo=discord&logoColor=white&label=Discord&color=4d3dff
[shield-repo-sponsors]:   https://img.shields.io/github/sponsors/lokcme?logo=githubsponsors&logoColor=white&label=Sponsors&color=ea4aaa

[repo-releases]:          https://github.com/lokcme/ChivaStrap/releases
[repo-latest]:            https://github.com/lokcme/ChivaStrap/releases/latest
[repo-stargazers]:        https://github.com/lokcme/ChivaStrap/stargazers
[discord-invite]:         https://discord.gg/dfA9PdWgcV
[sponsor-link]:           https://github.com/sponsors/lokcme

[shield-license]:         https://img.shields.io/github/license/lokcme/ChivaStrap?style=for-the-badge
[link-license]:           LICENSE.CHIVAS

[shield-csharp]:          https://img.shields.io/badge/C%23-239120?style=for-the-badge
[shield-dotnet]:          https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white
[shield-wpf]:             https://img.shields.io/badge/WPF-0078D4?style=for-the-badge
[shield-wpfui]:           https://img.shields.io/badge/WPF%20UI-24292F?style=for-the-badge
[shield-webview2]:        https://img.shields.io/badge/WebView2-0C59A4?style=for-the-badge
[shield-java]:            https://img.shields.io/badge/Java-ED8B00?style=for-the-badge&logo=openjdk&logoColor=white
[shield-rust]:            https://img.shields.io/badge/Rust-000000?style=for-the-badge&logo=rust&logoColor=white
[shield-android]:         https://img.shields.io/badge/Android-3DDC84?style=for-the-badge&logo=android&logoColor=white

[link-csharp]:            https://learn.microsoft.com/dotnet/csharp/
[link-dotnet]:            https://dotnet.microsoft.com/
[link-wpf]:               https://github.com/dotnet/wpf
[link-wpfui]:             https://github.com/lepoco/wpfui
[link-webview2]:          https://developer.microsoft.com/microsoft-edge/webview2/
[link-java]:              https://openjdk.org/
[link-rust]:              https://www.rust-lang.org/
[link-android]:           https://developer.android.com/
