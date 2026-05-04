<div align="center">
<img src="images/phoebe.png" alt="phoebe" width="200"/>
<h1>Phoebe Editor Reloaded</h1>
</div>

# Tweak Graphics
With Simple GUI<br>
Made for potato spec community who loves play WuWa<br>

# Main Feature
- [x] Detect Engine.ini Automaticaly
- [x] Backup the Engine.ini setting
- [x] Can Uncap 60 FPS (need fullscreen)
- [x] Can Modify game resolution
- [x] Set Distance scale Object
- [x] Disable/Enable shadow
- And many more....

# How To Use

- Download [.NET 10.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-10.0.7-windows-x64-installer?cid=getdotnetcore)
- Download from [Release](https://github.com/Guunexpert/WuWa-GraphicsTweaker/releases) tab
- Place in your Wuthering Waves files

> ⚠️ Use at your own risk!

---

# Build from Source

### Requirements
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- Visual Studio 2022 or newer with **WPF workload** installed

### Steps
```bash
git clone https://github.com/Guunexpert/WuWa-GraphicsTweaker.git
cd WuWa-GraphicsTweaker
dotnet build
```

### Publish single exe
```bash
dotnet publish -c Release
```
Output: `bin\Release\net10.0-windows\win-x64\publish\`
