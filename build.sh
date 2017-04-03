#!/bin/sh
export artifacts=$(dirname "$(readlink -f "$0")")/artifacts
export configuration=Release

dotnet restore JustEat.StatsD.sln --verbosity minimal || exit 1
dotnet build src/JustEat.StatsD/JustEat.StatsD.csproj --output $artifacts --configuration $configuration --framework "netstandard1.6" || exit 1
dotnet test src/JustEat.StatsD.Tests/JustEat.StatsD.Tests.csproj --output $artifacts --configuration $configuration --framework "netcoreapp1.0" || exit 1
