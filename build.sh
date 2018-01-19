#!/usr/bin/env bash

root=$(cd "$(dirname "$0")"; pwd -P)
artifacts=$root/artifacts
configuration=Release

export CLI_VERSION="2.1.4"
export DOTNET_INSTALL_DIR="$root/.dotnetcli"
export PATH="$DOTNET_INSTALL_DIR:$PATH"

dotnet_version=$(dotnet --version)

if [ "$dotnet_version" != "$CLI_VERSION" ]; then
    curl -sSL https://raw.githubusercontent.com/dotnet/cli/release/2.0.0/scripts/obtain/dotnet-install.sh | bash /dev/stdin --version "$CLI_VERSION" --install-dir "$DOTNET_INSTALL_DIR"
fi

dotnet restore NLog.StructuredLogging.Json.sln --verbosity minimal || exit 1
dotnet build src/NLog.StructuredLogging.Json/NLog.StructuredLogging.Json.csproj --output $artifacts --configuration $configuration --framework "netstandard2.0" || exit 1

dotnet test ./src/NLog.StructuredLogging.Json.Tests/NLog.StructuredLogging.Json.Tests.csproj --configuration Debug --framework "netcoreapp2.0" --filter "TestCategory!=NotInNetCore" || exit 1