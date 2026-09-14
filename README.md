<div align="center">

# Windows-Duo 💻✨

**Bring the stunning Mac-Duo folding screen effect to your Windows laptop.**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![GitHub stars](https://img.shields.io/github/stars/YOUR_USERNAME/Windows-Duo.svg?style=social&label=Star)](https://github.com/YOUR_USERNAME/Windows-Duo)

*Close your laptop lid and watch your screen content beautifully tilt, blur, and fade in 3D right before the system goes to sleep.*

</div>

---

## 🌟 Features

- **Hardware Integration:** Instantly detects when you start closing or opening your laptop lid using low-level Windows Power Setting APIs.
- **Real-Time 3D Rendering:** Captures your current desktop screen and applies a smooth 3D perspective tilt, replicating a folding physical screen.
- **Cinematic Polish:** Applies dynamic Gaussian blur and darkening overlays as the lid closes, creating a premium aesthetic.
- **Lightweight & Silent:** Runs quietly in the background without a taskbar icon. Consumes almost zero resources when the lid is static.
- **Self-Contained:** Can be published as a single portable `.exe` file. No messy installers required!

## 🚀 Quick Start (Download)

Don't want to compile from source? You can download the latest pre-built portable executable:

1. Go to the [Releases](../../releases) page.
2. Download `WindowsDuo_App.zip`.
3. Extract the folder and double-click `WindowsDuo.exe`. 
4. The app will launch silently in the background. Start closing your lid to see the effect!

*(Pro tip: Press `Win + R`, type `shell:startup`, and put a shortcut to `WindowsDuo.exe` there so it runs every time you turn on your PC).*

## 🛠️ Build from Source

Requirements:
- Windows 10 or 11
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download)

**Steps:**
1. Clone the repository:
   ```cmd
   git clone https://github.com/YOUR_USERNAME/Windows-Duo.git
   cd Windows-Duo
   ```
2. Build and run it directly:
   ```cmd
   dotnet run
   ```
3. Or, publish it as a single standalone `.exe` file (the executable will appear in `bin/Release/net8.0-windows10.0.19041.0/win-x64/publish/`):
   ```cmd
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```

## 🧠 How it Works

Unlike some modern MacBooks, standard Windows laptops don't possess a sensor that tracks the *continuous* physical angle of the hinge. They only report simple "Open" or "Closed" states. 

To bypass this hardware limitation, **Windows-Duo** intercepts the ACPI lid switch event (`GUID_LIDSWITCH_STATE_CHANGE`) mere milliseconds before the OS puts the computer to sleep. It captures the screen frame, maps it as a texture onto a Windows Presentation Foundation (WPF) `Viewport3D` plane, and animates a smooth rotation and blur sequence before yielding back to the sleep cycle.

## 🤝 Contributing

Pull requests and stars are always welcome! If you have a 2-in-1 device (like a Surface Laptop Studio) and want to help implement real-time continuous hinge tracking using the `HingeAngleSensor` API, feel free to open an issue!

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
