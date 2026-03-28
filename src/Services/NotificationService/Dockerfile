# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy solution
COPY FinTrack.sln ./

# copy project files for restore cache
COPY src/Services/NotificationService/NotificationService.csproj src/Services/NotificationService/
COPY src/Shared/SharedKernel/SharedKernel.csproj src/Shared/SharedKernel/

# restore
RUN dotnet restore src/Services/NotificationService/NotificationService.csproj

# copy full source
COPY . .

# publish
RUN dotnet publish src/Services/NotificationService/NotificationService.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "NotificationService.dll"]