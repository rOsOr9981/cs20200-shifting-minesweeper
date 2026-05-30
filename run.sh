#!/usr/bin/env bash
set -e
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$DIR"

# On Linux, Avalonia needs X11 libraries that minimal distros don't ship by
# default. If we are on Linux and libICE.so.6 is missing, try to install the
# required packages via apt. This is a no-op on macOS and on Linux systems
# that already have the libraries.
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    if ! ldconfig -p 2>/dev/null | grep -q libICE.so.6; then
        echo "[run.sh] Installing X11 dependencies required by Avalonia..."
        sudo apt install -y \
            libice6 libsm6 libx11-6 libxext6 libxrandr2 \
            libxi6 libxcursor1 libxfixes3 libxrender1 libfontconfig1
    fi
fi

dotnet run
