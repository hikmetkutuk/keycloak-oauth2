# Keycloak OAuth2 .NET API

![GitHub repo size](https://img.shields.io/github/repo-size/hikmetkutuk/keycloak-oauth2?color=inactive&logo=github&style=for-the-badge)
![.NetCore](https://img.shields.io/static/v1?&logo=.net&label=.NET&message=10&color=5c2d91&style=for-the-badge)
![Keycloak](https://img.shields.io/static/v1?&logo=JSON%20web%20tokens&label=Keycloak&message=26.0.0&color=E8336B&style=for-the-badge)
![PostgreSQL](https://img.shields.io/static/v1?logo=postgresql&label=PostgreSQL&message=17&color=336791&style=for-the-badge)
![Docker](https://img.shields.io/static/v1?logo=docker&label=Docker&message=29&color=086dd7&style=for-the-badge)

## Proje Açıklaması

Bu proje, Keycloak OAuth2/OpenID Connect ile entegre edilmiş bir .NET 10 ASP.NET Core Web API örneğidir. Uygulama; JWT token doğrulaması, Swagger UI üzerinden PKCE ile login, role bazlı yetkilendirme ve gerçek iş endpointleri (products/orders) içerir.

## Özellikler

- ✅ .NET 10 + ASP.NET Core
- ✅ Keycloak 26.0.0 ile OAuth2/OpenID Connect entegrasyonu
- ✅ JWT Bearer token doğrulaması
- ✅ Swagger UI + OAuth2 PKCE akışı
- ✅ Role bazlı authorization (`admin`, `user`)
- ✅ Orta seviye örnek API endpointleri (`products`, `orders`)
- ✅ OpenTelemetry entegrasyonu
- ✅ Aspire Dashboard desteği
- ✅ Docker Compose ile lokal geliştirme ortamı
- ✅ PostgreSQL veri tabanı
- ✅ Demo amaçlı in-memory ürün/sipariş store
- ✅ Realm import ile otomatik Keycloak başlangıç kurulumu

## Kurulum

### Gereksinimler

- Docker ve Docker Compose
- .NET 10 SDK (lokal çalıştırma için)

### Başlangıç

1. Docker containerları başlatın:
```bash
docker-compose up -d
```

`8080` portu doluysa API'yi farklı portta başlatabilirsiniz:
```bash
API_HTTP_PORT=8082 docker-compose up -d
```

Servisler şu portlarda çalışır:
- **API**: http://localhost:8080 (Swagger: http://localhost:8080/swagger)
- **Keycloak**: http://localhost:8081 (Kullanıcı: admin / Şifre: admin)
- **Aspire Dashboard**: http://localhost:18888

2. Swagger üzerinden login için örnek kullanıcılar:
- `demo / demo123` (user rolü)
- `admin / admin123` (admin + user rolü)

3. Lokal olarak API'yi IDE/CLI ile çalıştırma (opsiyonel):
```bash
cd src/KeycloakOAuthApi
dotnet run
```

## API Yetenekleri

### Kimlik ve Yetki
- Tüm `api/*` endpointleri kimlik doğrulaması ister.
- `admin` rolü gerektiren endpointler:
  - `POST /api/products`
  - `PATCH /api/products/{id}/stock`
  - `GET /api/orders`

### İş Endpointleri
- `GET /api/products` - Ürünleri listeler.
- `GET /api/products/{id}` - Tek ürün döner.
- `POST /api/products` - Yeni ürün oluşturur (`admin`).
- `PATCH /api/products/{id}/stock` - Stok günceller (`admin`).
- `POST /api/orders` - Giriş yapan kullanıcı için sipariş oluşturur.
- `GET /api/orders/me` - Kendi siparişlerini listeler.
- `GET /api/orders/me/{id}` - Kendi tek siparişini döner.
- `GET /api/orders` - Tüm siparişler (`admin`).
- `GET /users/me` - Token claim introspection endpointi.
- `GET /health` - Basit sağlık kontrolü.

## Swagger ile Uçtan Uca Test

1. `http://localhost:8080/swagger` adresine gidin.
2. `Authorize` butonuna basın.
3. Keycloak login ekranında `demo/demo123` veya `admin/admin123` ile giriş yapın.
4. `GET /api/products` çağrısı yapın.
5. `admin` ile giriş yaptıysanız `POST /api/products` ve `GET /api/orders` endpointlerini de test edin.

## CLI ile Uçtan Uca Test

`cli-client` sadece lokal smoke test için eklenmiştir (password grant).

1. Demo token alın:
```bash
curl -sS -X POST "http://localhost:8081/realms/auth/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=cli-client" \
  -d "grant_type=password" \
  -d "username=demo" \
  -d "password=demo123" \
  -d "scope=openid profile" | jq -r '.access_token'
```

2. Admin token alın:
```bash
curl -sS -X POST "http://localhost:8081/realms/auth/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=cli-client" \
  -d "grant_type=password" \
  -d "username=admin" \
  -d "password=admin123" \
  -d "scope=openid profile" | jq -r '.access_token'
```

3. Beklenen davranış:
- `GET /api/products` (demo token) -> `200`
- `POST /api/products` (demo token) -> `403`
- `POST /api/products` (admin token) -> `201`
- `POST /api/orders` (demo token) -> `201`
- `GET /api/orders` (admin token) -> `200`

## Yapılandırma Notları

- API, token doğrulaması için:
  - `Authentication:Issuer = http://localhost:8081/realms/auth`
  - `Authentication:MetadataAddress = http://localhost:8081/realms/auth/.well-known/openid-configuration`
- Docker Compose içinde API container'ı metadata'yı iç ağdan alır:
  - `Authentication__MetadataAddress = http://keycloak:8080/realms/auth/.well-known/openid-configuration`
- `keycloak/import/auth-realm.json` dosyası başlangıçta otomatik import edilir.
- Realm içinde iki client tanımlıdır:
  - `public-client` (Swagger + Authorization Code + PKCE)
  - `cli-client` (lokal CLI smoke test için password grant)

## Teknoloji Stack

- ASP.NET Core 10
- Microsoft.AspNetCore.Authentication.JwtBearer
- Swashbuckle.AspNetCore
- OpenTelemetry
- Keycloak 26
- PostgreSQL 17
