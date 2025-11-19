# Migration Guide: v1.x to v2.0

This guide will help you migrate from Routya.ResultKit v1.x to v2.0, which introduces full RFC 7807 Problem Details compliance and ASP.NET Core integration.

## Table of Contents

- [Overview of Changes](#overview-of-changes)
- [Breaking Changes](#breaking-changes)
- [Migration Steps](#migration-steps)
- [New Features](#new-features)
- [ASP.NET Core Integration](#aspnet-core-integration)
- [Code Examples](#code-examples)
- [Upgrade Checklist](#upgrade-checklist)

## Overview of Changes

Version 2.0 is a major upgrade that brings:

✅ **RFC 7807 Compliance**: Full support for RFC 7807 Problem Details standard  
✅ **Rich ProblemDetails**: Added `Type`, `Detail`, and `Instance` properties  
✅ **Type-Safe Extensions**: New `SetExtension<T>()` and `TryGetExtension<T>()` methods  
✅ **Fluent Builder API**: `ProblemDetailsBuilder` for easy construction  
✅ **Standard Problem Types**: Predefined URIs following RFC 9110  
✅ **ASP.NET Core Package**: Separate package with middleware, exception handling, and IResult/IActionResult extensions  
✅ **Improved Serialization**: Custom JSON converter with configurable naming policies  

## Breaking Changes

### 1. ProblemDetails.Extensions Property

**Old (v1.x):**
```csharp
public Dictionary<string, object?> Extensions { get; set; } = new Dictionary<string, object?>();

// Usage
var problem = new ProblemDetails
{
    Title = "Validation Failed",
    Status = 400,
    Extensions = { ["errors"] = errors }
};

// Access
var errors = (IDictionary<string, string[]>)problem.Extensions["errors"];
```

**New (v2.0):**
```csharp
// Extensions is now internal storage accessed via methods
public void SetExtension<T>(string key, T value);
public bool TryGetExtension<T>(string key, out T? value);

// Usage
var problem = new ProblemDetails
{
    Title = "Validation Failed",
    Status = 400
};
problem.SetExtension("errors", errors);

// Access
if (problem.TryGetExtension<IDictionary<string, string[]>>("errors", out var errors))
{
    // Use errors
}
```

**Why?** This enables proper RFC 7807 serialization where extension members appear as top-level JSON properties, not nested under an `Extensions` key.

### 2. Result<T>.Fail() Method Signature

**Old (v1.x):**
```csharp
Result<User>.Fail("Validation Failed", 400, errors);
```

**New (v2.0):**
```csharp
// Option 1: Use new specific factory methods (RECOMMENDED)
Result<User>.ValidationFailed(errors);
Result<User>.NotFound("User not found");
Result<User>.BadRequest("Invalid request");

// Option 2: Use obsolete method (will be removed in v3.0)
Result<User>.Fail("Validation Failed", 400, errors); // Obsolete warning

// Option 3: Use ProblemDetailsBuilder
var problem = ProblemDetailsBuilder.ValidationError(errors).Build();
Result<User>.Fail(problem);
```

**Why?** The new factory methods provide clearer intent, automatic RFC 7807 problem type URIs, and proper HTTP status codes.

### 3. ProblemDetails Initialization

**Old (v1.x):**
```csharp
var problem = new ProblemDetails
{
    Title = "Not Found",
    Status = 404,
    Extensions = { ["resourceId"] = userId }
};
```

**New (v2.0):**
```csharp
// Option 1: Using builder (RECOMMENDED)
var problem = ProblemDetailsBuilder.NotFound("User not found")
    .WithInstance($"/api/users/{userId}")
    .WithExtension("resourceId", userId)
    .Build();

// Option 2: Using init properties + SetExtension
var problem = new ProblemDetails
{
    Type = StandardProblemTypes.NotFound,
    Title = "Not Found",
    Status = 404,
    Detail = "User not found",
    Instance = $"/api/users/{userId}"
};
problem.SetExtension("resourceId", userId);
```

### 4. JSON Serialization Output

**Old (v1.x):**
```json
{
  "Title": "Validation Failed",
  "Status": 400,
  "Extensions": {
    "errors": {
      "email": ["Email is required"]
    }
  }
}
```

**New (v2.0):**
```json
{
  "type": "urn:problem-type:validation-error",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "email": ["Email is required"]
  }
}
```

**Why?** This matches the RFC 7807 standard where extensions are top-level properties and property names use camelCase by default.

## Migration Steps

### Step 1: Update Package References

Update your `.csproj` file:

```xml
<ItemGroup>
  <!-- Update from v1.x to v2.0 -->
  <PackageReference Include="Routya.ResultKit" Version="2.0.0" />
  
  <!-- Optional: Add ASP.NET Core integration (requires .NET 7+) -->
  <PackageReference Include="Routya.ResultKit.AspNetCore" Version="2.0.0" />
</ItemGroup>
```

### Step 2: Update Validation Error Handling

**Before:**
```csharp
var result = request.Validate();
if (!result.Success)
{
    var errors = (IDictionary<string, string[]>)result.Error.Extensions["errors"];
    // Process errors
}
```

**After:**
```csharp
var result = request.Validate();
if (!result.Success)
{
    if (result.Error.TryGetExtension<IDictionary<string, string[]>>("errors", out var errors))
    {
        // Process errors
    }
}
```

### Step 3: Replace Result.Fail() Calls

Find all usages of `Result<T>.Fail(title, statusCode, errors)` and replace with specific factory methods:

**Before:**
```csharp
return Result<User>.Fail("Not Found", 404);
return Result<User>.Fail("Validation Failed", 400, errors);
return Result<User>.Fail("Unauthorized", 401);
```

**After:**
```csharp
return Result<User>.NotFound("User not found");
return Result<User>.ValidationFailed(errors);
return Result<User>.Unauthorized("Authentication required");
```

### Step 4: Update Custom ProblemDetails Creation

**Before:**
```csharp
var problem = new ProblemDetails
{
    Title = "Business Rule Violation",
    Status = 422,
    Extensions = { ["rule"] = "MaxOrderLimit" }
};
return Result<Order>.Fail(problem);
```

**After:**
```csharp
var problem = new ProblemDetailsBuilder()
    .WithType(StandardProblemTypes.BusinessRuleViolation)
    .WithTitle("Business Rule Violation")
    .WithStatus(422)
    .WithDetail("Order exceeds maximum limit")
    .WithExtension("rule", "MaxOrderLimit")
    .Build();
return Result<Order>.Fail(problem);
```

### Step 5: Update TransformationExtensions Usage

No changes required! The updated `Transform()` method now properly preserves all extension members automatically.

```csharp
// Works the same in both versions
var result = request.Validate()
    .Transform(r => new CreateUserCommand { Name = r.Name });
```

## New Features

### 1. Standard Problem Types

Use predefined problem types with proper URIs:

```csharp
using Routya.ResultKit.ProblemTypes;

// RFC 9110 URIs for HTTP errors
StandardProblemTypes.BadRequest          // 400
StandardProblemTypes.Unauthorized        // 401
StandardProblemTypes.Forbidden           // 403
StandardProblemTypes.NotFound            // 404
StandardProblemTypes.Conflict            // 409
StandardProblemTypes.InternalServerError // 500

// Domain-specific types (customizable base URI)
StandardProblemTypes.ValidationError
StandardProblemTypes.BusinessRuleViolation
StandardProblemTypes.ResourceAlreadyExists
StandardProblemTypes.OperationNotPermitted

// Custom domain types
StandardProblemTypes.Custom("user-quota-exceeded")
```

### 2. Configure Problem Type Base URI

```csharp
using Routya.ResultKit.ProblemTypes;

// Set custom base URI for domain-specific problem types
StandardProblemTypes.DefaultBaseUri = "https://api.example.com/problems/";

// Now domain types use your URI
StandardProblemTypes.ValidationError 
// => "https://api.example.com/problems/validation-error"
```

### 3. Fluent ProblemDetails Builder

```csharp
using Routya.ResultKit.Builders;

var problem = new ProblemDetailsBuilder()
    .WithType("https://example.com/problems/insufficient-funds")
    .WithTitle("Insufficient Funds")
    .WithStatus(400)
    .WithDetail($"Account balance is ${balance}, but ${amount} was requested")
    .WithInstance($"/api/accounts/{accountId}/withdraw")
    .WithExtension("accountId", accountId)
    .WithExtension("availableBalance", balance)
    .WithExtension("requestedAmount", amount)
    .Build();

return Result<Transaction>.Fail(problem);
```

### 4. Factory Methods on Builder

```csharp
// Quick creation of common problem types
var problem = ProblemDetailsBuilder.NotFound("Resource not found")
    .WithInstance("/api/users/123")
    .WithExtension("userId", 123)
    .Build();

var problem = ProblemDetailsBuilder.ValidationError(errors)
    .WithInstance(context.Request.Path)
    .Build();
```

### 5. Type-Safe Extension Access

```csharp
// Set any type
problem.SetExtension("userId", 123);
problem.SetExtension("metadata", new { created = DateTime.UtcNow });
problem.SetExtension("tags", new[] { "important", "urgent" });

// Type-safe retrieval
if (problem.TryGetExtension<int>("userId", out var userId))
{
    Console.WriteLine($"User ID: {userId}");
}

if (problem.TryGetExtension<string[]>("tags", out var tags))
{
    Console.WriteLine($"Tags: {string.Join(", ", tags)}");
}
```

## ASP.NET Core Integration

### Installation

```bash
dotnet add package Routya.ResultKit.AspNetCore
```

**Requirements:** .NET 7, .NET 8, .NET 9, or .NET 10

### Basic Setup

```csharp
using Routya.ResultKit.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add ResultKit services
builder.Services.AddResultKitProblemDetails(options =>
{
    options.ProblemTypeBaseUri = "https://api.example.com/problems/";
    options.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    options.IncludeTraceId = true;
    options.TraceIdExtensionName = "traceId";
});

var app = builder.Build();

// Add exception handler middleware (IMPORTANT: Add early in pipeline)
app.UseResultKitExceptionHandler();

app.MapControllers();
app.Run();
```

### Minimal API Integration

```csharp
app.MapPost("/users", (CreateUserRequest request, HttpContext context) =>
{
    var result = request.Validate();
    
    // Automatic conversion to IResult with proper content-type
    return result.ToHttpResult(context);
});

// Returns on success:
// HTTP 200 OK
// Content-Type: application/json
// { "id": 1, "name": "John" }

// Returns on validation failure:
// HTTP 400 Bad Request
// Content-Type: application/problem+json
// {
//   "type": "urn:problem-type:validation-error",
//   "title": "Validation Failed",
//   "status": 400,
//   "detail": "One or more validation errors occurred.",
//   "instance": "/users",
//   "errors": { "name": ["Name is required"] }
// }
```

### MVC Controller Integration

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost]
    public IActionResult CreateUser([FromBody] CreateUserRequest request)
    {
        var result = request.Validate()
            .Transform(r => new User { Name = r.Name, Email = r.Email });
        
        return result.ToActionResult(HttpContext);
    }
    
    [HttpGet("{id}")]
    public IActionResult GetUser(int id)
    {
        var user = _repository.FindById(id);
        
        if (user == null)
        {
            return Result<User>.NotFound($"User with ID {id} not found", $"/api/users/{id}")
                .ToActionResult();
        }
        
        return Result<User>.Ok(user).ToActionResult();
    }
}
```

### Exception Handling

**Automatic exception-to-ProblemDetails conversion:**

```csharp
app.MapGet("/users/{id}", (int id) =>
{
    // Any unhandled exception is automatically converted to ProblemDetails
    if (id <= 0)
        throw new ArgumentException("ID must be positive", nameof(id));
    
    // Becomes:
    // HTTP 400 Bad Request
    // {
    //   "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
    //   "title": "Bad Request",
    //   "status": 400,
    //   "detail": "ID must be positive (Parameter 'id')",
    //   "instance": "/users/-1",
    //   "traceId": "00-abc123..."  // if IncludeTraceId = true
    // }
    
    return Results.Ok(new User { Id = id });
});
```

**Custom exception with ProblemDetails:**

```csharp
using Routya.ResultKit.AspNetCore.Exceptions;
using Routya.ResultKit.ProblemTypes;

public class InsufficientFundsException : ProblemDetailsException
{
    public InsufficientFundsException(decimal available, decimal requested)
        : base(
            type: StandardProblemTypes.Custom("insufficient-funds"),
            title: "Insufficient Funds",
            status: 400,
            detail: $"Available balance ${available} is less than requested ${requested}")
    {
        Extensions["availableBalance"] = available;
        Extensions["requestedAmount"] = requested;
    }
}

// Usage
if (account.Balance < amount)
    throw new InsufficientFundsException(account.Balance, amount);
```

**Custom exception mapper:**

```csharp
using Routya.ResultKit.AspNetCore.Mappers;

public class UserNotFoundExceptionMapper : IExceptionMapper
{
    public bool CanHandle(Exception exception) 
        => exception is UserNotFoundException;
    
    public ProblemDetails Map(Exception exception, HttpContext context)
    {
        var ex = (UserNotFoundException)exception;
        return ProblemDetailsBuilder.NotFound(ex.Message)
            .WithInstance(context.Request.Path)
            .WithExtension("userId", ex.UserId)
            .Build();
    }
}

// Register in Startup
builder.Services.AddResultKitProblemDetails();
builder.Services.AddExceptionMapper(new UserNotFoundExceptionMapper());
```

## Code Examples

### Complete Before/After Example

**Before (v1.x):**

```csharp
public class UserService
{
    public Result<User> CreateUser(CreateUserRequest request)
    {
        var validationResult = request.Validate();
        if (!validationResult.Success)
            return validationResult.Transform(r => new User());
        
        var existing = _repository.FindByEmail(request.Email);
        if (existing != null)
        {
            var problem = new ProblemDetails
            {
                Title = "Conflict",
                Status = 409,
                Extensions = { ["email"] = request.Email }
            };
            return Result<User>.Fail(problem);
        }
        
        var user = new User { Name = request.Name, Email = request.Email };
        _repository.Add(user);
        
        return Result<User>.Ok(user);
    }
}

// Controller
[HttpPost]
public IActionResult CreateUser([FromBody] CreateUserRequest request)
{
    var result = _service.CreateUser(request);
    
    if (!result.Success)
        return BadRequest(result);
    
    return Ok(result.Data);
}
```

**After (v2.0):**

```csharp
public class UserService
{
    public Result<User> CreateUser(CreateUserRequest request)
    {
        var validationResult = request.Validate();
        if (!validationResult.Success)
            return validationResult.Transform(r => new User());
        
        var existing = _repository.FindByEmail(request.Email);
        if (existing != null)
        {
            return Result<User>.Conflict("A user with this email already exists")
                .WithExtension("email", request.Email);
        }
        
        var user = new User { Name = request.Name, Email = request.Email };
        _repository.Add(user);
        
        return Result<User>.Ok(user);
    }
}

// Controller (with ASP.NET Core package)
[HttpPost]
public IActionResult CreateUser([FromBody] CreateUserRequest request)
{
    var result = _service.CreateUser(request);
    return result.ToActionResult(HttpContext);
}

// OR Minimal API
app.MapPost("/users", (CreateUserRequest request, UserService service, HttpContext context) =>
{
    return service.CreateUser(request).ToHttpResult(context);
});
```

## Upgrade Checklist

- [ ] Update `Routya.ResultKit` package reference to v2.0.0
- [ ] Find all uses of `Result<T>.Fail(title, statusCode, errors)` and replace with specific factory methods
- [ ] Update all `problem.Extensions["key"]` accesses to use `TryGetExtension<T>("key", out var value)`
- [ ] Update custom `ProblemDetails` creation to use `ProblemDetailsBuilder`
- [ ] Review JSON serialization expectations (extensions are now top-level properties)
- [ ] Configure `StandardProblemTypes.DefaultBaseUri` if using custom domain
- [ ] (Optional) Install `Routya.ResultKit.AspNetCore` package for ASP.NET Core projects
- [ ] (Optional) Add `AddResultKitProblemDetails()` and `UseResultKitExceptionHandler()` to ASP.NET Core app
- [ ] (Optional) Replace manual error handling in controllers with `.ToHttpResult()` or `.ToActionResult()`
- [ ] Test API responses to ensure RFC 7807 compliance
- [ ] Update API documentation to reflect new ProblemDetails schema

## Need Help?

- **GitHub Issues**: https://github.com/HBartosch/Routya.ResultKit/issues
- **RFC 7807 Specification**: https://datatracker.ietf.org/doc/html/rfc7807
- **RFC 9110 HTTP Semantics**: https://datatracker.ietf.org/doc/html/rfc9110

## Summary

Version 2.0 brings Routya.ResultKit into full RFC 7807 compliance while maintaining the clean, fluent API you love. The migration primarily involves:

1. Using new factory methods (`NotFound()`, `ValidationFailed()`, etc.)
2. Accessing extensions via `TryGetExtension<T>()` instead of dictionary indexing
3. (Optional) Adopting ASP.NET Core integration for automatic middleware and conversions

The obsolete `Fail(title, statusCode, errors)` method remains available in v2.0 with a compiler warning, giving you time to migrate gradually. It will be removed in v3.0.
