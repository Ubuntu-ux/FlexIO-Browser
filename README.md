# FlexIO Browser

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![Open Source](https://img.shields.io/badge/Open%20Source-Yes-green.svg)

FlexIO Browser is a lightweight, open-source web browser built with C# and WebView2. It provides a clean, simple interface while leveraging the power of the Chromium engine.

## Features

- 🚀 Modern Chromium-based engine (WebView2)
- 📱 Clean and intuitive user interface
- 🔒 Secure browsing experience
- 🛠 Basic navigation controls
- 💫 Fast and lightweight
- 📦 Easy installation process

## Requirements

- Windows 10/11
- [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Microsoft Edge WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)

## Installation

1. Download the latest installer from the [Releases](../../releases) page
2. Extract the files to folder, and run FlexIOBrowserInstaller.exe with administrator privileges
3. Follow the installation wizard
4. Launch FlexIO Browser from desktop shortcut or start menu

## Building from Source

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or Visual Studio Code (optional)

### Browser Build

```bash
cd FlexIO-Browser
dotnet restore
dotnet build
dotnet run
```

### Installer Build

```bash
cd FlexIOBrowserInstaller
dotnet restore
dotnet publish -c Release
```

## Project Structure

```
├── FlexIO-Browser/           # Browser source code
│   ├── FlexIO-Browser.csproj
│   ├── Program.cs
│   └── MainForm.cs
│
└── FlexIOBrowserInstaller/   # Installer source code
    ├── FlexIOBrowserInstaller.csproj
    ├── FlexIOBrowserInstaller.cs
    └── app.manifest
```

## Contributing

We welcome contributions! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Open Source Statement

FlexIO Browser is proudly open source. We believe in the power of community-driven development and welcome contributions from developers around the world. Our goal is to create a transparent, accessible, and collaborative browser project that can be used, studied, modified, and distributed by anyone.

## Acknowledgments

- [Microsoft Edge WebView2](https://docs.microsoft.com/en-us/microsoft-edge/webview2/)
- [.NET Community](https://dotnet.microsoft.com/platform/community)
- All our contributors and supporters

---
Made with ❤️ by the FlexIO Team
```