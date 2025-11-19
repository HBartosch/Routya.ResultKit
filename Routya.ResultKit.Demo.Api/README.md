# Routya.ResultKit Demo API

A comprehensive demonstration API showcasing all features of the **Routya.ResultKit** and **Routya.ResultKit.AspNetCore** packages.

## Overview

This demo API demonstrates:
- ✅ **Validation**: All built-in and custom validation attributes
- ✅ **Transformation**: Object and Result transformations
- ✅ **ProblemDetails**: RFC 7807 compliant error responses
- ✅ **Exception Mapping**: Automatic exception-to-ProblemDetails conversion
- ✅ **Extensions**: Rich error context with custom extensions
- ✅ **Result Status Codes**: Semantic HTTP status codes (200 OK, 201 Created, 202 Accepted)

## Key Concepts

### Result Status Codes

`Result<T>` carries semantic intent via its `StatusCode` property:

```csharp
// Success results with different status codes
Result<User>.Ok(user)        // 200 OK
Result<User>.Created(user)   // 201 Created
Result<User>.Accepted(data)  // 202 Accepted

// Error results from ProblemDetails
Result<User>.NotFound()      // 404 Not Found
Result<User>.BadRequest()    // 400 Bad Request
```

When calling `ToActionResult()`, the correct HTTP status is automatically applied - **no manual status code needed**.

## Getting Started

### Run the API

```powershell
cd Routya.ResultKit.Demo.Api
dotnet run
```

The API will start at `http://localhost:5000` with Swagger UI at the root.

### Explore with Swagger

Open your browser to `http://localhost:5000` to access the interactive Swagger UI documentation.

## API Endpoints

### 1. Validation Controller (`/api/validation`)

Demonstrates all validation features including built-in and custom attributes.

#### Create User - `POST /api/validation/users`

**Demonstrates**: `Required`, `StringLength`, `EmailAddress`, `Range`, `MatchRegex`, and `Result.Created()` for 201 responses

**Valid Request**:
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "age": 25,
  "phoneNumber": "+1234567890"
}
```

**Success Response** (201 Created):
```json
{
  "id": 1,
  "name": "John Doe",
  "email": "john@example.com",
  "age": 25,
  "phoneNumber": "+1234567890"
}
```

**Invalid Request** (demonstrates multiple validation errors):
```json
{
  "name": "J",
  "email": "invalid-email",
  "age": 15,
  "phoneNumber": "invalid"
}
```

**Response** (422 Unprocessable Entity):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 422,
  "detail": "See the errors property for details.",
  "instance": "/api/validation/users",
  "errors": {
    "name": ["Name must be between 2 and 100 characters"],
    "email": ["Invalid email address"],
    "age": ["Age must be between 18 and 120"],
    "phoneNumber": ["Invalid phone number format"]
  }
}
```

#### Create Product - `POST /api/validation/products`

**Demonstrates**: `StringEnum`, `MinItems`, `MaxItems`, and `Result.Created()` for 201 responses

**Valid Request**:
```json
{
  "name": "Laptop",
  "description": "High-performance laptop",
  "price": 999.99,
  "stock": 50,
  "category": "Electronics",
  "tags": ["laptop", "computer", "electronics"]
}
```

**Success Response** (201 Created):
```json
{
  "id": 1,
  "name": "Laptop",
  "description": "High-performance laptop",
  "price": 999.99,
  "stock": 50,
  "category": "Electronics",
  "tags": ["laptop", "computer", "electronics"]
}
```

**Invalid Request**:
```json
{
  "name": "X",
  "description": "Too short",
  "price": -10,
  "stock": 15000,
  "category": "InvalidCategory",
  "tags": []
}
```

#### Create Order - `POST /api/validation/orders`

**Demonstrates**: `RequiredIf`, `RequiredIfEmpty`, nested validation

**Valid Request with Email**:
```json
{
  "orderDate": "2024-11-19T10:00:00",
  "deliveryDate": "2024-11-25T10:00:00",
  "totalAmount": 150.00,
  "status": "Pending",
  "email": "customer@example.com",
  "items": [
    {
      "productName": "Product A",
      "quantity": 2,
      "unitPrice": 75.00
    }
  ]
}
```

**Valid Request with Shipped Status**:
```json
{
  "orderDate": "2024-11-19T10:00:00",
  "totalAmount": 150.00,
  "status": "Shipped",
  "trackingNumber": "TRACK123456",
  "phone": "+1234567890",
  "items": [
    {
      "productName": "Product A",
      "quantity": 2,
      "unitPrice": 75.00
    }
  ]
}
```

### 2. Transformation Controller (`/api/transformation`)

Demonstrates object and result transformation features.

#### Get User as DTO - `GET /api/transformation/users/{id}/dto`

**Demonstrates**: `TransformObject` for converting domain objects to DTOs

**Request**: `GET /api/transformation/users/1/dto`

**Response** (200 OK):
```json
{
  "id": 1,
  "name": "Alice Johnson",
  "email": "alice@example.com",
  "age": 28
}
```

