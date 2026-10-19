# .NET Testing Guide

[![Build](https://github.com/varunkonnojutech-lab/dotnet-testing-guide/actions/workflows/ci.yml/badge.svg)](https://github.com/varunkonnojutech-lab/dotnet-testing-guide/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/varunkonnojutech-lab/dotnet-testing-guide)

A complete, runnable guide to testing .NET applications: **xUnit** for the test framework, **Moq** for mocking dependencies, and **FluentAssertions** for readable assertions. Three test styles — pure unit tests, mocked unit tests, and full integration tests with `WebApplicationFactory` — all demonstrated against one small Orders API.

## Who is this for

Developers who can write code but aren't confident writing tests for it — and seniors who want a reference project showing how the three test styles fit together in one solution.

## What you'll learn

- Writing xUnit tests: `[Fact]`, `[Theory]` + `[InlineData]`, async tests
- FluentAssertions: readable assertions (`Should().Be()`, `ThrowAsync`, wildcards)
- Moq: `Setup`/`ReturnsAsync`, `Verify` with `Times.Once`/`Times.Never`, argument matchers (`It.IsAny`, `It.Is`)
- Integration testing with `WebApplicationFactory`: boot the real API in-process, call it over HTTP, assert status codes and payloads
- When to use each style: the test pyramid (fast unit tests at the base, fewer integration tests at the top)

## The sample app

A tiny Orders API (`src/OrdersApp`) with three endpoints:

| Method | Route | Behavior |
|---|---|---|
| `POST` | `/orders` | Creates an order → `201 Created` with `Location` header, or `400` on invalid input |
| `GET` | `/orders` | Lists all orders |
| `GET` | `/orders/{id}` | Returns one order, or `404` |

Pricing rules (10% discount on subtotals ≥ $100, then 8% tax) live in the pure static `OrderPricing` class; validation + orchestration lives in `OrderService`, which depends on `IOrderRepository` — an interface precisely so tests can mock it.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) — or click **Open in GitHub Codespaces** above for a ready-made environment in your browser, no install needed.

## Quick start

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

Run the API and try it:

```bash
dotnet run --project src/OrdersApp
curl -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -d '{"customerName":"Ada","items":[{"productName":"Widget","unitPrice":25,"quantity":2}]}'
# -> 201 Created, total 54 (2 x 25 = 50, +8% tax)
```

## The test suite

`tests/OrdersApp.Tests` — 18 tests across three styles:

**1. Pure unit tests** (`OrderPricingTests.cs`) — no mocks, no I/O. The fastest tests you can write: input in, value out.

```csharp
[Theory]
[InlineData(100.00, 10.00)]
public void CalculateDiscount_Applies_Ten_Percent_At_Or_Above_Threshold(
    decimal subtotal, decimal expectedDiscount)
{
    OrderPricing.CalculateDiscount(subtotal).Should().Be(expectedDiscount);
}
```

**2. Mocked unit tests** (`OrderServiceTests.cs`) — Moq stands in for `IOrderRepository`, so we test `OrderService` logic (validation, total calculation) without a real store, and *verify interactions*:

```csharp
_repository
    .Setup(r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync((Order order, CancellationToken _) => order);

// ...act...

_repository.Verify(
    r => r.SaveAsync(It.Is<Order>(o => o.CustomerName == "Grace Hopper"),
                      It.IsAny<CancellationToken>()),
    Times.Once);
```

**3. Integration tests** (`OrdersApiTests.cs`) — `WebApplicationFactory<Program>` boots the real app in-process and exercises it over HTTP: routing, JSON binding, validation, status codes, and `Location` headers.

```csharp
var response = await _client.PostAsJsonAsync("/orders", request);
response.StatusCode.Should().Be(HttpStatusCode.Created);
```

Run just one style:

```bash
dotnet test --filter "FullyQualifiedName~OrderPricingTests"
dotnet test --filter "FullyQualifiedName~OrderServiceTests"
dotnet test --filter "FullyQualifiedName~OrdersApiTests"
```

## Cheat sheet

| I want to... | Reach for |
|---|---|
| Assert equality readably | `result.Should().Be(expected)` |
| Assert an exception (async) | `await act.Should().ThrowAsync<ArgumentException>().WithMessage("*part*")` |
| Assert a collection contains a match | `orders.Should().Contain(o => o.CustomerName == "Ada")` |
| Fake a dependency | `var mock = new Mock<IOrderRepository>();` |
| Stub an async method | `.Setup(...).ReturnsAsync(value)` or `.ReturnsAsync((Order o, CancellationToken _) => o)` |
| Match any argument | `It.IsAny<Order>()`; match a predicate: `It.Is<Order>(o => o.Id > 0)` |
| Verify a call happened once / never | `.Verify(..., Times.Once)` / `.Verify(..., Times.Never)` |
| Boot the API in a test | `IClassFixture<WebApplicationFactory<Program>>` + `public partial class Program { }` |
| Run tests in CI | `dotnet test -c Release` (see `.github/workflows/ci.yml`) |

## The test pyramid, applied here

- **Base — pure unit tests** (`OrderPricing`): cheapest, fastest, most of them. Test every branch.
- **Middle — mocked unit tests** (`OrderService`): verify logic + interactions when dependencies exist.
- **Top — integration tests** (HTTP via `WebApplicationFactory`): fewer, slower, but they prove the whole stack works together.

A common mistake is testing business logic *only* through HTTP. Put the logic tests at the base; use integration tests to prove wiring (routes, binding, status codes).

## Next steps

- Add a test for the discount boundary: a $99.99 order vs a $100.00 order through `POST /orders`.
- Swap `InMemoryOrderRepository` for EF Core + SQLite and add a repository test class — same test style, real persistence.
- Try mutation testing (`stryker-net`) to check whether your tests actually catch bugs.
- Then see [owasp-dotnet](https://github.com/varunkonnojutech-lab/owasp-dotnet) *(coming soon)* for security-focused testing.

## License

MIT — see [LICENSE](LICENSE).
