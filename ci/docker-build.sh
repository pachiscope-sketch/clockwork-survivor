#!/usr/bin/env bash
set -Eeuo pipefail

PROJECT_PATH="${PROJECT_PATH:-/workspace}"
BUILD_TARGET="${BUILD_TARGET:-WebGL}"
BUILD_PATH="${BUILD_PATH:-/workspace/Builds/WebGL}"
BUILD_LOG="${BUILD_LOG:-/workspace/Logs/docker-build.log}"
UNITY_VERSION="${UNITY_VERSION:-6000.4.7f1}"

find_unity() {
  if [[ -n "${UNITY_EXECUTABLE:-}" && -x "${UNITY_EXECUTABLE}" ]]; then
    printf '%s\n' "${UNITY_EXECUTABLE}"
    return 0
  fi

  local candidates=(
    "/opt/unity/editors/${UNITY_VERSION}/Editor/Unity"
    "/opt/unity/Editor/Unity"
    "/opt/Unity/Editor/Unity"
  )

  for candidate in "${candidates[@]}"; do
    if [[ -x "${candidate}" ]]; then
      printf '%s\n' "${candidate}"
      return 0
    fi
  done

  command -v unity-editor || command -v Unity
}

install_license() {
  if [[ -z "${UNITY_LICENSE:-}" ]]; then
    echo "UNITY_LICENSE is not set. Provide a Unity .ulf license file as plain text or base64 content."
    return 0
  fi

  local license_dir="${HOME}/.local/share/unity3d/Unity"
  local license_file="${license_dir}/Unity_lic.ulf"
  local decoded_file="${license_file}.decoded"
  mkdir -p "${license_dir}"

  if printf '%s' "${UNITY_LICENSE}" | base64 -d > "${decoded_file}" 2>/dev/null; then
    mv "${decoded_file}" "${license_file}"
  else
    rm -f "${decoded_file}"
    printf '%s' "${UNITY_LICENSE}" > "${license_file}"
  fi

  echo "Unity license file prepared at ${license_file}"
}

UNITY_BIN="$(find_unity)"
mkdir -p "$(dirname "${BUILD_LOG}")"
install_license

echo "Unity executable: ${UNITY_BIN}"
echo "Project path: ${PROJECT_PATH}"
echo "Build target: ${BUILD_TARGET}"
echo "Build path: ${BUILD_PATH}"

set +e
"${UNITY_BIN}" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "${PROJECT_PATH}" \
  -buildTarget "${BUILD_TARGET}" \
  -executeMethod ClockworkSurvivor.EditorTools.CiBuild.Build \
  -logFile "${BUILD_LOG}"
unity_exit=$?
set -e

if [[ -f "${BUILD_LOG}" ]]; then
  echo "Last Unity log lines:"
  tail -n 120 "${BUILD_LOG}"
fi

if [[ ${unity_exit} -ne 0 ]]; then
  echo "Unity build failed with exit code ${unity_exit}"
  exit "${unity_exit}"
fi

echo "Unity build finished."
if [[ -d "${BUILD_PATH}" ]]; then
  find "${BUILD_PATH}" -maxdepth 2 -type f | sort
elif [[ -f "${BUILD_PATH}" ]]; then
  ls -lh "${BUILD_PATH}"
fi
