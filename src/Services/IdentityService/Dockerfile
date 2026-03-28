# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 复制 solution
COPY FinTrack.sln ./

# 复制项目文件（用于 restore 缓存）
COPY src/Services/IdentityService/IdentityService.csproj src/Services/IdentityService/
COPY src/Shared/SharedKernel/SharedKernel.csproj src/Shared/SharedKernel/

# restore
RUN dotnet restore src/Services/IdentityService/IdentityService.csproj

# 再复制全部源码
COPY . .

# publish
RUN dotnet publish src/Services/IdentityService/IdentityService.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "IdentityService.dll"]