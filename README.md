# SuperMartApp

This repository contains the SuperMart application, built with ASP.NET Core 8 and Entity Framework Core.

## Solution Structure
- **SuperMartApp.Api** → ASP.NET Core Web API (controllers, endpoints, middleware).
- **SuperMartApp.Infrastructure** → EF Core + PostgreSQL (DbContext, Migrations, Repositories).
- **SuperMartApp.Tests** → Unit + Integration tests (xUnit, FluentAssertions).

## Getting Started
1. **Build the solution**
   ```bash
    dotnet build
    dotnet run --project SuperMartApp.Api
    dotnet test

## Run
dotnet restore
dotnet build
dotnet ef database update   # if using EF Core migrations
dotnet run --project src/Web

## Tech
ASP.NET Core 8, EF Core, PostgreSQL, xUnit/FluentAssertions

## Config
Put secrets with `dotnet user-secrets` or `appsettings.Development.json` (not committed).
## Tests
Run with `dotnet test`