#### Get All Users as DTOs - `GET /api/transformation/users/all-dto`

**Demonstrates**: Collection transformation

**Response** (200 OK):
```json
[
  {
    "id": 1,
    "name": "Alice Johnson",
    "email": "alice@example.com",
    "age": 28
  },
  {
    "id": 2,
    "name": "Bob Smith",
    "email": "bob@example.com",
    "age": 35
  }
]
```

#### Get Product as DTO - `GET /api/transformation/products/{id}/dto`

**Demonstrates**: `TransformResult` with error handling

**Request**: `GET /api/transformation/products/1/dto`

**Response** (200 OK):
```json
{
  "id": 1,
  "name": "Laptop",
  "price": 999.99,
  "category": "Electronics"
}
```

#### Get Categorized Products - `GET /api/transformation/products/categorized`

**Demonstrates**: Transformation with conditional logic

**Response** (200 OK):
```json
[
  {
    "id": 1,
    "name": "Premium - Laptop",
    "price": 999.99,
    "category": "Electronics"
  },
  {
    "id": 2,
    "name": "Budget - Book",
    "price": 49.99,
    "category": "Books"
  }
]
```

#### Get User Summary - `GET /api/transformation/users/{id}/summary`

**Demonstrates**: Chained transformations

**Request**: `GET /api/transformation/users/1/summary`

**Response** (200 OK):
```json
"User #1: Alice Johnson (28 years old) - alice@example.com"
```

### 3. ProblemDetails Controller (`/api/problemdetails`)

Demonstrates all ProblemDetails types and extension features.

#### Bad Request - `GET /api/problemdetails/bad-request`

**Response** (400 Bad Request):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "The request format is invalid",
  "instance": "/api/problemdetails/bad-request",
  "expectedFormat": "JSON",
  "receivedFormat": "XML",
  "helpUrl": "https://api.example.com/docs/request-format"
}
```

#### Unauthorized - `GET /api/problemdetails/unauthorized`

**Response** (401 Unauthorized):
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication is required to access this resource",
  "instance": "/api/problemdetails/unauthorized",
  "authenticationScheme": "Bearer",
  "realm": "api.example.com"
}
```

#### Forbidden - `GET /api/problemdetails/forbidden`

**Response** (403 Forbidden):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to perform this action",
  "instance": "/api/problemdetails/forbidden",
  "requiredPermission": "admin:write",
  "currentPermissions": ["user:read", "user:write"],
  "contactSupport": "support@example.com"
}
```

#### Not Found - `GET /api/problemdetails/not-found/{resourceType}/{id}`

**Request**: `GET /api/problemdetails/not-found/user/123`

**Response** (404 Not Found):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "user with ID 123 was not found",
  "instance": "/api/problemdetails/not-found/user/123",
  "resourceType": "user",
  "resourceId": 123,
  "searchedIn": "primary database",
  "suggestion": "Verify the user ID and try again"
}
```

#### Conflict - `GET /api/problemdetails/conflict`

**Response** (409 Conflict):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "A user with this email already exists",
  "instance": "/api/problemdetails/conflict",
  "conflictingField": "email",
  "conflictingValue": "john@example.com",
  "existingResourceId": 123,
  "resolution": "Use a different email or update the existing user"
}
```

#### Validation Error - `GET /api/problemdetails/validation-error`

**Response** (422 Unprocessable Entity):
```json
{
  "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "One or more validation errors occurred",
  "instance": "/api/problemdetails/validation-error",
  "errors": {
    "name": ["Name is required", "Name must be at least 2 characters"],
    "email": ["Email is required", "Invalid email format"],
    "age": ["Age must be between 18 and 120"]
  },
  "validatedAt": "2024-11-19T10:30:00Z",
  "validationSchema": "UserSchema v1.0"
}
```

#### Server Error - `GET /api/problemdetails/server-error`

**Response** (500 Internal Server Error):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred while processing your request",
  "instance": "/api/problemdetails/server-error",
  "errorId": "550e8400-e29b-41d4-a716-446655440000",
  "timestamp": "2024-11-19T10:30:00Z",
  "supportMessage": "Please contact support with error ID: 550e8400-e29b-41d4-a716-446655440000"
}
```

#### Rich Error - `GET /api/problemdetails/rich-error`

**Demonstrates**: Multiple extensions for rich error context

**Response** (400 Bad Request):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Payment processing failed",
  "instance": "/api/problemdetails/rich-error",
  "transactionId": "txn_123456789",
  "paymentMethod": "credit_card",
  "amount": {
    "value": 99.99,
    "currency": "USD"
  },
  "failureReason": "insufficient_funds",
  "availableBalance": 45.50,
  "requiredAmount": 99.99,
  "retryable": true,
  "retryLimit": 3,
  "currentRetryCount": 1,
  "timestamp": "2024-11-19T10:30:00Z",
  "metadata": {
    "customerSegment": "premium",
    "loyaltyPoints": 1250,
    "previousTransactions": 47
  }
}
```

### 4. Exception Mapping Controller (`/api/exceptionmapping`)

Demonstrates automatic exception-to-ProblemDetails conversion.

#### Argument Exception - `GET /api/exceptionmapping/argument-exception`

**Throws**: `ArgumentException`  
**Maps to**: 400 Bad Request

**Response** (400 Bad Request):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid argument provided (Parameter 'testParameter')",
  "instance": "/api/exceptionmapping/argument-exception"
}
```

