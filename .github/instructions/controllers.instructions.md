---
applyTo: "WebApiShop/Controllers/**"
---

# Controller Layer Guidelines

## Location: `WebApiShop/Controllers/`

Controllers are the API entry point. They receive HTTP requests, delegate to a service, and return appropriate HTTP responses.

## File Naming Quirk

`ProductsController .cs` has a **trailing space** in the filename — this is a known artifact, do not rename it.

## Rules

- Controllers depend **only on service interfaces** (`IXxxServices`), never on repositories or `DbContext` directly.
- Inject the service via constructor and store it as a private readonly field.
- Use attribute routing: `[Route("api/[controller]")]` + `[ApiController]` on the class.
- Return proper HTTP status codes:
  - `Ok(result)` — 200 success with body
  - `CreatedAtAction(nameof(Get), new { id }, result)` — 201 on POST
  - `NoContent()` — 204 when body is empty
  - `NotFound()` — 404 when entity not found
- Do **not** put business logic in controllers; they are thin wrappers.
- Use `[FromBody]` for request bodies and `[FromQuery]` for query parameters.

## Adding a New Controller

File: `WebApiShop/Controllers/SupplierController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Services;
using DTOs;

namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierServices _supplierServices;

        public SupplierController(ISupplierServices supplierServices)
        {
            _supplierServices = supplierServices;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupplierDTO>>> Get()
        {
            var items = await _supplierServices.GetSuppliers();
            if (!items.Any()) return NoContent();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SupplierDTO>> Get(int id)
        {
            var item = await _supplierServices.GetSupplier(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<SupplierDTO>> Post([FromBody] SupplierDTO supplier)
        {
            var created = await _supplierServices.AddSupplier(supplier);
            return CreatedAtAction(nameof(Get), new { id = created.SupplierId }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<SupplierDTO>> Put(int id, [FromBody] SupplierDTO supplier)
        {
            var updated = await _supplierServices.UpdateSupplier(id, supplier);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _supplierServices.DeleteSupplier(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
```

## Middleware that wraps every request

Requests pass through (in order):
1. `ErrorHandlingMiddleware` — global exception handler, returns JSON `{ message, stackTrace, innerException }`
2. `RateLimitingMiddleware` — enforces per-IP limits from `appsettings.json → RateLimiting`
3. `Authentication` / `Authorization`
4. `RatingMiddleware` — logs the request to the `Ratings` DB table

## Authentication

JWT Bearer is used. Protect endpoints with `[Authorize]` when needed. Token validation parameters are in `appsettings.json → JwtSettings`.
