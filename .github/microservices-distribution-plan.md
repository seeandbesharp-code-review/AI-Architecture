# Microservices Distribution Plan — WebApiShop

## Current State (Monolith)

The app is a single ASP.NET Core Web API with layered architecture:

```
WebApiShop (entry point)
├── Services        → business logic
├── Repositories    → EF Core / SQL Server
├── Entities        → domain models
└── DTOs            → request/response shapes
```

All resources — **Products, Categories, Orders, OrderItems, Users, Ratings** — share one database (`NewApiShop`) and one runtime process.

---

## Target State (Microservices)

Split into **5 independent services**, each owning its data and exposing its own HTTP API.

```
┌─────────────────────┐   ┌─────────────────────┐
│   Product Service   │   │  Category Service   │
│  /api/products      │   │  /api/categories    │
│  DB: products_db    │   │  DB: categories_db  │
└─────────────────────┘   └─────────────────────┘

┌─────────────────────┐   ┌─────────────────────┐
│   Order Service     │   │   User Service      │
│  /api/orders        │   │  /api/users         │
│  DB: orders_db      │   │  DB: users_db       │
└─────────────────────┘   └─────────────────────┘

┌─────────────────────┐
│  Analytics Service  │
│  /api/ratings       │
│  DB: analytics_db   │
└─────────────────────┘

            ↑ all behind
    ┌───────────────────┐
    │    API Gateway    │  (e.g. YARP / Ocelot)
    │  auth, routing,   │
    │  rate limiting    │
    └───────────────────┘
```

---

## Service Breakdown

### 1. Product Service
| Item | Detail |
|---|---|
| Owns | `Product` entity, `ProductsRepository`, `ProductsServices` |
| Endpoints | `GET /api/products`, `GET /api/products/{id}`, `POST`, `PUT`, `DELETE` |
| Database | `products_db` — `Products` table |
| Depends on | Category Service (read-only, via HTTP for category name enrichment) |
| Caching | Redis — cache-aside on `GetProducts()` (already implemented) |

### 2. Category Service
| Item | Detail |
|---|---|
| Owns | `Category` entity, `CategoriesRepository`, `CategoriesServices` |
| Endpoints | `GET /api/categories`, `POST`, `PUT`, `DELETE` |
| Database | `categories_db` — `Categories` table |
| Depends on | None |

### 3. Order Service
| Item | Detail |
|---|---|
| Owns | `Order`, `OrderItem` entities, repositories, services |
| Endpoints | `GET /api/orders`, `GET /api/orders/{id}`, `POST`, `PUT`, `DELETE` |
| Database | `orders_db` — `Orders` + `Order_Item` tables |
| Depends on | User Service (validate user exists), Product Service (validate product + price) |
| Async events | Publishes `OrderPlaced` event → message broker (e.g. RabbitMQ / Azure Service Bus) |

### 4. User Service
| Item | Detail |
|---|---|
| Owns | `User` entity, `UserRepository`, `UserServices`, `PasswordServices` |
| Endpoints | `POST /api/users` (register), `GET /api/users/{id}`, `PUT`, `DELETE` |
| Endpoints | `POST /api/password/login` → issues JWT |
| Database | `users_db` — `Users` table |
| Auth | This service is the **JWT issuer** — all other services validate tokens |

### 5. Analytics Service
| Item | Detail |
|---|---|
| Owns | `Rating` entity, `RatingRepository`, `RatingService` |
| Endpoints | `GET /api/ratings` (internal only) |
| Database | `analytics_db` — `Ratings` table |
| Input | Listens for HTTP calls from `RatingMiddleware` (or consumes events from broker) |

---

## API Gateway

Replace the current per-service JWT + rate-limiting middleware with a centralized gateway:

| Responsibility | Current Location | Target Location |
|---|---|---|
| JWT validation | each service | API Gateway |
| Rate limiting (`RateLimitingMiddleware`) | WebApiShop | API Gateway |
| Request routing | monolith routing | Gateway routes table |
| HTTPS termination | each service | API Gateway |

Recommended: **YARP** (Yet Another Reverse Proxy) — native .NET 9, integrates with ASP.NET Core DI.

---

## Shared Concerns

| Concern | Approach |
|---|---|
| Shared DTOs | Publish a `WebApiShop.Contracts` NuGet package (or git submodule) containing shared record types |
| Service discovery | Kubernetes DNS or Consul |
| Distributed tracing | OpenTelemetry → Jaeger / Azure Monitor |
| Logging | Serilog → centralized sink (Seq / ELK) |
| Health checks | `app.MapHealthChecks("/health")` in every service |

---

## Migration Phases

### Phase 1 — Strangler Fig (no downtime)
1. Deploy an API Gateway in front of the existing monolith.
2. Route all traffic through the gateway (monolith is the single backend).

### Phase 2 — Extract User Service
- Lowest coupling; no foreign keys to other tables.
- Move `Users` table to `users_db`, update gateway routing.
- Validate JWT issuance works independently.

### Phase 3 — Extract Category Service
- No dependencies on other services.
- Update Product Service to call Category Service by HTTP for enrichment.

### Phase 4 — Extract Product Service
- Remove Redis dependency from monolith; keep it in Product Service only.
- Publish `ProductPriceChanged` event when price is updated.

### Phase 5 — Extract Order Service
- Consume `UserValidated` and `ProductValidated` events or make sync HTTP calls.
- Publish `OrderPlaced` event to message broker.

### Phase 6 — Extract Analytics Service
- Update `RatingMiddleware` to call Analytics Service endpoint instead of writing to DB directly.

### Phase 7 — Decommission Monolith

---

## Folder Structure per Service

Each extracted service follows the same internal layered structure as the monolith:

```
ProductService/
├── ProductService.sln
├── ProductService/         ← Controllers, Middleware, Program.cs
├── ProductService.Services/
├── ProductService.Repositories/
├── ProductService.Entities/
├── ProductService.DTOs/
└── ProductService.Tests/
```

---

## Docker Compose Skeleton

```yaml
services:
  gateway:
    image: webapishop-gateway
    ports: ["443:443"]

  product-service:
    image: webapishop-products
    environment:
      - ConnectionStrings__DefaultConnection=...
      - Redis__ConnectionString=redis:6379

  category-service:
    image: webapishop-categories

  order-service:
    image: webapishop-orders

  user-service:
    image: webapishop-users

  analytics-service:
    image: webapishop-analytics

  redis:
    image: redis:7-alpine

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
```
