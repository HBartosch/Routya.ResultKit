# Routya.ResultKit.AspNetCore

ASP.NET Core integration for [Routya.ResultKit](https://github.com/HBartosch/Routya.ResultKit) providing seamless integration with Microsoft's ProblemDetails, automatic exception handling middleware, and IResult/IActionResult extensions for Minimal APIs and MVC controllers.

## Features

✅ **Automatic Exception Handling** - Global middleware that converts exceptions to RFC 7807 ProblemDetails  
✅ **Microsoft ProblemDetails Integration** - Bidirectional conversion with `Microsoft.AspNetCore.Http.ProblemDetails`  
✅ **Minimal API Support** - `ToHttpResult()` extension for seamless `IResult` conversion  
✅ **MVC Controller Support** - `ToActionResult()` extension for `IActionResult` conversion  
✅ **Custom Exception Mappers** - Register custom exception-to-ProblemDetails mappers  
✅ **Configurable Options** - Control trace IDs, exception details, naming policies, and more  
✅ **RFC 7807 Compliant** - Automatic `application/problem+json` content type  

## Requirements

- .NET 7, .NET 8, or .NET 9
- `Routya.ResultKit` v2.0.0+

## Installation

```bash
dotnet add package Routya.ResultKit.AspNetCore --version 2.1.0
```

## Quick Start

### 1. Register Services

```csharp
using Routya.ResultKit.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add ResultKit services
builder.Services.AddResultKitProblemDetails(options =>
{
    options.ProblemTypeBaseUri = "https://api.example.com/problems/";
    options.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    options.IncludeTraceId = true;
});

var app = builder.Build();

// Add exception handler middleware (early in pipeline)
app.UseResultKitExceptionHandler();

app.MapControllers();
app.Run();
```

### 2. Use in Minimal APIs

```csharp
app.MapPost("/users", (CreateUserRequest request, HttpContext context) =>
{
    var result = request.Validate();
    return result.ToHttpResult(context);
});
```

**Success Response (200 OK):**
```json
{
  "id": 1,
  "name": "John Doe",
  "email": "john@example.com"
}
```

**Validation Error Response (400 Bad Request):**
```http
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{
  "type": "urn:problem-type:validation-error",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/users",
  "errors": {
    "email": ["The Email field is required."],
    "name": ["The Name field is required."]
  },
  "traceId": "00-abc123..."
}
```

### 3. Use in MVC Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult GetUser(int id)
    {
        var user = _repository.FindById(id);
        
        if (user == null)
            return Result<User>.NotFound($"User with ID {id} not found")
                .ToActionResult(HttpContext);
        
        return Result<User>.Ok(user).ToActionResult();
    }
    
    [HttpPost]
    public IActionResult CreateUser(CreateUserRequest request)
    {
        var validationResult = request.Validate();
        if (!validationResult.Success)
            return validationResult.ToActionResult(HttpContext);
        
        var user = _repository.Create(request);
        
        // Result.Created() automatically sets 201 status code
        return Result<User>.Created(user).ToActionResult(HttpContext);
    }
    
    [HttpPost("{id}/process")]
    public IActionResult ProcessUser(int id)
    {
        _processor.QueueForProcessing(id);
        
        // Result.Accepted() automatically sets 202 status code
        return Result<object>.Accepted(new { id, status = "queued" })
            .ToActionResult(HttpContext);
    }
    
    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
        var user = _repository.FindById(id);
        if (user == null)
            return Result<User>.NotFound($"User {id} not found")
                .ToActionResult(HttpContext);
        
        _repository.Delete(user);
        
        // Result.NoContent() automatically sets 204 status code
        return Result<User>.NoContent().ToActionResult(HttpContext);
    }
}
```

## Result Status Codes

Result<T> carries semantic HTTP intent with automatic status code handling:

| Method | Status Code | Description | Use Case |
|--------|-------------|-------------|----------|
| `Ok(data)` | 200 | Success | Standard GET, PUT operations |
| `Created(data)` | 201 | Resource created | POST operations creating resources |
| `Accepted(data)` | 202 | Accepted for processing | Async/queued operations |
| `NoContent()` | 204 | Success with no body | DELETE, HEAD, PUT with no response |
| `Redirect(location)` | 302 | Temporary redirect | Resource temporarily moved |
| `RedirectPermanent(location)` | 301 | Permanent redirect | Resource permanently moved |
| `NotFound(msg)` | 404 | Not found | Resource doesn't exist |
| `BadRequest(msg)` | 400 | Bad request | Invalid input |
| `Unauthorized(msg)` | 401 | Not authenticated | Authentication required |

### NoContent Example

```csharp
[HttpDelete("users/{id}")]
public IActionResult DeleteUser(int id)
{
    var result = _repository.Delete(id);
    return result 
        ? Result<User>.NoContent().ToActionResult(HttpContext)
        : Result<User>.NotFound($"User {id} not found").ToActionResult(HttpContext);
}

