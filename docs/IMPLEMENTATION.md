# Sales Records — Implementation Guide

This document describes the implemented Sales API and how to run it in a local, test, or external environment.

## Database model

The diagram covers the tables implemented for the Sales flow and asynchronous publication. `SaleItems` is an owned collection of `Sales`; `OutboxMessages` is deliberately independent, without a foreign key, so it can represent any persisted domain event.

```mermaid
erDiagram
    SALES ||--|{ SALE_ITEMS : contains

    SALES {
        uuid Id PK
        int Number "identity"
        uuid CustomerId
        varchar CustomerName
        uuid BranchId
        varchar BranchName
        int Quantity
        decimal TotalAmount
        decimal Discount
        varchar Status
        timestamptz CreatedAt
        timestamptz UpdatedAt
    }

    SALE_ITEMS {
        uuid SaleId PK, FK
        int Id PK "owned-item identifier"
        uuid ProductId
        varchar ProductName
        decimal UnitPrice
        int Quantity
        decimal TotalAmount
        decimal Discount
    }

    OUTBOX_MESSAGES {
        uuid Id PK
        uuid EventId UK
        timestamptz OccurredAt
        varchar Type
        jsonb Payload
        timestamptz ProcessedAt
        int RetryCount
        timestamptz NextAttemptAt
        varchar Error
    }
```

`SaleItems.SaleId` has a cascading foreign key to `Sales.Id`. An `OutboxMessages` row is created in the same transaction as a sale creation or update, but it has no physical relationship to a specific sale.

## What was implemented

The API provides a complete sales flow:

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/api/Sale` | Creates a sale and calculates its aggregates. |
| `GET` | `/api/Sale/{id}` | Gets one sale and its items. |
| `GET` | `/api/Sale` | Lists sales with pagination, filtering, and ordering. |
| `PATCH` | `/api/Sale/{id}` | Partially updates a sale. |
| `PATCH` | `/api/Sale/{id}/cancel` | Cancels a sale. |
| `DELETE` | `/api/Sale/{id}` | Permanently removes a sale and its items. |

The Sales domain uses the **External Identities** pattern. Customer, branch, and product are represented by their identifiers and denormalized names; the Sales module does not create database foreign keys or navigation properties to those domains.

### Business rules

- Quantities must be between 1 and 20 for each product.
- From 4 to 9 equal items, the item receives a 10% discount.
- From 10 to 20 equal items, the item receives a 20% discount.
- Quantities below 4 receive no discount.

## Architecture and quality

The solution separates the business core from input and output adapters:

```mermaid
flowchart LR
    Client[Client] --> Api[Driver: Web API]
    Api --> Application[Core: Application]
    Application --> Domain[Core: Domain]

    Api --> Worker[Driver: Outbox Worker]
    Worker --> Application

    Application -->|repository ports| Persistence[Driven: EF Core / ORM]
    Persistence --> PostgreSQL[(PostgreSQL)]

    Application -->|event publisher port| Messaging[Driven: Rebus Adapter]
    Messaging --> RabbitMQ[(RabbitMQ)]
```

The Web API receives HTTP requests and the hosted worker triggers Outbox processing. The Application layer coordinates use cases through ports, while the Domain layer holds the sales rules. PostgreSQL and RabbitMQ remain replaceable driven adapters.

Implemented automated tests include:

- Unit tests for business rules and application handlers.
- Integration tests for EF Core persistence with PostgreSQL.
- Functional tests covering HTTP endpoints.

## Prerequisites

- .NET SDK 8.0 or later, with the .NET 8 runtime installed.
- Docker Engine and Docker Compose.

## Quick start

From the repository root, run:

```bash
cd template/backend
docker compose up -d ambev.developerevaluation.database
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

