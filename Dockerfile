FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /App
COPY . ./
RUN dotnet publish osu.Game.Tests -c Release -o out

WORKDIR /App/out
ENTRYPOINT ["dotnet", "test", "osu.Game.Tests.dll"]