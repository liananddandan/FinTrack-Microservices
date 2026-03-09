# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy solution
COPY FinTrack.sln ./

# copy project files for restore cache
COPY src/Services/AuditLogService/AuditLogService.csproj src/Services/AuditLogService/
COPY src/Shared/SharedKernel/SharedKernel.csproj src/Shared/SharedKernel/

# restore
RUN dotnet restore src/Services/AuditLogService/AuditLogService.csproj

# copy all source
COPY . .

# publish
RUN dotnet publish src/Services/AuditLogService/AuditLogService.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "AuditLogService.dll"]