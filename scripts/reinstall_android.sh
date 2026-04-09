#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="/Users/admin/Code/LGBTOUR"
PROJECT_PATH="$REPO_ROOT/SaigonAudioTour.Mobile/SaigonAudioTour.Mobile.csproj"
PACKAGE_ID="com.companyname.saigonaudiotour.mobile"
if ! adb get-state >/dev/null 2>&1; then
  echo "No Android device/emulator is connected. Start emulator-5554 first."
  exit 1
fi

DEVICE_ID="$(adb devices | awk 'NR > 1 && $2 == "device" { print $1; exit }')"
if [[ -z "$DEVICE_ID" ]]; then
  echo "No Android device is in 'device' state. Open the emulator and wait until it finishes booting."
  exit 1
fi

adb uninstall "$PACKAGE_ID" >/dev/null 2>&1 || true

dotnet build -t:Run \
  -p:Configuration=Debug \
  -f net10.0-android \
  -p:AdbTarget="-s $DEVICE_ID" \
  -p:AndroidAttachDebugger=false \
  -p:EnableDiagnostics=false \
  -p:EnableMauiXamlDiagnostics=false \
  "$PROJECT_PATH"
