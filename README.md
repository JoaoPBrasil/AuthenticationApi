# Authentication API

A RESTful authentication API built with **C# and ASP.NET Core**, focused on authentication, authorization, security, and clean backend architecture.

The project was developed as part of my backend development portfolio to practice real-world authentication concepts beyond basic CRUD operations.

## Features

- User registration
- User login
- Password hashing with BCrypt
- JWT access tokens
- Refresh tokens
- Refresh token rotation
- Refresh token revocation
- Logout
- Role-based authorization
- Protected endpoints
- Input validation with FluentValidation
- Global exception handling
- PostgreSQL persistence
- Entity Framework Core migrations
- Swagger/OpenAPI documentation
- Unit tests with xUnit and Moq

## Technologies

- **C#**
- **.NET 8**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **PostgreSQL**
- **JWT Bearer Authentication**
- **BCrypt**
- **FluentValidation**
- **Swagger / OpenAPI**
- **xUnit**
- **Moq**
- **Git / GitHub**

## Architecture

The project follows a layered architecture with clear separation of responsibilities:

```text
AuthApi
│
├── AuthApi.API
│   ├── Controllers
│   ├── Middleware
│   └── Program.cs
│
├── AuthApi.Application
│   ├── Exceptions
│   ├── Security
│   └── UseCases
│
├── AuthApi.Domain
│   ├── Entities
│   ├── Enums
│   └── Repositories
│
├── AuthApi.Infrastructure
│   ├── Data
│   ├── Migrations
│   └── Repositories
│
└── AuthApi.Tests
    └── UseCases
```

### Layer responsibilities

**API**

Responsible for HTTP communication, controllers, authentication configuration, authorization, Swagger, and global exception handling.

**Application**

Contains application use cases, validation, authentication services, password hashing, JWT generation, and refresh token generation.

**Domain**

Contains the core business entities, enums, and repository contracts. The Domain layer has no dependency on Infrastructure or external application concerns.

**Infrastructure**

Responsible for persistence, Entity Framework Core, PostgreSQL, database migrations, repository implementations, and database initialization.

**Tests**

Contains automated unit tests for the application's main authentication use cases.

## Authentication Flow

### Registration

```text
Client
  │
  ▼
POST /auth/register
  │
  ▼
Input Validation
  │
  ▼
Password Hashing
  │
  ▼
User Creation
  │
  ▼
PostgreSQL
```

### Login

```text
Client
  │
  ▼
POST /auth/login
  │
  ▼
Validate Credentials
  │
  ▼
Verify Password
  │
  ├──────────────┐
  ▼              ▼
JWT Access     Refresh
Token          Token
  │              │
  └──────┬───────┘
         ▼
       Client
```

### Refresh Token Rotation

```text
Client
  │
  ▼
POST /auth/refresh
  │
  ▼
Validate Refresh Token
  │
  ▼
Revoke Current Token
  │
  ├──────────────┐
  ▼              ▼
New Access     New Refresh
Token          Token
  │              │
  └──────┬───────┘
         ▼
       Client
```

The previous refresh token becomes invalid after rotation, preventing it from being reused.

## Endpoints

| Method | Endpoint | Description | Authentication |
|---|---|---|---|
| POST | `/auth/register` | Register a new user | Public |
| POST | `/auth/login` | Authenticate a user | Public |
| POST | `/auth/refresh` | Generate new access and refresh tokens | Public* |
| POST | `/auth/logout` | Revoke a refresh token | Public* |
| GET | `/auth/me` | Get authenticated user information | Required |
| GET | `/auth/admin` | Access an administrator-only resource | Admin |

\* The refresh and logout endpoints require a valid refresh token in the request body.

## Authorization

The API uses JWT claims to identify authenticated users and their roles.

Currently, the project contains two roles:

- `User`
- `Admin`

Protected endpoints use ASP.NET Core's authorization system.

For example:

```csharp
[Authorize]
```

requires an authenticated user, while:

```csharp
[Authorize(Roles = "Admin")]
```

restricts access to users with the `Admin` role.

## Security

The project implements several security-related practices:

