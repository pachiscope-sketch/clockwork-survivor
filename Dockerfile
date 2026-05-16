# syntax=docker/dockerfile:1

FROM unityci/hub:ubuntu-3.1.0

ARG UNITY_VERSION=6000.4.7f1
ARG UNITY_CHANGESET=f3c3c4248748
ARG UNITY_MODULES=webgl

ENV UNITY_VERSION=${UNITY_VERSION} \
    UNITY_CHANGESET=${UNITY_CHANGESET} \
    PROJECT_PATH=/workspace \
    BUILD_TARGET=WebGL \
    BUILD_PATH=/workspace/Builds/WebGL \
    BUILD_LOG=/workspace/Logs/docker-build.log \
    UNITY_THISISABUILDMACHINE=1

RUN unity-hub install --version "${UNITY_VERSION}" --changeset "${UNITY_CHANGESET}" --module ${UNITY_MODULES}

WORKDIR /workspace
COPY . /workspace

RUN chmod +x /workspace/ci/docker-build.sh

ENTRYPOINT ["/workspace/ci/docker-build.sh"]
