# Contributing (C# / .NET)

## Branching
- Create a feature branch from main:
  git checkout -b feature/<name>

## Coding
- ASP.NET Core 8 + EF Core (Npgsql)
- Validate inputs; return RFC7807 ProblemDetails for 4xx errors
- Store DateTime in **UTC** (server-side)

## Tests
- xUnit + FluentAssertions
- Every feature needs unit tests (and integration tests for endpoints)

## Pull Requests
- Keep PRs small, focused, and passing:
  dotnet build && dotnet test
- Update Swagger/OpenAPI when endpoints change
