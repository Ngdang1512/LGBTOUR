#!/usr/bin/env bash
set -euo pipefail

# Tự động cấu hình khi cắm máy Android thật qua USB:
# - adb reverse tcp:5117 tcp:5117 (để app gọi API localhost:5117 trên máy dev)
# - tùy chọn mở app lần đầu khi vừa nhận máy

API_PORT="${API_PORT:-5117}"
PACKAGE_ID="${PACKAGE_ID:-com.companyname.saigonaudiotour.mobile}"
POLL_SECONDS="${POLL_SECONDS:-2}"
AUTO_LAUNCH_APP="${AUTO_LAUNCH_APP:-1}"
ADB_BIN="${ADB_BIN:-}"

if [[ -z "$ADB_BIN" ]]; then
  if command -v adb >/dev/null 2>&1; then
    ADB_BIN="$(command -v adb)"
  elif [[ -x "/usr/local/bin/adb" ]]; then
    ADB_BIN="/usr/local/bin/adb"
  elif [[ -x "/opt/homebrew/bin/adb" ]]; then
    ADB_BIN="/opt/homebrew/bin/adb"
  fi
fi

if [[ -z "$ADB_BIN" || ! -x "$ADB_BIN" ]]; then
  echo "[usb-autobridge] adb not found. Set ADB_BIN or install Android platform-tools."
  exit 1
fi

echo "[usb-autobridge] started | adb=${ADB_BIN} | api_port=${API_PORT} | package=${PACKAGE_ID} | poll=${POLL_SECONDS}s"

previous_devices=""

while true; do
  connected_devices="$("$ADB_BIN" devices | awk 'NR>1 && $2=="device" {print $1}')"

  for device in ${connected_devices}; do
    # Bỏ qua emulator, chỉ xử lý máy thật qua USB/Wi-Fi ADB
    if [[ "$device" == emulator-* ]]; then
      continue
    fi

    # Luôn đảm bảo reverse hoạt động (idempotent)
    "$ADB_BIN" -s "$device" reverse "tcp:${API_PORT}" "tcp:${API_PORT}" >/dev/null 2>&1 || true

    # Nếu là máy mới vừa cắm: log + mở app 1 lần
    if ! printf '%s\n' "$previous_devices" | grep -Fxq "$device"; then
      echo "[usb-autobridge] new device: ${device} -> reverse tcp:${API_PORT}"

      if [[ "$AUTO_LAUNCH_APP" == "1" ]]; then
        "$ADB_BIN" -s "$device" shell monkey -p "$PACKAGE_ID" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
        echo "[usb-autobridge] launched app on ${device}"
      fi
    fi
  done

  previous_devices="$connected_devices"
  sleep "$POLL_SECONDS"
done
