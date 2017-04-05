#!/bin/sh
export artifacts=$(dirname "$(readlink -f "$0")")/artifacts
export configuration=Release

dotnet restore NLog.StructuredLogging.Json.sln --verbosity minimal || exit 1
dotnet build src/NLog.StructuredLogging.Json/NLog.StructuredLogging.Json.csproj --output $artifacts --configuration $configuration --framework "netstandard1.3" || exit 1
dotnet build src/NLog.StructuredLogging.Json.Tests/NLog.StructuredLogging.Json.Tests.csproj --output $artifacts --configuration $configuration --framework "netcoreapp1.0" || exit 1
dotnet run --project src/NLog.StructuredLogging.Json.Tests/NLog.StructuredLogging.Json.Tests.csproj --configuration $configuration --framework "netcoreapp1.0" || exit 1

