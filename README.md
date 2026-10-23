# Web-based ATM Application

A single-user web ATM with two bank accounts (Checking and Savings). It supports deposits, withdrawals, transfers between the accounts, balances and per-account transaction history.

- **Backend:** .NET 10, ASP.NET Core minimal API, EF Core 10 + SQLite, built as a Clean Architecture solution.
- **Frontend:** React + TypeScript (Vite).
- **Tests:** NUnit, covering the domain, application and API layers (70 tests).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20 or later

## Getting started

Run the backend and the frontend in two terminals from the repository root.

**1. API** (http://localhost:5080)

```bash
dotnet run --project src/Atm.Api
```

On startup the API applies the EF Core migrations, which creates `src/Atm.Api/atm.db` and seeds the two accounts with a balance of 0. In Development, interactive API docs are served at http://localhost:5080/scalar.

**2. Frontend** (http://localhost:5173)

```bash
cd src/atm-web
npm install
npm run dev
```

The Vite dev server proxies `/api` to the backend, so no CORS setup is needed.

**Tests**

```bash
dotnet test
```

To reset the data, stop the API and delete `src/Atm.Api/atm.db`.

## Architecture

```
            ┌──────────────┐
            │   Atm.Api    │  Minimal API endpoints, HTTP contracts, error mapping,
            └──────┬───────┘  composition root (DI wiring)
         ┌─────────┴──────────┐
         ▼                    ▼
┌──────────────────┐   ┌────────────────────┐
│ Atm.Application  │◄──│ Atm.Infrastructure │  EF Core DbContext, migrations,
└────────┬─────────┘   └────────────────────┘  repositories, unit of work
         │  Commands/ and Queries/ handlers, repository + unit-of-work interfaces, DTOs
         ▼
┌──────────────────┐
│   Atm.Domain     │  Account, AccountTransaction, FundsTransfer, amount rules,
└──────────────────┘  Result/Error — no dependencies
```

Dependencies point inward. Each layer is its own project, so the compiler enforces the boundaries: the Domain cannot reference EF Core, and the Application layer only knows the persistence interfaces it defines.

- **Domain:** business rules live on the entities. `Account.Deposit` and `Account.Withdraw` validate the amount (greater than 0, at most 2 decimal places), prevent overdrafts and record an immutable `AccountTransaction`. `FundsTransfer` validates both sides before changing either account and links the two history entries with a shared `TransferId`.
- **Application:** one handler per use case (single responsibility), behind the small `ICommandHandler` and `IQueryHandler` interfaces.
- **Infrastructure:** EF Core with SQLite. The schema is managed by EF Core migrations, and the two accounts are seeded through a migration.
- **Api:** thin endpoints that translate HTTP into commands and queries, and translate results back into HTTP responses.

### Endpoints

| Method | Route | Body | Success |
|---|---|---|---|
| GET | `/api/accounts` | — | 200, list of accounts |
| GET | `/api/accounts/{id}` | — | 200, account |
| GET | `/api/accounts/{id}/transactions` | — | 200, history, newest first |
| POST | `/api/accounts/{id}/deposits` | `{ "amount": 100.00 }` | 200, updated account and transaction |
| POST | `/api/accounts/{id}/withdrawals` | `{ "amount": 25.50 }` | 200, updated account and transaction |
| POST | `/api/transfers` | `{ "fromAccountId": 1, "toAccountId": 2, "amount": 40 }` | 200, both accounts and the transfer id |

### Error handling

There are two separate channels:

1. **Expected business failures** are returned as values (`Result` / `Result<T>` carrying a typed `Error`), not thrown. A single mapping in `Atm.Api/Errors/ResultHttpExtensions.cs` turns them into responses:

   | Error kind | Example | HTTP |
   |---|---|---|
   | Validation | amount ≤ 0, more than 2 decimals, same-account transfer | 400 |
   | NotFound | unknown account | 404 |
   | InsufficientFunds | withdrawal or transfer larger than the balance | 422 |
   | Conflict | concurrent update still conflicting after one retry | 409 |

2. **Unexpected failures** go to a global `IExceptionHandler`. It logs the exception and returns a 500 without exposing internal details. Malformed requests (invalid JSON, wrong types, missing fields, unsupported content type) and unmatched routes also return ProblemDetails, with the matching 4xx status.

Every error response is an RFC 7807 `application/problem+json` body with a stable `code` field (for example `account.insufficient_funds`) that the frontend can rely on.

### Concurrency and consistency

- **Atomic transfers.** Both balance updates and both history rows are written in one `SaveChanges()` call, which is a single database transaction.
- **No lost updates or overdrafts under concurrency.** `Account.Version` is an EF Core concurrency token; SQLite has no `rowversion`, so the domain increments it on every change. If two requests change the same account at the same time, the second save fails the version check. The handler then discards its state, reloads fresh data and retries once. If that also conflicts, it returns 409. An integration test fires parallel withdrawals and checks that the account is never overdrawn and that the balance matches the history.

## Technical decisions and tradeoffs

| Decision | Why | Tradeoff |
|---|---|---|
| Separate projects per layer | The compiler enforces dependency direction | More projects to set up than folders in one project |
| EF Core + EF migrations (instead of DbUp) | One source of truth for the schema (the model), LINQ, change tracking, atomic `SaveChanges`, built-in optimistic concurrency | Less direct control over SQL than hand-written scripts |
| SQLite | Reviewers can run it with no setup; real relational behavior | Not a server database; decimals are stored as TEXT, so all arithmetic happens in C# |
| `decimal` for money | Exact base-10 arithmetic; `double` cannot represent values like 0.10 exactly | Slightly slower, which doesn't matter here |
| Result pattern instead of exceptions for business errors | Failures are explicit in signatures and cheap; exceptions stay exceptional | Some `if (result.IsFailure)` plumbing |
| No MediatR | Plain handlers behind two small interfaces are easy to read and debug | No pipeline behaviors (they aren't needed at this size) |
| Concurrency token + one retry | Correct under concurrent requests without pessimistic locks | Under heavy contention some requests get 409 and the client must retry |
| Migrations applied at startup | Clone-and-run experience | In production, migrations would run as a separate deployment step |
| API tests use a temporary SQLite **file** per test | Real SQLite locking semantics for the parallel-request test, plus full isolation | Slightly slower than in-memory |
| No frontend tests | Deliberate scope choice; the backend is the focus | UI behavior was checked manually |

## Project layout

```
src/
  Atm.Domain/           Entities, rules, Result/Error
  Atm.Application/      Commands/, Queries/, Abstractions/, DTOs
  Atm.Infrastructure/   Persistence/ (DbContext, configurations, repositories, migrations)
  Atm.Api/              Endpoints/, Contracts/, Errors/, Program.cs
  atm-web/              React + TypeScript frontend
tests/
  Atm.Domain.Tests/        Entity and transfer rules
  Atm.Application.Tests/   Handlers (with in-memory fakes), retry behavior
  Atm.Api.Tests/           End-to-end HTTP tests against a real SQLite database
```

## With more time

- Authentication and multiple users and accounts.
- Pagination for transaction history.
- Idempotency keys on POST requests, so a retried request can't apply twice.
- Frontend tests (React Testing Library) and an end-to-end smoke test.
- Docker Compose and a CI pipeline (build, test, lint).
- Structured logging and health checks.
