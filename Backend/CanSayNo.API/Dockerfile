# CanSayNo API — üretim imajı. Bağlam: Backend/ klasörü
#   docker build -f CanSayNo.API/Dockerfile -t cansayno-api .
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Önce yalnızca proje dosyaları → restore katmanı kod değişmedikçe önbellekten gelir
COPY CanSayNo.API/CanSayNo.API.csproj CanSayNo.API/
COPY CanSayNo.Application/CanSayNo.Application.csproj CanSayNo.Application/
COPY CanSayNo.Domain/CanSayNo.Domain.csproj CanSayNo.Domain/
COPY CanSayNo.Infrastructure/CanSayNo.Infrastructure.csproj CanSayNo.Infrastructure/
RUN dotnet restore CanSayNo.API/CanSayNo.API.csproj

COPY CanSayNo.API/ CanSayNo.API/
COPY CanSayNo.Application/ CanSayNo.Application/
COPY CanSayNo.Domain/ CanSayNo.Domain/
COPY CanSayNo.Infrastructure/ CanSayNo.Infrastructure/
RUN dotnet publish CanSayNo.API/CanSayNo.API.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
# aspnet imajı zaten 8080'i dinler (HTTP_PORTS). Küçük sunucuda daha az bellek: workstation GC
ENV DOTNET_gcServer=0
EXPOSE 8080
# Görüntüdeki yetkisiz kullanıcı (root değil)
USER $APP_UID
ENTRYPOINT ["dotnet", "CanSayNo.API.dll"]
