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

ARG RM_DEV_SL_TOKEN=local
ARG GH_PAT=local
ENV RM_DEV_SL_TOKEN ${RM_DEV_SL_TOKEN}
ENV GH_PAT ${GH_PAT}

RUN echo "RM_DEV_SL_TOKEN: ${RM_DEV_SL_TOKEN}"
RUN echo "GH_PAT: ${GH_PAT}"

ENTRYPOINT "echo end"