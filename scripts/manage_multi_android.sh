#!/usr/bin/env bash
set -euo pipefail

# Quản lý nhiều thiết bị Android cùng lúc (ưu tiên máy thật).
# Lệnh:
#   ./scripts/manage_multi_android.sh deploy   # build 1 lần + cài + reverse + mở app trên tất cả máy thật
#   ./scripts/manage_multi_android.sh reverse  # set adb reverse cho tất cả máy thật
#   ./scripts/manage_multi_android.sh launch   # mở app trên tất cả máy thật
#   ./scripts/manage_multi_android.sh status   # liệt kê thiết bị đang online

REPO_ROOT="/Users/admin/Code/SaigonAudioTour"
PROJECT_PATH="$REPO_ROOT/SaigonAudioTour.Mobile/SaigonAudioTour.Mobile.csproj"
PACKAGE_ID="com.companyname.saigonaudiotour.mobile"
API_PORT="${API_PORT:-5117}"

ACTION="${1:-deploy}"

if ! command -v adb >/dev/null 2>&1; then
  echo "[multi-device] adb not found"
  exit 1
fi

if ! command -v dotnet >/dev/null 2>&1; then
  echo "[multi-device] dotnet not found"
  exit 1
fi

collect_devices() {
  adb devices | awk 'NR>1 && $2=="device" {print $1}' | awk '!/^emulator-/'
}

print_status() {
  local devices
  devices="$(collect_devices)"

  if [[ -z "$devices" ]]; then
    echo "[multi-device] Không có máy thật nào đang online"
    return 0
  fi

  echo "[multi-device] Máy thật đang online:"
  while IFS= read -r d; do
    [[ -z "$d" ]] && continue
    local model
    model="$(adb -s "$d" shell getprop ro.product.model 2>/dev/null | tr -d '\r')"
    echo "  - $d (${model:-unknown})"
  done <<< "$devices"
}

build_apk_arm64() {
  echo "[multi-device] Building ARM64 APK (Debug, no fast deployment)..."
  dotnet build "$PROJECT_PATH" \
    -f net10.0-android \
    -c Debug \
    -p:RuntimeIdentifier=android-arm64 \
    -p:AndroidUseFastDeployment=false \
    -p:AndroidUseSharedRuntime=false \
    -p:EmbedAssembliesIntoApk=true \
    -p:AndroidPackageFormat=apk
}

APK_PATH="$REPO_ROOT/SaigonAudioTour.Mobile/bin/Debug/net10.0-android/android-arm64/${PACKAGE_ID}-Signed.apk"

set_reverse_all() {
  local devices="$1"
  while IFS= read -r d; do
    [[ -z "$d" ]] && continue
    adb -s "$d" reverse "tcp:${API_PORT}" "tcp:${API_PORT}" >/dev/null 2>&1 || true
    echo "[multi-device] reverse ok: $d -> tcp:${API_PORT}"
  done <<< "$devices"
}

launch_all() {
  local devices="$1"
  while IFS= read -r d; do
    [[ -z "$d" ]] && continue
    adb -s "$d" shell monkey -p "$PACKAGE_ID" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
    echo "[multi-device] launched: $d"
  done <<< "$devices"
}

install_all() {
  local devices="$1"
  local success=0
  local failed=0

  while IFS= read -r d; do
    [[ -z "$d" ]] && continue
    echo "[multi-device] Installing on $d ..."

    adb -s "$d" uninstall "$PACKAGE_ID" >/dev/null 2>&1 || true

    if adb -s "$d" install -r "$APK_PATH" >/tmp/saigon_install_${d}.log 2>&1; then
      echo "[multi-device] install success: $d"
      success=$((success+1))
    else
      echo "[multi-device] install failed: $d"
      tail -n 4 "/tmp/saigon_install_${d}.log" || true
      failed=$((failed+1))
    fi
  done <<< "$devices"

  echo "[multi-device] install summary: success=$success failed=$failed"

  if [[ $success -eq 0 ]]; then
    return 1
  fi
}

devices="$(collect_devices)"

if [[ "$ACTION" == "status" ]]; then
  print_status
  exit 0
fi

if [[ -z "$devices" ]]; then
  echo "[multi-device] Không có máy thật nào đang online"
  exit 1
fi

case "$ACTION" in
  deploy)
    print_status
    build_apk_arm64

    if [[ ! -f "$APK_PATH" ]]; then
      echo "[multi-device] APK not found: $APK_PATH"
      exit 1
    fi

    install_all "$devices"
    set_reverse_all "$devices"
    launch_all "$devices"
    ;;
  reverse)
    print_status
    set_reverse_all "$devices"
    ;;
  launch)
    print_status
    launch_all "$devices"
    ;;
  *)
    echo "Usage: $0 [deploy|reverse|launch|status]"
    exit 1
    ;;
esac

echo "[multi-device] done"
