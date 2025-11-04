# .NET SDK Installation Guide

## Problem

If you see this error:
```
Error: [/usr/lib64/dotnet/host/fxr] does not contain any version-numbered child folders
Failed to resolve libhostfxr.so [not found]. Error code: 0x80008083
```

This means .NET SDK is not properly installed or corrupted.

## Solution Options

### Option 1: Using DNF (Fedora Package Manager) - Recommended

```bash
# Remove old/corrupted installation
sudo dnf remove -y dotnet*

# Add Microsoft repository
sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc
sudo dnf config-manager --add-repo https://packages.microsoft.com/fedora/40/prod/

# Install .NET SDK 8.0
sudo dnf install -y dotnet-sdk-8.0

# Verify installation
dotnet --version
```

### Option 2: Using Installation Script

Run the automated installation script:

```bash
cd /home/aayush/egui1/CinemaTicketSystemCore
sudo ./scripts/install-dotnet.sh
```

This script will:
- Remove old .NET installations
- Add Microsoft repository
- Install .NET SDK 8.0
- Verify installation

### Option 3: Manual Installation (Microsoft Official)

```bash
# Download installer
cd /tmp
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh

# Install to system directory
sudo ./dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet

# Create symlink
sudo ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet

# Verify
dotnet --version
```

Or use the manual script:

```bash
cd /home/aayush/egui1/CinemaTicketSystemCore
./scripts/install-dotnet-manual.sh
```

### Option 4: User Installation (No sudo required)

```bash
# Install to user directory
./dotnet-install.sh --channel 8.0 --install-dir ~/.dotnet

# Add to PATH (add to ~/.bashrc)
export PATH=$PATH:$HOME/.dotnet

# Reload shell
source ~/.bashrc

# Verify
dotnet --version
```

## After Installation

Once .NET SDK is installed, run:

```bash
cd /home/aayush/egui1/CinemaTicketSystemCore
dotnet restore
dotnet build
dotnet run
```

## Troubleshooting

### Check if .NET is installed

```bash
which dotnet
dotnet --version
```

### Check installed runtimes

```bash
dotnet --list-runtimes
dotnet --list-sdks
```

### If still not working

```bash
# Check PATH
echo $PATH

# Check installation location
ls -la /usr/share/dotnet/
ls -la /usr/lib64/dotnet/
```

## Quick Fix Command

If you just need to fix the current installation:

```bash
sudo dnf reinstall dotnet-sdk-8.0
```

Or:

```bash
sudo dnf remove dotnet*
sudo dnf install dotnet-sdk-8.0
```

