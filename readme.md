# Keycloak OAuth2 .NET API

![GitHub repo size](https://img.shields.io/github/repo-size/hikmetkutuk/keycloak-oauth2?color=inactive&logo=github&style=for-the-badge)

![.NetCore](https://img.shields.io/static/v1?&logo=.net&label=.NET&message=10&color=5c2d91&style=for-the-badge)

![Keycloak](https://img.shields.io/static/v1?&logo=JSON%20web%20tokens&label=Keycloak&message=26.0.0&color=E8336B&style=for-the-badge)

![PostgreSQL](https://img.shields.io/static/v1?logo=postgresql&label=PostgreSQL&message=17&color=336791&style=for-the-badge)

![Docker](https://img.shields.io/static/v1?logo=docker&label=Docker&message=29&color=086dd7&style=for-the-badge)

## Proje Açıklaması

Bu proje, Keycloak OAuth2 ile entegre edilmiş bir .NET 10 ASP.NET Core Web API uygulamasıdır. JWT token doğrulaması yapılmakta olup, Swagger/OpenAPI ile OAuth2 PKCE akışı desteklenmektedir.

## Özellikler

- ✅ .NET 10 + ASP.NET Core
- ✅ Keycloak 26.0.0 ile OAuth2/OpenID Connect entegrasyonu
- ✅ JWT Bearer token doğrulaması
- ✅ Swagger UI + OAuth2 PKCE akışı
- ✅ OpenTelemetry entegrasyonu
- ✅ Aspire Dashboard desteği
- ✅ Docker Compose ile lokal geliştirme ortamı
- ✅ PostgreSQL veri tabanı

## Kurulum

### Gereksinimler

- Docker ve Docker Compose
- .NET 10 SDK (lokal çalıştırma için)

### Başlangıç

1. Docker containerları başlatın:
```bash
docker-compose up -d
```

Servisler şu portlarda çalışır:
- **API**: http://localhost:8080 (Swagger: http://localhost:8080/swagger)
- **Keycloak**: http://localhost:8081 (Kullanıcı: admin / Şifre: admin)
- **Aspire Dashboard**: http://localhost:18888

2. Lokal olarak çalıştırma (opsiyonel):
```bash
cd src/KeycloakOAuthApi
dotnet run
```

## Yapılandırma

Keycloak ayarları `docker-compose.yml` içinde konfigüre edilmiştir:
- İssuers: `http://localhost:8081/realms/auth`
- Metadata: `http://keycloak:8080/realms/auth/.well-known/openid-configuration`

## Endpoints

- `GET /swagger` - Swagger UI (OAuth2 desteğiyle)
- `GET /users/me` - Kimlik doğrulanmış kullanıcı bilgilerini döndürür (Yetkilendirme gerekli)

## Teknolojiler

- Microsoft.AspNetCore.Authentication.JwtBearer
- Swashbuckle.AspNetCore (Swagger)
- OpenTelemetry
- .NET 10 / ASP.NET Core 10