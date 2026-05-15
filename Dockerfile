# ── Stage 1: Build ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore dependencies trước (cache layer)
COPY SaigonAudioTour.Api/SaigonAudioTour.Api.csproj SaigonAudioTour.Api/
RUN dotnet restore SaigonAudioTour.Api/SaigonAudioTour.Api.csproj

# Copy source và publish
COPY SaigonAudioTour.Api/ SaigonAudioTour.Api/
RUN dotnet publish SaigonAudioTour.Api/SaigonAudioTour.Api.csproj \
    -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Tạo thư mục uploads cho file storage
RUN mkdir -p /app/uploads

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SaigonAudioTour.Api.dll"]
