# Routya.ResultKit Demo API - Quick Start

## Launch the Demo

The demo API is now ready to explore! It showcases all features of Routya.ResultKit.

### Start the API

```powershell
cd Routya.ResultKit.Demo.Api
dotnet run
```

The API will start at **http://localhost:5000** with Swagger UI at the root.

### Open Swagger UI

Navigate to **http://localhost:5000** in your browser to access the interactive API documentation.

## Quick Examples

### 1. Validation Example

Try creating a user with validation:

**POST http://localhost:5000/api/validation/users**

```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "age": 25,
  "phoneNumber": "+1234567890"
}
```

Try an invalid request to see validation errors:

```json
{
  "name": "J",
  "email": "invalid",
  "age": 15
}
```

### 2. Transformation Example

Get a transformed user DTO:

**GET http://localhost:5000/api/transformation/users/1/dto**

### 3. ProblemDetails Example

See rich error responses:

**GET http://localhost:5000/api/problemdetails/not-found/user/123**

**GET http://localhost:5000/api/problemdetails/rich-error**

### 4. Exception Handling

See automatic exception-to-ProblemDetails conversion:

**GET http://localhost:5000/api/exceptionmapping/argument-exception**

**GET http://localhost:5000/api/exceptionmapping/unauthorized-exception**

## Features Demonstrated

✅ **Validation** - All built-in and custom validation attributes  
✅ **Transformation** - Object and Result transformations  
✅ **ProblemDetails** - RFC 7807 compliant error responses  
✅ **Exception Mapping** - Automatic exception handling  
✅ **Extensions** - Rich error context with custom properties  
✅ **Result Status Codes** - Semantic HTTP responses (200 OK, 201 Created, 202 Accepted)  

## Key Concepts

### Result Status Codes

The demo shows how `Result<T>` carries semantic HTTP status codes:

```csharp
// POST endpoints return 201 Created automatically
Result<User>.Created(user).ToActionResult()  // Returns 201 Created

// GET endpoints return 200 OK
Result<User>.Ok(user).ToActionResult()       // Returns 200 OK

// No manual status code needed - it's carried by the Result!
```  

## Next Steps

- Explore all endpoints in Swagger UI
- Try different validation scenarios
- See how transformations chain together
- Test exception handling
- Review the source code in the Controllers folder

Enjoy exploring Routya.ResultKit! 🚀
