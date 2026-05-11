# WebApiShop — Copilot Instructions

## What the App Does

WebApiShop is a production-style RESTful Web API for an online shop. It manages **Products**, **Categories**, **Orders**, **OrderItems**, and **Users**, with JWT authentication, request rate limiting, and full request tracking via a `Rating` table.

---

## Tech Stack

| Area | Technology |
|---|---|
| Runtime | .NET 9 / C# |
| Web Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core (SQL Server) |
| Object Mapping | AutoMapper |
| Logging | Serilog (console + rolling file to `Logs/`) |
| Auth | JWT Bearer (configured in `JwtSettings` in `appsettings.json`) |
| Testing | xUnit + Moq + Moq.EntityFrameworkCore |

> **Focused guidelines:** see `.github/instructions/repositories.instructions.md` for repository patterns and `.github/instructions/controllers.instructions.md` for controller patterns.

---

## Project Structure

```
WebApiShop.sln
├── WebApiShop/         # Entry point: Controllers, Middleware, AutoMapper profile, Program.cs
├── Services/           # Business logic; everything goes through service interfaces
├── Repositories/       # EF Core context (ApiShopContext) + repository implementations
├── Entities/           # EF Core entity classes (auto-generated via EF Core Power Tools)
├── DTOs/               # C# record types used as request/response shapes
└── TestProject/        # xUnit unit & integration tests
```

---

## Architecture Rules

1. **Always code to interfaces.** Controllers depend on `IXxxServices`; services depend on `IXxxRepository`. Never reference concrete classes across layer boundaries.
2. **DTOs are C# records.** Add new DTOs in `DTOs/` as `public record MyDTO(...)`.
3. **Entities are auto-generated** by EF Core Power Tools — use `partial class` extensions rather than editing them directly.
4. **Register all new dependencies in `Program.cs`** with `builder.Services.AddScoped<IFoo, Foo>()`.
5. **Add AutoMapper mappings** in `WebApiShop/AutoMapper.cs` using `CreateMap<Entity, DTO>().ReverseMap()`.
6. **All database access is async** — use `async Task<T>` / `await`; never `.Result` or `.Wait()`.
7. **Logging uses Serilog** — inject `ILogger<T>` from `Microsoft.Extensions.Logging`.

---

## Adding a New Resource (order matters)

1. Entity → `Entities/SupplierEntity.cs`
2. DbSet → `Repositories/ApiShopContext.cs`
3. DTO → `DTOs/SupplierDTO.cs` (record type)
4. Repository interface + implementation → `Repositories/`
5. Service interface + implementation → `Services/`
6. Register both in `Program.cs` (AddScoped)
7. AutoMapper mapping → `WebApiShop/AutoMapper.cs`
8. Controller → `WebApiShop/Controllers/SupplierController.cs`
9. Unit tests → `TestProject/`

---

## Key Configuration Notes

- **Connection string key is `"DefaultConnection"`** — `appsettings.json` currently only has `"Home"` and `"Studies"` keys; add `"DefaultConnection"` to run locally.
- **JWT settings** — `JwtSettings:SecretKey`, `Issuer`, and `Audience` must all be set.
- **Redis** — `ProductsServices` requires `IConnectionMultiplexer`; register with `builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(...))`.
- **Rate limiting** — configured under `RateLimiting:RequestLimit` and `RateLimiting:TimeWindowMinutes`.

## File Naming Quirks

- `WebApiShop/Controllers/ProductsController .cs` — trailing **space** in filename.
- Several files in `Repositories/` and `Services/` have **invisible Unicode RTL mark characters** prepended. Search by content, not filename.

---

## Build & Run Commands

```powershell
dotnet restore
dotnet ef database update --project Repositories
dotnet run --project WebApiShop
dotnet test
```

> **Known pre-existing build issues:** `WebApiShop.csproj` is missing `Serilog` and `Microsoft.AspNetCore.Authentication.JwtBearer` package references, so solution-level `dotnet build` fails. Build individual projects to validate your changes: `dotnet build Repositories/Repositories.csproj` or `dotnet build Services/Services.csproj`.

> Integration tests require a live SQL Server at `srv2\pupils` — run unit tests only with `dotnet test --filter "FullyQualifiedName!~Integration"`.
