#!/bin/sh
export artifacts=$(dirname "$(readlink -f "$0")")/artifacts
export configuration=Release

dotnet restore NLog.StructuredLogging.Json.sln --verbosity minimal || exit 1
dotnet build src/NLog.StructuredLogging.Json/NLog.StructuredLogging.Json.csproj --output $artifacts --configuration $configuration --framework "netstandard1.3" || exit 1
dotnet test src/NLog.StructuredLogging.Json.Tests/NLog.StructuredLogging.Json.Tests.csproj --configuration Debug --framework "netcoreapp1.0" --filter "TestCategory!=CallSite&TestCategory!=NotInNetCore" || exit 1
