FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY Client.csproj .
RUN dotnet restore 
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "Client.dll"]