

# SuperMart
Projects:
- SuperMartApp.Api → ASP.NET Core Web API
- SuperMartApp.Infrastructure → EF Core + Postgres
- SuperMartApp.Tests → xUnit + FluentAssertions

How to run:
dotnet build
dotnet run --project SuperMartApp.Api
dotnet test

# SuperMart (ASP.NET Core 8)

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