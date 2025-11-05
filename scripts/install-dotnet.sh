#!/bin/bash

# .NET SDK Installation Script for Fedora
# This script installs or fixes .NET SDK 8.0 installation

set -e

echo "=========================================="
echo ".NET SDK Installation Script"
echo "=========================================="
echo ""

# Check if running as root for package installation
if [ "$EUID" -ne 0 ]; then 
    echo "This script needs sudo privileges to install packages."
    echo "Please run with: sudo ./install-dotnet.sh"
    exit 1
fi

echo "Step 1: Removing old/corrupted .NET installations..."
echo ""

# Remove old dotnet installations
dnf remove -y dotnet* 2>/dev/null || true
rm -rf /usr/share/dotnet 2>/dev/null || true
rm -rf /usr/lib64/dotnet 2>/dev/null || true

echo "✓ Old installations removed"
echo ""

echo "Step 2: Adding Microsoft repository..."
echo ""

# Add Microsoft repository
rpm --import https://packages.microsoft.com/keys/microsoft.asc || true

# Add Microsoft repository for Fedora
cat > /etc/yum.repos.d/microsoft-prod.repo <<EOF
[microsoft-prod]
name=Microsoft Product Repository
baseurl=https://packages.microsoft.com/fedora/40/prod/
enabled=1
gpgcheck=1
gpgkey=https://packages.microsoft.com/keys/microsoft.asc
EOF

echo "✓ Microsoft repository added"
echo ""

echo "Step 3: Installing .NET SDK 8.0..."
echo ""

# Install .NET SDK 8.0
dnf install -y dotnet-sdk-8.0

echo ""
echo "✓ .NET SDK 8.0 installed"
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

