# Product API Assessment

A production-oriented **Product Management REST API** built with **ASP.NET Core 8** following **Clean Architecture** principles.

The application provides product CRUD operations, JWT-based authentication, refresh-token management, request validation, Entity Framework Core, SQL Server, Docker support, Swagger/OpenAPI documentation, and automated testing.

---

## Table of Contents

* [Overview](#overview)
* [Features](#features)
* [Tech Stack](#tech-stack)
* [Architecture](#architecture)
* [Project Structure](#project-structure)
* [Database](#database)
* [Authentication](#authentication)
* [API Endpoints](#api-endpoints)
* [Swagger / OpenAPI](#swagger--openapi)
* [Running with Docker](#running-with-docker)
* [Running Locally](#running-locally)
* [Testing](#testing)
* [Configuration](#configuration)
* [Validation and Error Handling](#validation-and-error-handling)
* [Security](#security)
* [Development Commands](#development-commands)
* [License](#license)

---

## Overview

Product API is a RESTful Web API developed using **ASP.NET Core 8**.

The project follows **Clean Architecture** to maintain separation of concerns, improve testability, and make the application easier to maintain and extend.

Core capabilities include:

* Product management
* User authentication
* JWT access tokens
* Refresh tokens
* Role-based authorization
* Request validation
* Database persistence using SQL Server
* Docker-based deployment
* Automated unit and integration testing

---

## Features

* Product CRUD operations
* Product pagination
* JWT Bearer authentication
* Access tokens
* Refresh tokens
* Refresh token revocation
* Password hashing
* Role-based authorization
* FluentValidation
* Global exception handling
* Entity Framework Core
* SQL Server
* Repository Pattern
* Unit of Work Pattern
* Swagger / OpenAPI
* CORS configuration
* Response compression
* Security response headers
* HTTPS redirection
* Docker support
* Docker Compose
* SQL Server health checks
* Entity Framework Core migrations
* Unit tests
* Integration tests

---

## Tech Stack

| Technology             | Purpose                       |
| ---------------------- | ----------------------------- |
| .NET 8                 | Backend framework             |
| ASP.NET Core Web API   | REST API                      |
| Entity Framework Core  | ORM / data access             |
| SQL Server 2022        | Database                      |
| JWT                    | Authentication                |
| FluentValidation       | Request validation            |
| Swagger / OpenAPI      | API documentation             |
| Docker                 | Containerization              |
| Docker Compose         | Multi-container orchestration |
| xUnit / Test Framework | Automated testing             |
| Clean Architecture     | Application architecture      |

---

## Architecture

The application follows **Clean Architecture** and is divided into four main layers:

### Domain

Contains the core business entities and domain concepts.

Responsibilities:

* Entities
* Enums
* Core business models

### Application

Contains application-level business logic and contracts.

Responsibilities:

* DTOs
* Interfaces
* Services
* Validators
* Configuration

### Infrastructure

Contains external dependencies and their implementations.

Responsibilities:

* Entity Framework Core
* SQL Server
* Repositories
* Unit of Work
* Database configurations
* Database migrations
* JWT token generation
* Password hashing

### API

Contains the HTTP/API layer.

Responsibilities:

* Controllers
* Middleware
* Authentication
* Authorization
* Swagger
* CORS
* Security headers
* Application startup

### Architecture Flow

```text
                Client
                   |
                   v
              +---------+
              |   API   |
              +---------+
                   |
                   v
           +---------------+
           |  Application  |
           +---------------+
                   |
                   v
              +---------+
              | Domain  |
              +---------+
                   ^
                   |
           +---------------+
           | Infrastructure|
           +---------------+
                   |
                   v
              SQL Server
```

---

## Project Structure

```text
ProductApiAssessment/
│
├── src/
│   │
│   ├── API/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   └── ProductController.cs
│   │   │
│   │   ├── Middleware/
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   │
│   │   ├── Program.cs
│   │   ├── API.csproj
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   │
│   ├── Application/
│   │   ├── Configuration/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   ├── Validators/
│   │   └── Application.csproj
│   │
│   ├── Domain/
│   │   ├── Entities/
│   │   ├── Enum/
│   │   └── Domain.csproj
│   │
│   └── Infrastructure/
│       ├── Data/
│       │   ├── Configuration/
│       │   ├── Migrations/
│       │   └── Repositories/
│       │
│       ├── Services/
│       └── Infrastructure.csproj
│
├── tests/
│   ├── API.Tests/
│   ├── Application.Tests/
│   └── Infrastructure.Tests/
│
├── Dockerfile
├── docker-compose.yml
├── ProductApiAssessment.sln
├── global.json
└── README.md
```

---

## Database

The application uses **SQL Server** with **Entity Framework Core**.

### Database

```text
ProductApiDb
```

### Main Tables

* `Product`
* `Users`
* `Item`
* `RefreshTokens`
* `__EFMigrationsHistory`

### Migrations

Current migrations include:

* `InitialCreate`
* `AddUniqueIndexes`

When running through Docker Compose, pending Entity Framework Core migrations are automatically applied when the API starts.

---

## Authentication

The API uses **JWT Bearer Authentication**.

### Authentication Endpoints

| Method | Endpoint                | Description                   |
| ------ | ----------------------- | ----------------------------- |
| POST   | `/api/v1/auth/register` | Register a new user           |
| POST   | `/api/v1/auth/login`    | Authenticate an existing user |
| POST   | `/api/v1/auth/refresh`  | Refresh an access token       |
| POST   | `/api/v1/auth/revoke`   | Revoke a refresh token        |

### Authentication Flow

```text
Register
   |
   v
User Created
   |
   v
Login
   |
   v
Access Token + Refresh Token
   |
   v
Access Token
   |
   v
Protected API Endpoints
```

Access tokens are used to access protected endpoints.

When an access token expires, a refresh token can be used to obtain a new access token. Refresh tokens can also be revoked.

---

## API Endpoints

### Authentication

| Method | Endpoint                | Description          |
| ------ | ----------------------- | -------------------- |
| `POST` | `/api/v1/auth/register` | Register a new user  |
| `POST` | `/api/v1/auth/login`    | Login                |
| `POST` | `/api/v1/auth/refresh`  | Refresh access token |
| `POST` | `/api/v1/auth/revoke`   | Revoke refresh token |

### Products

| Method   | Endpoint                | Description       |
| -------- | ----------------------- | ----------------- |
| `GET`    | `/api/v1/products`      | Get products      |
| `GET`    | `/api/v1/products/{id}` | Get product by ID |
| `POST`   | `/api/v1/products`      | Create a product  |
| `PUT`    | `/api/v1/products/{id}` | Update a product  |
| `DELETE` | `/api/v1/products/{id}` | Delete a product  |

> Protected endpoints require a valid JWT access token.

---

## Swagger / OpenAPI

Swagger is enabled for API documentation and interactive testing.

When running the application with Docker:

```text
http://localhost:8081/swagger
```

Swagger provides:

* API endpoint documentation
* Request/response models
* Authentication support
* Interactive API testing

### Authorizing Swagger

For protected endpoints:

1. Open Swagger.
2. Click **Authorize**.
3. Enter the JWT token using:

```text
Bearer <access-token>
```

---

## Running with Docker

### Prerequisites

Make sure the following are installed:

* Docker Desktop
* Docker Compose

### Start the Application

From the project root:

```bash
docker compose up --build
```

The Docker deployment contains two services:

```text
Client
   |
   v
Product API
   |
   v
SQL Server
```

### Application URLs

API:

```text
http://localhost:8081
```

Swagger:

```text
http://localhost:8081/swagger
```

SQL Server:

```text
localhost:1433
```

Docker Compose waits for the SQL Server health check before starting the API.

Pending EF Core migrations are automatically applied when the API starts.

### Run in Background

```bash
docker compose up -d --build
```

### Stop Containers

```bash
docker compose down
```

### Check Containers

```bash
docker compose ps
```

---

## Running Locally

### Prerequisites

Install:

* .NET 8 SDK
* SQL Server
* Git

### Verify .NET Installation

```bash
dotnet --version
```

### Restore Dependencies

```bash
dotnet restore
```

### Build the Solution

```bash
dotnet build
```

### Run the API

```bash
dotnet run --project src/API/API.csproj
```

Swagger will be available at the URL displayed by ASP.NET Core when the application starts.

---

## Testing

The solution contains three test projects:

```text
tests/API.Tests
tests/Application.Tests
tests/Infrastructure.Tests
```

Run all tests using:

```bash
dotnet test
```

### Current Test Results

| Test Project         |        Result |
| -------------------- | ------------: |
| Infrastructure.Tests |      1 passed |
| Application.Tests    |     20 passed |
| API.Tests            |      6 passed |
| **Total**            | **27 passed** |

```text
Passed: 27
Failed: 0
Skipped: 0
```

The tests cover application services, infrastructure functionality, and API integration behavior.

---

## Build

Build the complete solution:

```bash
dotnet build
```

Expected result:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

The solution targets **.NET 8**.

---

## Configuration

Application configuration is provided through:

```text
src/API/appsettings.json
src/API/appsettings.Development.json
```

Docker configuration is provided through:

```text
docker-compose.yml
```

Important configuration values include:

* SQL Server connection string
* JWT signing key
* JWT issuer
* JWT audience
* Access token expiration
* Refresh token expiration

### Production Secrets

Production secrets should **never be committed to source control**.

Recommended secret-management solutions include:

* Environment variables
* Docker Secrets
* Azure Key Vault
* AWS Secrets Manager
* Other secure secret-management solutions

---

## Validation and Error Handling

The API uses **FluentValidation** for request validation.

Validators are implemented for:

* Product creation requests
* Product update requests

### Global Exception Handling

Unhandled exceptions are processed through:

```text
ExceptionHandlingMiddleware
```

### HTTP Status Codes

The API uses standard HTTP status codes:

| Status                      | Meaning                                  |
| --------------------------- | ---------------------------------------- |
| `200 OK`                    | Successful request                       |
| `201 Created`               | Resource created                         |
| `204 No Content`            | Successful request with no response body |
| `400 Bad Request`           | Invalid request                          |
| `401 Unauthorized`          | Authentication required/failed           |
| `403 Forbidden`             | Access denied                            |
| `404 Not Found`             | Resource not found                       |
| `409 Conflict`              | Resource conflict                        |
| `500 Internal Server Error` | Unexpected server error                  |

Validation failures return an appropriate client error response.

---

## Security

The application implements several security mechanisms:

* JWT Bearer authentication
* Access token expiration
* Refresh token expiration
* Refresh token revocation
* Password hashing
* Authentication and authorization
* HTTPS redirection
* CORS configuration
* Request validation
* Security response headers

### Security Headers

The API includes headers such as:

```text
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Referrer-Policy: no-referrer
```

JWT signing keys and database credentials should be securely managed in production and should not be hard-coded into source code.

---

## Docker Architecture

The Docker deployment consists of two services:

```text
                    Client
                       |
                       v
                   Port 8081
                       |
                       v
              +----------------+
              |  Product API   |
              |  ASP.NET Core  |
              |     .NET 8     |
              +----------------+
                       |
                       | Port 1433
                       v
              +----------------+
              |   SQL Server   |
              |      2022      |
              +----------------+
                       |
                       v
                Persistent Volume
```

### API Container

zdone
The API container:

* Uses the .NET 8 ASP.NET runtime image
* Exposes port `8080` internally
* Maps to port `8081` on the host
* Connects to SQL Server through the Docker Compose network
* Applies EF Core migrations during startup

### SQL Server Container

The SQL Server container:

* Uses SQL Server 2022
* Exposes port `1433`
* Uses a persistent Docker volume
* Includes a health check
* Starts before the API becomes available

---

## Development Commands

### Create Migration

```bash
dotnet ef migrations add MigrationName \
  --project src/Infrastructure \
  --startup-project src/API
```

### Apply Migrations

```bash
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/API
```

### Run Tests

```bash
dotnet test
```

### Build

```bash
dotnet build
```

### Build Docker Image

```bash
docker build -t product-api .
```

### Start Docker Compose

```bash
docker compose up --build
```

### View API Logs

```bash
docker logs product-api
```

### View SQL Server Logs

```bash
docker logs product-api-sqlserver
```

---

## License

This project was created as part of a **technical assessment**.


Hand over to the HR for To check the assessment 