- Passwords are never stored as plain text.
- Passwords are hashed using BCrypt.
- JWT tokens are signed using a secret key.
- JWT issuer and audience are validated.
- JWT expiration is validated.
- Refresh tokens are stored in the database.
- Refresh tokens expire after a defined period.
- Refresh tokens can be revoked.
- Refresh token rotation prevents reuse of previously consumed tokens.
- Sensitive configuration is stored using .NET User Secrets during local development.
- Database credentials and JWT secrets are not stored in the repository.
- Invalid authentication attempts return a generic error message.

## Validation and Error Handling

Request validation is implemented using **FluentValidation**.

The API validates inputs such as:

- Required fields
- Email format
- Password length
- Refresh token presence

Application exceptions are handled by a global middleware that converts expected application errors into appropriate HTTP responses.

Examples include:

- `400 Bad Request` — invalid input
- `401 Unauthorized` — invalid authentication credentials or token
- `403 Forbidden` — authenticated user without sufficient permissions
- `409 Conflict` — resource conflict, such as an already registered email
- `500 Internal Server Error` — unexpected server errors

## Database

The API uses **PostgreSQL** with **Entity Framework Core**.

The main entities are:

- `User`
- `RefreshToken`

Database changes are managed through Entity Framework Core migrations.

The database also enforces unique indexes for:

- User email
- Refresh token value

## Testing

The project includes automated unit tests using **xUnit** and **Moq**.

The tests cover important authentication scenarios, including:

- Successful user registration
- Duplicate email registration
- Invalid registration data
- Successful login
- Invalid credentials
- Inactive users
- Password verification
- Login timestamp recording
- Valid refresh token rotation
- Expired refresh tokens
- Revoked refresh tokens
- Refresh token reuse
- Logout and token revocation

Run the test suite with:

```bash
dotnet test
```

## Configuration

Sensitive configuration is managed through **.NET User Secrets** during local development.

The application requires configuration for:

- PostgreSQL connection string
- JWT secret key
- JWT issuer
- JWT audience
- JWT expiration
- Admin seed credentials

These values should not be committed to the repository.

## Running the Project

### Requirements

- .NET 8 SDK
- PostgreSQL
- Visual Studio or another compatible .NET IDE

### 1. Clone the repository

```bash
git clone https://github.com/JoaoPBrasil/AuthenticationApi-teste.git
cd AuthenticationApi-teste
```

### 2. Configure User Secrets

From the `AuthApi.API` project:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING"
dotnet user-secrets set "JwtSettings:SecretKey" "YOUR_SECRET_KEY"
dotnet user-secrets set "JwtSettings:Issuer" "AuthApi"
dotnet user-secrets set "JwtSettings:Audience" "AuthApiClient"
dotnet user-secrets set "JwtSettings:ExpirationMinutes" "15"
dotnet user-secrets set "AdminSeed:Email" "YOUR_ADMIN_EMAIL"
dotnet user-secrets set "AdminSeed:Password" "YOUR_ADMIN_PASSWORD"
```

### 3. Apply the database migrations

From the `AuthApi.API` directory:

```bash
dotnet ef database update --project ../AuthApi.Infrastructure --startup-project .
```

### 4. Run the API

```bash
dotnet run --project AuthApi.API
```

Once the application is running, Swagger can be used to explore and test the API.

## Project Goals

The main goal of this project was to move beyond basic CRUD APIs and practice backend concepts commonly used in authentication systems.

The project focuses on:

- Authentication and authorization
- Secure password handling
- Token-based authentication
- Refresh token lifecycle management
- Layered architecture
- Repository abstraction
- Database persistence
- Automated testing
- Secure configuration management

## What I Practiced

Through this project, I practiced designing and implementing an authentication system from the ground up, including the interaction between the API, application logic, domain entities, persistence layer, and authentication mechanisms.

The project also helped reinforce concepts such as dependency injection, asynchronous programming, Entity Framework Core, middleware, JWT claims, authorization policies, validation, unit testing, and secure application configuration.

## License

This project was created for educational and portfolio purposes.

## Author

João Pedro Oliveira Brasil

Back-End Developer

LinkedIn: [https://www.linkedin.com/in/joao-brasil/](https://www.linkedin.com/in/joao-brasil/)

GitHub: [https://github.com/JoaoPBrasil](https://github.com/JoaoPBrasil)