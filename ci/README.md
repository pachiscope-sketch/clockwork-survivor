# Docker Unity Build

This folder contains the CI/CD portfolio setup for building Clockwork Survivor in Docker.

## Local Docker Build

Build the Unity builder image:

```powershell
docker build -t clockwork-survivor-unity-builder .
```

Run a WebGL build. `UNITY_LICENSE` can be either raw `.ulf` text or base64-encoded license content:

```powershell
$env:UNITY_LICENSE = [Convert]::ToBase64String([IO.File]::ReadAllBytes("Unity_lic.ulf"))
docker run --rm `
  -e UNITY_LICENSE="$env:UNITY_LICENSE" `
  -e BUILD_TARGET=WebGL `
  -e BUILD_PATH=/workspace/Builds/WebGL `
  -v "${PWD}\Builds:/workspace/Builds" `
  clockwork-survivor-unity-builder
```

The WebGL output is written to `Builds/WebGL`.

## GitHub Actions

The workflow in `.github/workflows/docker-webgl-build.yml` builds the Docker image, runs the Unity WebGL build, and uploads the generated `Builds/WebGL` folder as an artifact.

Add a repository secret named `UNITY_LICENSE` before running the workflow.
