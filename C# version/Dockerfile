# Copyright 2021 Google LLC
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#      http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

# https://mcr.microsoft.com/v2/dotnet/sdk/tags/list
FROM mcr.microsoft.com/dotnet/sdk:6.0.201 as builder

ARG RM_DEV_SL_TOKEN=""
ARG IS_PR=""
ARG TARGET_BRANCH=""
ARG LATEST_COMMIT=""
ARG PR_NUMBER=""
ARG TARGET_REPO_URL=""
ARG BUILD_NAME=""

ENV RM_DEV_SL_TOKEN ${RM_DEV_SL_TOKEN}
ENV IS_PR ${IS_PR}
ENV TARGET_BRANCH ${TARGET_BRANCH}
ENV LATEST_COMMIT ${LATEST_COMMIT}
ENV PR_NUMBER ${PR_NUMBER}
ENV TARGET_REPO_URL ${TARGET_REPO_URL}
ENV BUILD_NAME ${BUILD_NAME}

RUN echo "========================================================="
RUN echo "targetBranch: ${TARGET_BRANCH}"
RUN echo "latestCommit: ${LATEST_COMMIT}"
RUN echo "pullRequestNumber ${PR_NUMBER}"
RUN echo "repositoryUrl ${TARGET_REPO_URL}"
RUN echo "========================================================="

WORKDIR /NotificationSender
COPY /NotificationSender/NotificationSender/NotificationSender.csproj .
RUN dotnet restore NotificationSender.csproj -r linux-musl-x64
COPY /NotificationSender/NotificationSender/ .
RUN dotnet publish NotificationSender.csproj -p:PublishSingleFile=true -r linux-musl-x64 --self-contained true -p:PublishTrimmed=True -p:TrimMode=Link -c release -o /notificationsender --no-restore

WORKDIR /tests
COPY ./NotificationSender/NotificationSenderTest/NotificationSenderTest.csproj .
RUN dotnet restore NotificationSenderTest.csproj -r linux-musl-x64
COPY ./NotificationSender/NotificationSenderTest/ .
RUN dotnet build

WORKDIR /notificationsender
ADD https://sl-repo-dev.s3.us-east-1.amazonaws.com/sealights-dotnet-agent-3.0.1-beta.hotfix-portable.tar.gz sealights-dotnet-agent-3.0.1-beta.hotfix-portable.tar.gz
RUN tar -xvzf sealights-dotnet-agent-3.0.1-beta.hotfix-portable.tar.gz
RUN mv -v /notificationsender/sealights-dotnet-agent-3.0.1-beta.hotfix-portable/* /notificationsender/
RUN rm sealights-dotnet-agent-3.0.1-beta.hotfix-portable.tar.gz

RUN if [ $IS_PR = 0 ]; then \
    echo "Check-in to repo"; \
    dotnet SL.DotNet.dll config --token $RM_DEV_SL_TOKEN --appName notificationsender --includedAssemblies "NotificationSender*" --branchName master \ 
    	--buildName $BUILD_NAME --includeNamespace "NotificaitonSender.*" --excludeNamespace Microsoft ; \
else \ 
    echo "Pull request"; \
    dotnet SL.DotNet.dll prConfig --token $RM_DEV_SL_TOKEN --appname notificationsender --includedAssemblies "NotificationSender*" --targetBranch "${TARGET_BRANCH}" \
    	--includeNamespace "NotificaitonSender.*" --excludeNamespace Microsoft --latestCommit "${LATEST_COMMIT}" --pullRequestNumber "${PR_NUMBER}" --repositoryUrl "${TARGET_REPO_URL}" ; \
fi

RUN dotnet SL.DotNet.dll scan --buildSessionIdFile buildSessionId --binDir /tests/bin/Debug/net6.0 --token $RM_DEV_SL_TOKEN --ignoreGeneratedCode true

RUN dotnet SL.DotNet.dll startExecution --buildSessionIdFile buildSessionId --labid=integ_master_813e_SLBoutique --token $RM_DEV_SL_TOKEN --testStage "Unit Tests"
RUN dotnet SL.DotNet.dll testListener --buildSessionIdFile buildSessionId --labid=integ_master_813e_SLBoutique --token $RM_DEV_SL_TOKEN --workingDir /tests/bin/Debug/net6.0 --target dotnet --targetArgs " test NotificationSenderTest.dll " || true
RUN dotnet SL.DotNet.dll endExecution --buildSessionIdFile buildSessionId --labid=integ_master_813e_SLBoutique --token $RM_DEV_SL_TOKEN  --testStage "Unit Tests"

# https://mcr.microsoft.com/v2/dotnet/runtime-deps/tags/list
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.3-alpine3.15-amd64

ARG RM_DEV_SL_TOKEN=""

ENV RM_DEV_SL_TOKEN ${RM_DEV_SL_TOKEN}

WORKDIR /app

COPY --from=builder /notificationsender .

ENV ASPNETCORE_URLS http://*:7080
ENTRYPOINT ["/app/NotificationSender"]
