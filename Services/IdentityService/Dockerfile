FROM mcr.microsoft.com/dotnet/aspnet:9.0.7 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Services/IdentityService/IdentityService.csproj", "Services/IdentityService/"]
RUN dotnet restore "Services/IdentityService/IdentityService.csproj"

COPY . .
WORKDIR "/src/Services/IdentityService"
RUN dotnet build "IdentityService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IdentityService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityService.dll"]