# Developer Evaluation Project

`READ CAREFULLY`

## Use Case
**You are a developer on the DeveloperStore team. Now we need to implement the API prototypes.**

As we work with `DDD`, to reference entities from other domains, we use the `External Identities` pattern with denormalization of entity descriptions.

Therefore, you will write an API (complete CRUD) that handles sales records. The API needs to be able to inform:

* Sale number
* Date when the sale was made
* Customer
* Total sale amount
* Branch where the sale was made
* Products
* Quantities
* Unit prices
* Discounts
* Total amount for each item
* Cancelled/Not Cancelled

It's not mandatory, but it would be a differential to build code for publishing events of:
* SaleCreated
* SaleModified
* SaleCancelled
* ItemCancelled

If you write the code, **it's not required** to actually publish to any Message Broker. You can log a message in the application log or however you find most convenient.

### Business Rules

* Purchases above 4 identical items have a 10% discount
* Purchases between 10 and 20 identical items have a 20% discount
* It's not possible to sell above 20 identical items
* Purchases below 4 items cannot have a discount

These business rules define quantity-based discounting tiers and limitations:

1. Discount Tiers:
   - 4+ items: 10% discount
   - 10-20 items: 20% discount

2. Restrictions:
   - Maximum limit: 20 items per product
   - No discounts allowed for quantities below 4 items

## Overview
This section provides a high-level overview of the project and the various skills and competencies it aims to assess for developer candidates. 

See [Overview](/.doc/overview.md)

## Tech Stack
This section lists the key technologies used in the project, including the backend, testing, frontend, and database components. 

See [Tech Stack](/.doc/tech-stack.md)

## Frameworks
This section outlines the frameworks and libraries that are leveraged in the project to enhance development productivity and maintainability. 

See [Frameworks](/.doc/frameworks.md)

<!-- 
## API Structure
This section includes links to the detailed documentation for the different API resources:
- [API General](./docs/general-api.md)
- [Products API](/.doc/products-api.md)
- [Carts API](/.doc/carts-api.md)
- [Users API](/.doc/users-api.md)
- [Auth API](/.doc/auth-api.md)
-->

## Project Structure
This section describes the overall structure and organization of the project files and directories. 

See [Project Structure](/.doc/project-structure.md)

## Implemented Sales API

The Sales API implements the complete CRUD flow, plus a separate cancellation action:

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/api/Sale` | Creates a sale and calculates discounts and totals. |
| `GET` | `/api/Sale/{id}` | Returns one sale with its items. |
| `GET` | `/api/Sale?_page=1&_size=10` | Returns a paginated list of sales. |
| `PATCH` | `/api/Sale/{id}` | Partially updates a sale. Fields not sent remain unchanged. |
| `PATCH` | `/api/Sale/{id}/cancel` | Cancels a sale. |
| `DELETE` | `/api/Sale/{id}` | Permanently removes a sale and its items. |

Sales store customer, branch and product identifiers together with their descriptions. These are external identities, so the Sales domain has no navigation properties or foreign keys to those external domains.

### Running locally

#### Prerequisites

- .NET SDK 8.0 or later
- Docker and Docker Compose

From the repository root, enter the backend directory:

```bash
cd template/backend
```

Start the PostgreSQL containers used by the application and the integration tests:

```bash
docker compose up -d ambev.developerevaluation.database ambev.developerevaluation.database.test
```

Run the API:

```bash
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

When running with the `Development` environment, the API automatically applies pending migrations during startup. No separate `dotnet ef` command is necessary.

To use another PostgreSQL instance, override the connection string through a standard .NET environment variable:

```bash
export ConnectionStrings__DefaultConnection='Host=localhost;Port=5432;Database=sales_records;Username=sales_user;Password=change-me'
```

The same variable is honored by the API and EF Core migrations. Configure `Jwt__SecretKey` in the same manner when running outside local development. In the default HTTPS launch profile, Swagger is available at `https://localhost:7181/swagger`.

### Running tests

From `template/backend`:

```bash
dotnet restore
dotnet test Ambev.DeveloperEvaluation.sln --no-restore
```

Integration and functional tests require the test PostgreSQL container started in the previous section.

## Sales API examples

Set the API base URL and use a generated identifier after creating a sale:

```bash
export API_URL='https://localhost:7181'
export SALE_ID='<sale-id-returned-by-the-create-request>'
```

For local HTTPS development, the examples use `-k` to accept the development certificate.

### Create a sale

```bash
curl -k -X POST "$API_URL/api/Sale" \
  -H 'Content-Type: application/json' \
  -d '{
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
      }
    ]
  }'
```

### Get one sale or list sales

```bash
curl -k "$API_URL/api/Sale/$SALE_ID"
curl -k "$API_URL/api/Sale?_page=1&_size=10&status=NotCancelled&_order=createdAt%20desc,totalAmount%20asc"
```

The list accepts exact filters for `number`, `customerId`, `branchId`, `productId` and `status`. It also accepts ranges through `_minTotalAmount`, `_maxTotalAmount`, `_minCreatedAt` and `_maxCreatedAt`. The `_order` value accepts `number`, `createdAt` or `totalAmount`, optionally followed by `asc` or `desc`; separate multiple criteria with commas.

### Partially update a sale

Only fields included in the body are changed. Customer and branch are external identities, so each one must always be sent as its complete `Id` + `Name` pair. Providing `items` replaces the complete item collection and recalculates the sale aggregates.

```bash
curl -k -X PATCH "$API_URL/api/Sale/$SALE_ID" \
  -H 'Content-Type: application/json' \
  -d '{
    "customerId": "44444444-4444-4444-4444-444444444444",
    "customerName": "Updated Customer"
  }'
```

### Cancel a sale

```bash
curl -k -X PATCH "$API_URL/api/Sale/$SALE_ID/cancel"
```

### Delete a sale

```bash
curl -k -X DELETE "$API_URL/api/Sale/$SALE_ID"
```
