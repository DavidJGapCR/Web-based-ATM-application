# My Specification

## Decisions (2026-10-22)

Full design and implementation plan are kept locally (not committed).

- **Priorities:** backend over frontend; Clean Architecture; SOLID; "scalable" = structure that grows easily + money correct under concurrency.
- **Architecture:** separate projects — `Atm.Domain`, `Atm.Application` (Commands/ + Queries/), `Atm.Infrastructure`, `Atm.Api` (minimal API, Endpoints/). The Commands/Queries split moved from a `Database/` folder to the Application layer.
- **Data:** EF Core + **EF Core migrations** (DbUp dropped). **SQLite** database; a temporary SQLite file per test for API integration tests.
- **Tables:** `Accounts` (Id, Name, Balance, Version) and `Transactions` (Id, AccountId, Type, Amount, BalanceAfter, TransferId, CreatedAt). Two accounts seeded.
- **Concurrency:** `Version` concurrency token; transfers atomic in one `SaveChanges()`; conflicts → retry once, then 409.
- **Error handling:** Result pattern for business failures → ProblemDetails (400/404/409/422); global exception handler → 500.
- **Endpoints (6):** GET accounts, GET account, GET account transactions, POST deposits, POST withdrawals, POST transfers.
- **Tests:** NUnit — domain, application, API integration. No frontend tests.
- **Frontend:** basic React + TypeScript (Vite), single page.

---

*Original notes below.*

These are my own notes. I want you to review this, ask me questions if you need to and lets brainstorm these ideas.

## Tech Stack

### Back End

1. Lets do a minimal API.
2. Create a Api folder to map the required endpoints.
3. Create a Database folder and inside it a Command folder and Queries folder.
4. I would like to use this tool for database migrations: https://dbup.readthedocs.io/en/latest/
5. We need unit test coverage with NUnit.
6. Lets do persistented database with in-memory for tests if required.
7. I like dependency injection
8. Do you have any questions about the back end?

### Front end

1. Do not include unit tests for the front end.
2. Lets do a ReactJS app with typescript.
3. Lets do something basic, just the necessary to reach the goal.
4. Do you have any questions about the front end?

### Questions

1. What tables do you think we need to create?
2. What do you plan to do for error handling?
3. how many endpoints do you plan to do?
4. Is there something I am not seeing. Let me know.

## Other important information

1. Make sure you take into account solid principles: Single responsibility principal, etc.
2. Take into account that timeframe is 2 to 4 hours for a human developer implementation.
