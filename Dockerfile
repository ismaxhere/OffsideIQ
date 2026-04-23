FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/OffsideIQ.API/OffsideIQ.API.csproj", "OffsideIQ.API/"]
COPY ["src/OffsideIQ.Core/OffsideIQ.Core.csproj", "OffsideIQ.Core/"]
COPY ["src/OffsideIQ.Infrastructure/OffsideIQ.Infrastructure.csproj", "OffsideIQ.Infrastructure/"]
COPY ["src/OffsideIQ.Application/OffsideIQ.Application.csproj", "OffsideIQ.Application/"]
RUN dotnet restore "OffsideIQ.API/OffsideIQ.API.csproj"
COPY src/ .
WORKDIR "/src/OffsideIQ.API"
RUN dotnet build "OffsideIQ.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OffsideIQ.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OffsideIQ.API.dll"]