// HEAD request
[HttpHead("users/{id}")]
public IActionResult CheckUserExists(int id)
{
    var exists = _repository.Exists(id);
    return exists
        ? Result<User>.NoContent().ToActionResult(HttpContext)
        : Result<User>.NotFound("User not found").ToActionResult(HttpContext);
}
```

### Redirect Examples

```csharp
// Temporary redirect (302)
[HttpGet("docs")]
public IActionResult RedirectToDocs()
{
    return Result<string>.Redirect("https://routya.github.io/")
        .ToActionResult(HttpContext);
}

// Permanent redirect (301)
[HttpGet("old-endpoint")]
public IActionResult OldEndpoint()
{
    var newUrl = $"{Request.Scheme}://{Request.Host}/api/new-endpoint";
    return Result<string>.RedirectPermanent(newUrl)
        .ToActionResult(HttpContext);
}
```

> **Note:** `ToActionResult()` and `ToHttpResult()` automatically use the appropriate status code from Result<T>. No manual status code parameters needed!

---

## Exception Handling

The middleware automatically converts unhandled exceptions to RFC 7807 ProblemDetails:

```csharp
app.MapGet("/users/{id}", (int id) =>
{
    if (id <= 0)
        throw new ArgumentException("ID must be positive");
    
    // Automatically becomes 400 Bad Request with ProblemDetails
    return Results.Ok(GetUser(id));
});
```

### Custom Exceptions

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
            detail: $"Balance ${available} is less than requested ${requested}")
    {
        Extensions["availableBalance"] = available;
        Extensions["requestedAmount"] = requested;
    }
}
```

### Custom Exception Mappers

```csharp
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

// Register
builder.Services.AddExceptionMapper(new UserNotFoundExceptionMapper());
```

## Configuration Options

```csharp
builder.Services.AddResultKitProblemDetails(options =>
{
    // Base URI for domain-specific problem types
    options.ProblemTypeBaseUri = "https://api.example.com/problems/";
    
    // Include exception details (stacktrace, etc.) - should be false in production
    options.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    
    // Automatically add trace ID to all problem responses
    options.IncludeTraceId = true;
    
    // Name of the trace ID extension member
    options.TraceIdExtensionName = "traceId";
    
    // JSON naming policy (default: camelCase)
    options.NamingPolicy = JsonNamingPolicy.CamelCase;
});
```

## Built-in Exception Mappings

The following exceptions are automatically mapped:

| Exception Type | HTTP Status | Problem Type |
|----------------|-------------|--------------|
| `ProblemDetailsException` | Custom | Custom |
| `ArgumentException` | 400 | Bad Request |
| `ArgumentNullException` | 400 | Bad Request |
| `ArgumentOutOfRangeException` | 400 | Bad Request |
| `UnauthorizedAccessException` | 403 | Forbidden |
| `InvalidOperationException` | 409 | Conflict |
| `NotImplementedException` | 501 | Not Implemented |
| Other exceptions | 500 | Internal Server Error |