The default configuration targets the development database at `localhost:5432` (`developer_evaluation`). The API applies pending migrations automatically in the `Development` environment. On an empty database, it also adds three sample sales (including one cancelled sale) so the list, detail and filter endpoints can be tried immediately. The seed is idempotent and runs only in `Development`; it does not run in functional or integration test databases. Seeded sales use the same repository flow as API requests, so their events are persisted in the Outbox and can be published after RabbitMQ is enabled. With the default `http` launch profile, Swagger is available at [http://localhost:5119/swagger](http://localhost:5119/swagger).

If the same terminal was previously used to run the worker against the test database, clear its environment overrides before using the default configuration:

**Bash**

```bash
unset ConnectionStrings__DefaultConnection RabbitMq__Enabled RabbitMq__ConnectionString RabbitMq__InputQueue
```

**PowerShell**

```powershell
Remove-Item Env:ConnectionStrings__DefaultConnection, Env:RabbitMq__Enabled, Env:RabbitMq__ConnectionString, Env:RabbitMq__InputQueue -ErrorAction SilentlyContinue
```

For hot reload, replace the last command with:

```bash
dotnet watch run --project src/Ambev.DeveloperEvaluation.WebApi
```

## Run tests

Start the test database and run the complete suite:

```bash
docker compose up -d ambev.developerevaluation.database.test
dotnet test Ambev.DeveloperEvaluation.sln
```

The test database is isolated from the application database and is exposed locally on port `5433` by the provided Compose configuration.

## Custom configuration

The default Compose setup is sufficient for local development. For another PostgreSQL instance or a non-development environment, use standard .NET environment variables without editing tracked files.

**Bash**

```bash
export ConnectionStrings__DefaultConnection='Host=<host>;Port=5432;Database=<database>;Username=<user>;Password=<password>'
export Jwt__SecretKey='<secret-with-at-least-32-characters>'
```

**PowerShell**

```powershell
$env:ConnectionStrings__DefaultConnection = 'Host=<host>;Port=5432;Database=<database>;Username=<user>;Password=<password>'
$env:Jwt__SecretKey = '<secret-with-at-least-32-characters>'
```

Environment variables use `__` to represent nested configuration sections. RabbitMQ-specific variables are shown in the Outbox section below.

### Apply migrations manually (optional)

Manual migration application is useful for controlled environments where the application must not change the database schema during startup.

```text
dotnet tool install --global dotnet-ef --version 8.*
dotnet ef database update --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi --context DefaultContext
```

## Events, Outbox, and RabbitMQ

Creating, updating, or cancelling a sale registers `SaleCreated`, `SaleUpdated`, or `SaleCancelled` domain events.

`SaleRepository.CreateAsync` and `SaleRepository.UpdateAsync` explicitly create an `OutboxMessages` record for each domain event and persist both the sale change and Outbox records in the same EF Core `SaveChangesAsync` call. This avoids publishing an event for a transaction that did not persist.

RabbitMQ is optional and disabled by default. When enabled, `OutboxPublisherHostedService` checks for pending Outbox records every five seconds. It publishes an `IntegrationEventEnvelope` through the Application output port, whose implementation uses Rebus with RabbitMQ. Failed messages remain pending and are retried with exponential backoff.

### Development seed and Outbox

On the first run against an empty development database, the API creates three sample sales: two active and one cancelled. The seed uses `ISaleRepository`, the same persistence flow used by the API, and commits the complete sample data in one transaction.

This creates four pending Outbox messages:

- Three `SaleCreated` messages, one for each sample sale.
- One `SaleCancelled` message for the cancelled sample sale.

When RabbitMQ is disabled, the worker does not start and these messages remain pending in `OutboxMessages`. If RabbitMQ is enabled on a later API startup, the worker reads and publishes them. A message is marked as processed only after its publication succeeds. The seed is idempotent: once at least one sale exists, it does not create sales or messages again.

### Run the Outbox worker with RabbitMQ

The worker runs inside the Web API process; it is not a separate executable.

```text
docker compose up -d ambev.developerevaluation.database ambev.developerevaluation.rabbitmq
```

**Bash**

```bash
export RabbitMq__Enabled=true
export RabbitMq__ConnectionString='amqp://guest:guest@localhost:5672'
export RabbitMq__InputQueue='sales-records'
```

**PowerShell**

```powershell
$env:RabbitMq__Enabled = 'true'
$env:RabbitMq__ConnectionString = 'amqp://guest:guest@localhost:5672'
$env:RabbitMq__InputQueue = 'sales-records'
```

```text
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

The management UI of the local Compose container is available at [http://localhost:15672](http://localhost:15672). Its local default credentials are `guest` / `guest`. For a remote broker, create and use a dedicated non-default user instead.

### Validate the worker against the test database

The broker does not access PostgreSQL. The API worker reads the `OutboxMessages` table, so point the API at the test database when this is the intended environment:

```text
docker compose up -d ambev.developerevaluation.database.test ambev.developerevaluation.rabbitmq
```

**Bash**

```bash
export ConnectionStrings__DefaultConnection='Host=localhost;Port=5433;Database=<test-database>;Username=<user>;Password=<password>'
export RabbitMq__Enabled=true
export RabbitMq__ConnectionString='amqp://guest:guest@localhost:5672'
export RabbitMq__InputQueue='sales-records-test'
```

**PowerShell**

```powershell
$env:ConnectionStrings__DefaultConnection = 'Host=localhost;Port=5433;Database=<test-database>;Username=<user>;Password=<password>'
$env:RabbitMq__Enabled = 'true'
$env:RabbitMq__ConnectionString = 'amqp://guest:guest@localhost:5672'
$env:RabbitMq__InputQueue = 'sales-records-test'
```

```text
dotnet watch run --project src/Ambev.DeveloperEvaluation.WebApi
```

There is currently no consumer in this repository. The implementation is the producer side: it persists messages reliably and publishes them to RabbitMQ. A consuming service should own its own queue, handler, idempotency strategy, and business action.

The current worker is intended for a single API instance. Before scaling the API horizontally, the Outbox repository should implement an atomic claim/lock strategy so that two workers cannot publish the same pending message concurrently. Consumers should remain idempotent because broker publication is at-least-once.

## API usage examples

The requests below can be created directly in Postman or Insomnia. Start the API as described in **Quick start**, then use this base URL:

```text
http://localhost:5119
```

No authentication is required for the Sales endpoints. For requests with a body, add the `Content-Type: application/json` header. The interactive contract is available at [http://localhost:5119/swagger](http://localhost:5119/swagger).

If you use the `https` profile, change the base URL to `https://localhost:7181` and configure your API client to accept the local development certificate.

### 1. Create a sale

This example creates two items. The Notebook quantity of `4` receives a 10% discount (`40.00`), while the Mouse quantity of `10` receives a 20% discount (`100.00`). The resulting sale has quantity `14`, discount `140.00`, and total amount `760.00`.

| Field | Value |
| --- | --- |
| Method | `POST` |
| URL | `http://localhost:5119/api/Sale` |
| Header | `Content-Type: application/json` |

```json
{
  "customerId": "11111111-1111-1111-1111-111111111111",
  "customerName": "Sample Customer",
  "branchId": "22222222-2222-2222-2222-222222222222",
  "branchName": "Main Branch",
  "items": [
    {
      "productId": "33333333-3333-3333-3333-333333333333",
      "productName": "Notebook",
      "unitPrice": 100.00,
      "quantity": 4
    },
    {
      "productId": "44444444-4444-4444-4444-444444444444",
      "productName": "Mouse",
      "unitPrice": 50.00,
      "quantity": 10
    }
  ]
}
```

The API returns HTTP `201` with this shape. Copy the value of `data` and replace `<sale-id>` in subsequent requests.

```json
{
  "success": true,
  "message": "Sale created successfully",
  "data": "<sale-id>"
}
```

### 2. Get the complete sale

This endpoint returns the sale aggregates, status, timestamps, and every item.

| Field | Value |
| --- | --- |
| Method | `GET` |
| URL | `http://localhost:5119/api/Sale/<sale-id>` |

### 3. List and filter sales

The list response includes `data`, `currentPage`, `pageSize`, `totalPages`, and `totalCount`. `_page` starts at `1`; `_size` accepts values from `1` to `100`.

All list examples use method `GET` and do not require headers or a body.

| Purpose | URL |
| --- | --- |
| Page ordered by newest sale, then highest total | `http://localhost:5119/api/Sale?_page=1&_size=10&_order=createdAt%20desc,totalAmount%20desc` |
| Customer and status | `http://localhost:5119/api/Sale?customerId=11111111-1111-1111-1111-111111111111&status=NotCancelled` |
| Product and total range | `http://localhost:5119/api/Sale?productId=44444444-4444-4444-4444-444444444444&_minTotalAmount=500&_maxTotalAmount=1000` |
| Creation interval | `http://localhost:5119/api/Sale?_minCreatedAt=2026-01-01T00%3A00%3A00Z&_maxCreatedAt=2026-12-31T23%3A59%3A59Z` |

Supported exact filters are `number`, `customerId`, `branchId`, `productId`, and `status`. `_order` accepts `number`, `createdAt`, or `totalAmount`, each optionally followed by `asc` or `desc`; separate criteria with commas.

### 4. Partially update a sale

`PATCH` changes only fields present in the request. Customer and branch are external identities, so their `Id` and `Name` must always be provided together. Supplying `items` replaces the entire item collection; individual items cannot be patched separately.

| Field | Value |
| --- | --- |
| Method | `PATCH` |
| URL | `http://localhost:5119/api/Sale/<sale-id>` |
| Header | `Content-Type: application/json` |

Change the customer identity:

```json
{
  "customerId": "55555555-5555-5555-5555-555555555555",
  "customerName": "Updated Customer"
}
```

Replace all items and recalculate quantity, discount, and total amount:

```json
{
  "items": [
    {
      "productId": "33333333-3333-3333-3333-333333333333",
      "productName": "Notebook",
      "unitPrice": 120.00,
      "quantity": 5
    }
  ]
}
```

### 5. Cancel or delete a sale

Cancellation preserves the sale and changes its status to `Cancelled`. Deletion permanently removes the sale and its owned items; use it only when that is the intended result.

| Action | Method | URL | Body |
| --- | --- | --- | --- |
| Cancel | `PATCH` | `http://localhost:5119/api/Sale/<sale-id>/cancel` | None |
| Delete | `DELETE` | `http://localhost:5119/api/Sale/<sale-id>` | None |

Run the delete request only after completing the other examples.

### Responses and errors

Successful responses use an envelope with `success`, `message`, `errors`, and, when applicable, `data`. Validation failures return HTTP `400`; a missing sale returns HTTP `404`. Both use the error contract below:

```json
{
  "type": "ValidationError",
  "error": "Invalid input data.",
  "detail": "Quantity must be between 1 and 20."
}
```
