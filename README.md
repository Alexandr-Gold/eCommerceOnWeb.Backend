eCommerceOnWeb.Backend

A backend application for an electronics e-commerce store, built with 
.NET 8, ASP.NET Core Web API, Entity Framework Core, and PostgreSQL.

The project follows Clean Architecture and Domain-Driven Design (DDD) 
principles, with CQRS-style application handling using MediatR.

The primary goal is to build a maintainable, production-oriented backend
with clear separation of concerns, explicit domain rules, and reliable 
data persistence.

1. Technology Stack

Category

Technologies

Platform

.NET 8, ASP.NET Core Web API

Architecture

Clean Architecture, Domain-Driven Design, CQRS

Application

MediatR, FluentValidation, Result Pattern

ORM

Entity Framework Core 8

Database

PostgreSQL 16

Query Layer

Ardalis.Specification

Infrastructure

Docker Compose, Redis, MinIO

Error Handling

IExceptionHandler, ProblemDetails

Testing

xUnit, FluentAssertions

Database Testing

PostgreSQL Integration Tests, Respawn

2. Architecture

The solution follows Clean Architecture principles.

The main projects are organized as follows:

eCommerceOnWeb.Backend
│
├── Domain
│   └── Business entities, aggregates, value objects,
│       domain events and business rules
│
├── Application
│   └── Commands, queries, handlers, validators,
│       application interfaces and error handling
│
├── Persistence
│   └── EF Core, PostgreSQL, DbContext,
│       entity configurations, repositories and migrations
│
└── WebApi
    └── API endpoints, dependency injection,
        global exception handling and Swagger

Project Dependencies

Domain — independent of the other application layers.

Application — depends on Domain.

Persistence — depends on Application and Domain.

WebApi — composes Application and Persistence.

Architectural Principles

Domain-Driven Design with explicit aggregate boundaries.

Clean Architecture with dependency inversion.

CQRS-style separation of commands and queries.

MediatR for application request handling.

Result Pattern for expected application errors.

FluentValidation for request validation.

Repository abstractions and specifications for data access.

Domain Events for communicating domain changes.

3. Implemented Functionality

The current development focus is the Product aggregate.

Product Domain

The Product aggregate includes:

Product identity: Name, SKU, GTIN and Model Number.

Price represented by the Money value object.

Shipping dimensions represented by the Dimensions value object.

Warranty and stock management.

Product lifecycle and status transitions.

Product image management.

Dynamic product attributes stored as PostgreSQL JSONB.

Soft deletion.

Domain Events.

Product Status Lifecycle

The current Product lifecycle supports:

Draft

Active

Inactive

Discontinued

Business rules control allowed state transitions and modifications 
depending on the product status.

Database Integrity

PostgreSQL enforces data integrity through:

Unique SKU index.

Partial unique GTIN index (WHERE gtin IS NOT NULL).

Foreign keys for Category and Brand.

GIN index for JSONB product attributes.

Additional indexes for frequently queried relationships.

Application Features

Implemented application functionality includes:

Product creation.

Product retrieval.

Product details query.

Request validation.

Application-level error handling.

Database constraint violation abstraction.

4. Solution Structure

eCommerceOnWeb.Backend/
│
├── README.md
├── docker-compose.yml
├── .gitignore
│
└── src/
    │
    ├── eCommerceOnWeb.Backend.Domain/
    │
    ├── eCommerceOnWeb.Backend.Application/
    │
    ├── eCommerceOnWeb.Backend.Persistence/
    │   ├── Contexts/
    │   ├── EntityTypeConfigurations/
    │   ├── Migrations/
    │   └── Repositories/
    │
    ├── eCommerceOnWeb.Backend.WebApi/
    │
    └── tests/
        ├── eCommerceOnWeb.Backend.Domain.UnitTests/
        ├── eCommerceOnWeb.Backend.Application.UnitTests/
        └── eCommerceOnWeb.Backend.Persistence.IntegrationTests/

5. Prerequisites

The following tools are required for local development:

.NET 8 SDK

Docker Desktop

Git

Visual Studio 2022 (recommended)

6. Local Development Setup

Step 1. Clone the repository

git clone <YOUR_GITHUB_REPOSITORY_URL>
cd eCommerceOnWeb.Backend

Step 2. Start infrastructure services

The project uses Docker Compose to run its local infrastructure.

From the repository root:

docker compose up -d

Check the running containers:

docker compose ps

Configured services:

Service

Container

Port

PostgreSQL 16

ecommerce_postgres

5432

pgAdmin

ecommerce_pgadmin

5050

Redis 7

ecommerce_redis

6379

MinIO

ecommerce_minio

9000, 9001

MinIO initialization is handled by the minio_init service.

Step 3. Configure the database connection