## Result Status Codes

`Result<T>` carries a `StatusCode` property that determines the HTTP status code:

```csharp
Result<User>.Ok(user)        // StatusCode = 200
Result<User>.Created(user)   // StatusCode = 201
Result<User>.Accepted(data)  // StatusCode = 202
Result<User>.NotFound()      // StatusCode = 404 (from ProblemDetails)
Result<User>.BadRequest()    // StatusCode = 400 (from ProblemDetails)
// etc.
```

When calling `ToHttpResult()` or `ToActionResult()`, the appropriate HTTP response is automatically generated:

| Result Method | Status Code | HTTP Response |
|---------------|-------------|---------------|
| `Ok(data)` | 200 | `OkObjectResult` / `Results.Ok()` |
| `Created(data)` | 201 | `CreatedResult` / `Results.Created()` |
| `Accepted(data)` | 202 | `AcceptedResult` / `Results.Accepted()` |
| `Fail(problem)` | From ProblemDetails | `ObjectResult` with ProblemDetails |

**No need to manually specify status codes** - the semantic intent is carried by the Result itself.

## API Reference

### Extension Methods

#### `ToHttpResult<T>()`
Converts `Result<T>` to `IResult` for Minimal APIs. Automatically uses the status code from `result.StatusCode`.

```csharp
public static IResult ToHttpResult<T>(this Result<T> result, HttpContext? context = null)
```

**Parameters:**
- `result` - The Result to convert
- `context` - Optional HttpContext for setting the `instance` property from the request path

**Returns:** Appropriate IResult based on `result.StatusCode` (200→Ok, 201→Created, 202→Accepted, etc.)

#### `ToActionResult<T>()`
Converts `Result<T>` to `IActionResult` for MVC controllers. Automatically uses the status code from `result.StatusCode`.

```csharp
public static IActionResult ToActionResult<T>(this Result<T> result, HttpContext? context = null)
```

**Parameters:**
- `result` - The Result to convert
- `context` - Optional HttpContext for setting the `instance` property from the request path

**Returns:** Appropriate IActionResult based on `result.StatusCode` (200→OkObjectResult, 201→CreatedResult, 202→AcceptedResult, etc.)

#### `ToProblemResult()`
Converts `ProblemDetails` to `IResult`.

```csharp
public static IResult ToProblemResult(this ProblemDetails problemDetails)
```

#### `ToProblemActionResult()`
Converts `ProblemDetails` to `IActionResult`.

```csharp
public static IActionResult ToProblemActionResult(this ProblemDetails problemDetails)
```

### Converters

#### `ProblemDetailsConverter.ToMicrosoft()`
Converts `Routya.ResultKit.ProblemDetails` to `Microsoft.AspNetCore.Http.ProblemDetails`.

```csharp
public static Microsoft.AspNetCore.Http.ProblemDetails ToMicrosoft(
    Routya.ResultKit.ProblemDetails source)
```

#### `ProblemDetailsConverter.FromMicrosoft()`
Converts `Microsoft.AspNetCore.Http.ProblemDetails` to `Routya.ResultKit.ProblemDetails`.

```csharp
public static Routya.ResultKit.ProblemDetails FromMicrosoft(
    Microsoft.AspNetCore.Http.ProblemDetails source)
```

## Examples

See the [Migration Guide](https://github.com/HBartosch/Routya.ResultKit/blob/main/docs/MIGRATION_V2.md) for complete examples.

## License

MIT License - see [LICENSE](https://github.com/HBartosch/Routya.ResultKit/blob/main/LICENSE)

## Links

- [Main Package (Routya.ResultKit)](https://www.nuget.org/packages/Routya.ResultKit)
- [GitHub Repository](https://github.com/HBartosch/Routya.ResultKit)
- [Migration Guide](https://github.com/HBartosch/Routya.ResultKit/blob/main/docs/MIGRATION_V2.md)
- [RFC 7807 Specification](https://datatracker.ietf.org/doc/html/rfc7807)
