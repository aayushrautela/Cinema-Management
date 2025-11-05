#!/bin/bash

# Alternative .NET SDK Installation Script
# Uses Microsoft's official installation method

set -e

echo "=========================================="
echo ".NET SDK Installation (Microsoft Method)"
echo "=========================================="
echo ""

echo "Step 1: Downloading .NET SDK 8.0..."
echo ""

# Download .NET SDK 8.0 for Linux x64
cd /tmp
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh

echo "✓ Download complete"
echo ""

echo "Step 2: Installing .NET SDK 8.0..."
echo ""

# Install .NET SDK 8.0 to /usr/share/dotnet
sudo ./dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet

echo ""
echo "✓ Installation complete"
echo ""

echo "Step 3: Creating symlinks..."
echo ""

# Create symlinks if needed
sudo ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet || true

echo "✓ Symlinks created"
echo ""

echo "Step 4: Verifying installation..."
echo ""

# Verify installation
dotnet --version

echo ""
echo "=========================================="
echo "Installation Complete!"
echo "=========================================="
echo ""
echo "You can now run:"
echo "  dotnet restore"
echo "  dotnet build"
echo "  dotnet run"
echo ""

