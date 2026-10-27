# Vývojové prostředí na NixOS (Linux)

Projekt cílí na **.NET 9 + .NET MAUI pro Android**. Na Windows se nic z tohoto
nepotřebuje – Visual Studio 2022 si Android SDK i JDK doinstaluje samo
(viz `README.md`). Tento soubor popisuje, co je potřeba na NixOS, kde `.NET`
z nástrojů Microsoftu není zabudovaný.

## 1. Systémová konfigurace (`/etc/nixos/configuration.nix`)

```nix
  # Umožní spouštět binárky z .NET SDK a Android SDK (jsou linkované proti glibc).
  programs.nix-ld.enable = true;
  programs.nix-ld.libraries = with pkgs; [
    zlib
    icu
    openssl
    stdenv.cc.cc.lib
    libgcc
  ];

  environment.systemPackages = with pkgs; [
    android-tools   # adb, fastboot
    jdk17           # Java pro aapt2 / D8
    file
  ];

  users.users.<jméno>.extraGroups = [ ... "kvm" ];
```

- `nix-ld` je nutný: `dotnet`, `adb` a `aapt2` jsou dynamicky linkované.
  Bez něj hlásí `Could not start dynamically linked executable … NixOS cannot
  run dynamically linked executables`.
- `kvm` zrychlí emulátor a je potřeba pro hardwarovou akceleraci.
- USB ladění na NixOS nepotřebuje udev pravidla – práva řeší systemd *uaccess*
  pro přihlášeného uživatele (`android-udev-rules` už v nixpkgs není).

Po úpravě:

```bash
sudo nixos-rebuild switch
# potom se odhlásit a přihlásit (skupiny)
```

## 2. .NET 9 SDK do domovského adresáře

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
bash /tmp/dotnet-install.sh --channel 9.0 --install-dir ~/.dotnet --no-path
fish_add_path -U ~/.dotnet
dotnet --version     # 9.0.319
```

## 3. MAUI workload

```bash
dotnet workload install maui-android
dotnet workload list   # musí obsahovat maui-android
```

## 4. Android SDK

Google SDK se instaluje do `~/Android/Sdk` (stejná složka, jakou používá
Android Studio / Visual Studio, takže projekt zůstává přenosný):

```bash
mkdir -p ~/Android/Sdk/cmdline-tools
cd /tmp
curl -O https://dl.google.com/android/repository/commandlinetools-linux-11076708_latest.zip
unzip -q commandlinetools-linux-11076708_latest.zip -d /tmp/cmdline-temp
mv /tmp/cmdline-temp/cmdline-tools ~/Android/Sdk/cmdline-tools/latest

fish_add_path -U ~/Android/Sdk/platform-tools ~/Android/Sdk/cmdline-tools/latest/bin
set -Ux ANDROID_HOME $HOME/Android/Sdk
set -Ux ANDROID_SDK_ROOT $HOME/Android/Sdk

sdkmanager --licenses
sdkmanager "platform-tools" "platforms;android-35" "build-tools;35.0.0"
```

`JAVA_HOME` nastav na JDK 17 z nixpkgs:

```bash
set -Ux JAVA_HOME (dirname (dirname (readlink -f (which java))))
```

`Directory.Build.props` v projektu si cestu k SDK vezme z `ANDROID_HOME`
(resp. z `~/Android/Sdk`) a `JavaSdkDirectory` z `JAVA_HOME`, takže
v `.csproj` nejsou žádné cesty specifické pro Linux.

## 5. Zařízení

```bash
adb devices          # telefon musí být "device", ne "unauthorized"
```

Při `unauthorized` potvrď na displeji „Povolit ladění USB“.
Alternativa bez kabelu: `adb pair <ip>:<port>` (bezdrátové ladění).

Emulátor (APK 35):

```bash
emulator -avd Medium_Phone_API_35 -no-snapshot -gpu swiftshader_indirect
```

## 6. Build a nasazení

```bash
cd PennyWise
dotnet build -f net9.0-android -c Debug
dotnet build -f net9.0-android -c Debug -t:Install   # nainstaluje na zařízení
dotnet build -f net9.0-android -c Release -p:AndroidPackageFormat=apk
```

## Známé chyby

| Chyba | Řešení |
|---|---|
| `Could not start dynamically linked executable` | `programs.nix-ld.enable = true` + rebuild + re-login |
| `XA5300: The Android SDK directory could not be found` | `ANDROID_HOME`, případně `-p:AndroidSdkDirectory=…` |
| `An exception occurred while validating the Java SDK` | `JAVA_HOME` na JDK 17 (`readlink -f $(which java)`) |
| `No space left on device` při `workload install` | bývá zaplněný `TMPDIR`; nastav `TMPDIR` na disk s místem |
| `adb: device unauthorized` | potvrdit dialog na telefonu |
| `sdkmanager` z nixpkgs padá na `/opt/android-sdk` | použij `~/Android/Sdk/cmdline-tools/latest/bin/sdkmanager` |