#### Unauthorized Exception - `GET /api/exceptionmapping/unauthorized-exception`

**Throws**: `UnauthorizedAccessException`  
**Maps to**: 401 Unauthorized

#### Key Not Found Exception - `GET /api/exceptionmapping/key-not-found-exception`

**Throws**: `KeyNotFoundException`  
**Maps to**: 404 Not Found

#### ProblemDetails Exception - `GET /api/exceptionmapping/problem-details-exception`

**Demonstrates**: Custom exception with rich context

**Response** (400 Bad Request):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Custom business logic error",
  "instance": "/api/exceptionmapping/problem-details-exception",
  "errorCode": "BUSINESS_RULE_VIOLATION",
  "ruleName": "MinimumOrderAmount",
  "minimumAmount": 50.00,
  "providedAmount": 25.00
}
```

#### Generic Exception - `GET /api/exceptionmapping/generic-exception`

**Throws**: Generic `Exception`  
**Maps to**: 500 Internal Server Error

**Response** (500 Internal Server Error):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred",
  "instance": "/api/exceptionmapping/generic-exception"
}
```

## Features Demonstrated

### Validation Attributes

#### Built-in Attributes
- `[Required]` - Field is required
- `[StringLength]` - Minimum and maximum length validation
- `[EmailAddress]` - Email format validation
- `[Range]` - Numeric range validation

#### Custom Attributes
- `[MatchRegex]` - Value must match regex pattern
- `[MinItems]` - Collection must have minimum items
- `[MaxItems]` - Collection must have maximum items
- `[StringEnum]` - Value must match enum member
- `[RequiredIf]` - Required when another field has specific value
- `[RequiredIfEmpty]` - Required when another field is empty

### Transformation Features

- **TransformObject**: Direct object-to-object transformation
- **TransformResult**: Result-aware transformation with error handling
- **Chained Transformations**: Multiple transformations in sequence
- **Collection Transformations**: Transform lists and collections
- **Conditional Transformations**: Apply logic during transformation

### ProblemDetails Features

- **RFC 7807 Compliance**: Standard problem details format
- **Status Codes**: 400, 401, 403, 404, 409, 422, 500, and custom
- **Extensions**: Add unlimited custom properties
- **Validation Errors**: Structured validation error reporting
- **Instance Paths**: Automatic request path tracking

### Exception Mapping

- **Default Mappers**: Built-in mappers for common exceptions
  - `ArgumentException` → 400
  - `ArgumentNullException` → 400
  - `UnauthorizedAccessException` → 401
  - `InvalidOperationException` → 400
  - `KeyNotFoundException` → 404
  - `NotSupportedException` → 400
  - Generic `Exception` → 500
- **Custom Exceptions**: `ProblemDetailsException` for rich errors
- **Automatic Conversion**: Middleware handles conversion
- **Error Sanitization**: Production-safe error messages

## Project Structure

```
Routya.ResultKit.Demo.Api/
├── Controllers/
│   ├── ValidationController.cs           # Validation examples
│   ├── TransformationController.cs       # Transformation examples
│   ├── ProblemDetailsController.cs       # ProblemDetails examples
│   └── ExceptionMappingController.cs     # Exception mapping examples
├── Models/
│   └── Models.cs                         # Domain models and DTOs
├── Properties/
│   └── launchSettings.json
├── Program.cs                            # App configuration
├── appsettings.json
└── Routya.ResultKit.Demo.Api.csproj
```

## Technologies Used

- .NET 8.0
- ASP.NET Core Web API
- Routya.ResultKit 2.0.0
- Routya.ResultKit.AspNetCore 2.0.0
- Swashbuckle.AspNetCore 6.5.0 (Swagger/OpenAPI)

## Learn More

- [Routya.ResultKit GitHub Repository](https://github.com/HBartosch/Routya.ResultKit)
- [RFC 7807 - Problem Details](https://tools.ietf.org/html/rfc7807)

## Testing Tips

1. **Use Swagger UI**: Open `http://localhost:5000` for interactive testing
2. **Try Invalid Data**: Submit invalid requests to see validation in action
3. **Observe ProblemDetails**: Check the standardized error format
4. **Test Exceptions**: Trigger exceptions to see automatic mapping
5. **Check Extensions**: Notice the custom properties in responses

## Notes

- All data is stored in-memory and resets when the application restarts
- Use the `DELETE /api/validation/clear` endpoint to reset validation data
- Exception mapping middleware is enabled by default
- Swagger UI is available at the root URL for easy exploration
