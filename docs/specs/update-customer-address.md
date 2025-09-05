# Feature: Add Address to Customer entity

## Goal
Extend the existing `Customer` entity to include an `Address` object.

## Address object
Address {
  Street: string (max 120)
  City: string (max 80)
  State: string (max 80)
  Country: string (max 80)
  Zipcode: string (max 20)
}

## Requirements
- Update `Customer` entity class to include an `Address` property.
- Create a new `Address` class in the domain/entities layer.
- Update EF Core configuration (DbContext + Migrations).
- Ensure existing Customer Create API supports saving the Address fields.
- Validation:
  - Street, City, Country: required
  - State, Zipcode: optional

## Persistence
- EF Core should map Address as an **owned entity** (Customer → Address).
- Database schema must update accordingly.

## Tests
- Unit test: creating a Customer with an Address persists correctly.
- Integration test: POST /api/customers with Address returns 201 and saves in DB.
- Validation test: missing required fields should return 400 ProblemDetails.

## Non-goals
- No new controller or endpoint — reuse the existing Customer Create API.
- No changes to authentication/authorization.
