# 🛒 WebApiShop — REST API with .NET 9

A production-ready RESTful Web API for an online shop, built with **C#** and **.NET 9**, designed around clean architecture and solid backend engineering practices.

---

## 🏗️ Project Architecture

The codebase is divided into **3 layers** that communicate through abstractions:

| Layer | Role |
|---|---|
| **Application** | Entry point — controllers, middleware, configuration |
| **Services** | Business logic — orchestrates operations and domain rules |
| **Repositories** | Database access — abstracts all EF Core queries |

All layers are wired together using ASP.NET Core's built-in **Dependency Injection**, so each layer depends only on interfaces — not on concrete implementations.

---

## ✨ Features

### 🌐 REST API
Clean, resource-oriented endpoints following REST conventions — correct HTTP methods, meaningful status codes, and consistent response shapes.

### 🗃️ Entity Framework Core
The project uses **EF Core** as the ORM to interact with the database using strongly typed C# instead of raw SQL.

### ⚡ Fully Async
Every database call is made **asynchronously** using `async/await`, keeping threads free and the API responsive under concurrent load.

### 🔄 DTO Layer — Records + AutoMapper
- A dedicated **DTO layer** sits between the API and the data model, preventing internal entities from leaking into responses.
- All DTOs are written as **C# `record` types** — concise, immutable, and perfect for read-only data transfer.
- **AutoMapper** handles all conversions between entities and DTOs automatically.

### 🔧 External Configuration
Sensitive and environment-specific settings are stored in `appsettings.json` and `appsettings.Development.json`, never hardcoded.

### 📝 Structured Logging — NLog
**NLog** is configured in `nlog.config` and integrated across all layers to produce structured, readable logs for every request and operation.

### 🚨 Global Error Handling Middleware
A centralized `ErrorHandlingMiddleware` intercepts all unhandled exceptions, returning consistent error responses and keeping internal details private.

### 📈 Request Tracking
Every incoming request is logged to a **Rating table**, giving full visibility into API usage patterns and traffic over time.

### 🧪 Unit & Integration Tests
Testing is done with **xUnit** and covers:
- **Unit tests** — individual classes tested in isolation.
- **Integration tests** — full request-to-database flows tested with a shared `DataBaseFixture`.

Tested entities: `Category`, `Product`, `Order`, `User`.

---

## 🛠️ Tech Stack

| Technology | Role |
|---|---|
| .NET 9 / C# | Runtime & language |
| ASP.NET Core | Web API framework |
| Entity Framework Core | ORM & migrations |
| AutoMapper | Object mapping |
| NLog | Logging |
| xUnit | Testing |
| DI (built-in) | Layer decoupling |

---

## 📂 Project Structure

```
WebApiShop.sln
├── WebApiShop/           # Controllers, middleware, AutoMapper, Program.cs
├── Services/             # Business logic & service interfaces
├── Repositories/         # EF Core context, repositories & interfaces
├── Entities/             # EF Core domain models
├── DTOs/                 # C# record DTOs
└── TestProject/          # xUnit unit & integration tests
```

---

## 🚀 Getting Started

### Requirements
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server (or any EF Core-supported database)

### Run the API

```bash
dotnet restore
dotnet ef database update --project Repositories
dotnet run --project WebApiShop
```

### Run Tests

```bash
dotnet test
```

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