The WebApi project uses the DefaultConnection connection string.

For local development, use .NET User Secrets rather than storing 
passwords in application settings.

Initialize User Secrets (only if the project has not already been initialized):

dotnet user-secrets init --project 
"src/eCommerceOnWeb.Backend.WebApi/eCommerceOnWeb.Backend.WebApi.csproj"

Set the local connection string:

dotnet user-secrets set "ConnectionStrings:DefaultConnection" 
"Host=localhost;Port=5432;Database=ecommerce_db;Username=db_admin;
Password=YOUR_LOCAL_POSTGRES_PASSWORD" --project "src/eCommerceOnWeb.
Backend.WebApi/eCommerceOnWeb.Backend.WebApi.csproj"

Use the PostgreSQL credentials configured in your local Docker 
environment.

Step 4. Apply database migrations

The project uses Entity Framework Core migrations.

In Visual Studio 2022, open Package Manager Console and execute:

Update-Database -Context ECommerceDbContext -Project eCommerceOnWeb.
Backend.Persistence -StartupProject eCommerceOnWeb.Backend.WebApi

The migration command explicitly specifies ECommerceDbContext.

Step 5. Build the solution

dotnet build

Step 6. Run the Web API

dotnet run --project src/eCommerceOnWeb.Backend.WebApi/eCommerceOnWeb.
Backend.WebApi.csproj

Swagger UI is available at /swagger when enabled for the current 
application environment.

Use the application URL printed in the console or configured in the 
WebApi launch settings.

7. Database Configuration

The project uses separate databases for development and integration 
testing.

Database

Purpose

ecommerce_db

Main development database

ecommerce_test_db

Integration testing database

Both databases can run on the same PostgreSQL server, but integration 
tests must never modify the development database.

Integration Test Connection

The integration test project uses the following User Secrets key:

ConnectionStrings:DefaultConnection

Initialize and configure it for the integration test project:

dotnet user-secrets init --project "src/tests/eCommerceOnWeb.Backend.
Persistence.IntegrationTests/eCommerceOnWeb.Backend.Persistence.
IntegrationTests.csproj"

dotnet user-secrets set "ConnectionStrings:DefaultConnection" 
"Host=localhost;Port=5432;Database=ecommerce_test_db;Username=db_admin;
Password=YOUR_LOCAL_POSTGRES_PASSWORD" --project 
"src/tests/eCommerceOnWeb.Backend.Persistence.IntegrationTests/
eCommerceOnWeb.Backend.Persistence.IntegrationTests.csproj"

Use the same PostgreSQL credentials configured for your local Docker 
environment.

The integration test infrastructure validates the database name before 
applying migrations or running database operations.

8. Testing

The solution contains three test projects.

Domain.UnitTests

Tests domain entities, aggregates, value objects, business invariants, 
and domain events.

Run:

dotnet test src/tests/eCommerceOnWeb.Backend.Domain.UnitTests/
eCommerceOnWeb.Backend.Domain.UnitTests.csproj

Application.UnitTests

Tests application-layer behavior and request handling.

Run:

dotnet test src/tests/eCommerceOnWeb.Backend.Application.UnitTests/
eCommerceOnWeb.Backend.Application.UnitTests.csproj

Persistence.IntegrationTests

Tests EF Core persistence behavior against a dedicated PostgreSQL 
database.

Planned and developing test coverage includes:

Product creation and persistence.

Unique SKU and GTIN constraints.

Category and Brand foreign key constraints.

Soft deletion and query filters.

JSONB product attributes.

Database constraint violation handling.

The integration testing infrastructure uses a dedicated test database 
and Respawn for database cleanup.

Run the integration test project:

dotnet test src/tests/eCommerceOnWeb.Backend.Persistence.
IntegrationTests/eCommerceOnWeb.Backend.Persistence.IntegrationTests.
csproj

Ensure Docker is running and the test database connection is configured 
before running integration tests.

9. Configuration and Security

Sensitive configuration values must not be committed to source control.

Recommended practices:

Use .NET User Secrets for local development.

Keep actual .env files out of Git.

Do not commit real connection strings, passwords, or API credentials.

Use separate databases for development and integration testing.

Use dedicated credentials and configuration for production environments.

10. Future Development

Potential development areas include:

Completing PostgreSQL integration test coverage.

Expanding product and catalog functionality.

Cart and Order aggregates.

Payment processing.

Reliable domain event publishing using the Outbox Pattern.

Additional search and filtering capabilities.

Production monitoring and observability.

11. Project Status

The project is under active development.

The current implementation focuses on the Product aggregate, 
application-layer use cases, PostgreSQL persistence, database integrity, 
and automated testing.

The architecture and implementation are being developed incrementally, 
with an emphasis on practical engineering decisions and maintainable 
business logic.
