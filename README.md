# Inventory Management Integration API

A .NET 10 Web API for managing inventory products and processing inbound stock receipts from external warehouse partners.

The project is designed as a practical integration-oriented API rather than a basic CRUD demo. It includes persistent SQLite storage, stock adjustment audit records, duplicate-request protection, and automated tests for core receipt-processing rules.

## Features

- Product CRUD API
- SQLite persistence with Entity Framework Core migrations
- Request and response DTOs
- Input validation with data annotations
- Service layer separating HTTP handling from database/business logic
- Stock adjustments for received, issued, and corrected stock
- Duplicate-protection for individual stock adjustments using an external reference
- Inbound partner receipt processing with multiple product lines
- Duplicate receipt detection using partner code and receipt reference
- Automated xUnit tests using SQLite in memory

## Architecture

```text
Controller
   ↓
Service
   ↓
Entity Framework Core DbContext
   ↓
SQLite database
```

Controllers handle HTTP concerns such as routes and status codes. Services contain inventory and integration business rules. Entity Framework Core persists the data to SQLite.

## Main Endpoints

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/products` | Get all products |
| GET | `/api/products/{id}` | Get one product |
| POST | `/api/products` | Create a product |
| PUT | `/api/products/{id}` | Update a product |
| DELETE | `/api/products/{id}` | Delete a product |
| POST | `/api/products/{id}/stock-adjustments` | Record one stock adjustment |
| POST | `/api/inbound-receipts` | Process a partner stock-receipt batch |

## Inbound Receipt Example

```json
{
  "partnerCode": "WAREHOUSE-A",
  "receiptReference": "GRN-2026-0001",
  "lines": [
    {
      "productId": 1,
      "quantityReceived": 3
    },
    {
      "productId": 2,
      "quantityReceived": 2
    }
  ]
}
```

A valid receipt updates all listed product quantities, creates stock-adjustment records, and stores the receipt reference.

Sending the same `partnerCode` and `receiptReference` again returns `409 Conflict`, preventing the stock from being updated twice.

## Running Locally

1. Open the solution in Visual Studio.
2. Open **Tools → NuGet Package Manager → Package Manager Console**.
3. Run:

```powershell
Update-Database
```

4. Start the API with `F5`.
5. Use the included `.http` requests or a REST client to call the endpoints.
6. Open `https://localhost:7016/swagger` to explore and test the API interactively.

## Tests

Open **Test → Test Explorer** and choose **Run All**.

The current tests verify that:

- a valid inbound receipt increases stock and creates receipt/audit records;
- a duplicate receipt does not increase stock twice;
- a receipt containing an unknown product does not change any stock.

## Authentication and Authorization

The API uses JWT bearer authentication and role-based authorization.

| Role | Permissions |
|---|---|
| `InventoryReader` | View products |
| `InventoryManager` | Create, update, delete products, and record manual stock adjustments |
| `WarehouseIntegration` | Submit inbound warehouse receipts |

For local development, JWTs are created with `dotnet user-jwts`. Tokens and signing keys are local-only and must never be committed to source control.