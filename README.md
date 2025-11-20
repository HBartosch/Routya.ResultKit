![CI](https://img.shields.io/github/actions/workflow/status/hbartosch/routya.resultkit/dotnet.yml?label=CI&style=flat-square)
![CI](https://img.shields.io/github/actions/workflow/status/hbartosch/routya.resultkit/build-and-test.yml?label=Tests&style=flat-square)
[![NuGet](https://img.shields.io/nuget/v/Routya.ResultKit)](https://www.nuget.org/packages/Routya.ResultKit)
[![NuGet](https://img.shields.io/nuget/dt/Routya.ResultKit)](https://www.nuget.org/packages/Routya.ResultKit)
![.NET Standard](https://img.shields.io/badge/netstandard-2.0%20%7C%202.1-blue?logo=dotnet&logoColor=white)
![Supports Nested Validation](https://img.shields.io/badge/nested--validation-supported-brightgreen)

# 📦 Routya.ResultKit

**Lightweight result wrapper, validation and transformation toolkit for C# **  
Brings clean `Result<T>` handling and extensible validation with custom attributes.

---

## ✨ Features

- ✅ Consistent `Result<T>` response pattern  
- ✅ One-line `.Validate()` extension for request models  
- ✅ `.Transform()` extension for clean and safe object/result projection  
- ✅ Rich built-in and custom validation attributes  
- ✅ Works great with System.ComponentModel.Annotations
- ✅ Validation for nested objects


---

## 📥 Installation

```bash
dotnet add package Routya.ResultKit --version 2.1.0
```

> **Upgrading from v1.x?** See the [Migration Guide](#-migrating-from-v1x-to-v20) below.

---

## 🚀 Quick Start

### 1. Define Your Request Model (with Custom Validation)

```csharp
using Routya.ResultKit.Attributes;
using System.ComponentModel.DataAnnotations;

  private enum UserRole { Admin, User, Guest }

    private class TestModel
    {
        [Required]
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Range(18, 120)]
        public int Age { get; set; }

        [StringEnum(typeof(UserRole))]
        public string Role { get; set; }

        [Required]
        public string Password { get; set; }

        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        public decimal MinPurchase { get; set; }

        [GreaterThan("MinPurchase")]
        public decimal MaxPurchase { get; set; }
    }
```

---

### 2. Validate and Return

```csharp
using Routya.ResultKit;
using Routya.ResultKit.Validation;

app.MapPost("/users", (CreateUserRequest request) =>
{
    var validationResult = request.Validate();

    if (!validationResult.Success)
        return Results.BadRequest(validationResult.Error); // Returns ProblemDetails

    var user = new User { Id = 1, Name = request.Name, Email = request.Email };
    return Results.Ok(user);
});
```

---

### ✅ Successful Response Example (200 OK)

```json
{
	"id": 1,
	"name": "Henry",
	"email": "henry@example.com"
}
```

---

### ❌ Validation Error Response Example (422 Unprocessable Entity)

```json
{
	"type": "urn:problem-type:validation-error",
	"title": "Validation Failed",
	"status": 422,
	"detail": "One or more validation errors occurred.",
	"errors": {
		"name": ["The Name field is required."],
		"email": ["The Email field is not a valid e-mail address."],
		"age": ["The field Age must be between 18 and 120."],
		"role": ["Role must be one of: Admin, User, Guest"],
		"confirmPassword": ["'ConfirmPassword' and 'Password' do not match."],
		"maxPurchase": ["MaxPurchase must be greater than MinPurchase"]
	}
}
```

> **Note:** v2.0 uses RFC 7807 compliant ProblemDetails for all errors.

---

## 🛠️ Built-in Validation Attributes

Routya.ResultKit includes powerful validation attributes ready to use:

| Attribute | Purpose |
|-----------|---------|
| `[GreaterThan("OtherProp")]` | Ensure a property is greater than another |
| `[LessThan("OtherProp")]` | Ensure a property is less than another |
| `[RequiredIf("OtherProp", "Value")]` | Conditionally require a property |
| `[RequiredIfEmpty("OtherProp")]` | Require a property if another is empty |
| `[StringEnum(typeof(EnumType))]` | Ensure a string matches an Enum name |
| `[MatchRegex("pattern")]` | Validate a string against a regex |
| `[MinItems(count)]` | Validate minimum items in a collection |
| `[MaxItems(count)]` | Validate maximum items in a collection |
| `[ValidStartEndDateRange("Start", "End")]` | Validate that StartDate is before EndDate (This is a class level attribute) |
| `[ValidDateTimeOffsetRange("End")]` | Validate DateTimeOffset ranges |
| `[ValidDateTimeRange("End")]` | Validate DateTime ranges |

---

---

## 🔁 Transforming Models

Use `.Transform(...)` to reshape validated models or result data into domain entities or response objects — cleanly and safely.

---

### ✅ Example 1: Basic Object Transformation

```csharp
var request = "Hello";

var greeting = request.Transform(str => new Greeting
{
    Message = str,
    Length = str.Length
});
```

```csharp
public class Greeting
{
    public string Message { get; set; }
    public int Length { get; set; }
}
```

---

### ✅ Example 2: Full Validate → Transform → Result Flow

```csharp
var result = request.Validate()
    .Transform(req => new CreateUserCommand
    {
        Name = req.Name,
        Email = req.Email,
        Role = Enum.Parse<UserRole>(req.Role, ignoreCase: true)
    });
```

---

### ✅ Example 3: Transforming Result<T> Output

```csharp
var result = Result.Ok(user)
    .Transform(u => new UserResponse
    {
        Id = u.Id,
        Name = u.Name
    });
```

---

### 🧠 Why Use `Transform(...)`?

| Benefit             | Description |
|---------------------|-------------|
| ✅ Fluent            | Clean chaining after `.Validate()` |
| ✅ Safe              | When using Result<T> it only transforms data if result is successful |
| ✅ Expressive        | Encourages intentional mapping logic |
| ✅ Lightweight       | Zero dependencies, pure functional mapping |

---

### 🔍 Bonus: Works with Both Objects and Result<T>

```csharp
TOut Transform<TIn, TOut>(this TIn input, Func<TIn, TOut> selector)

Result<TOut> Transform<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> selector)
```

---

## 🌐 HTTP Status Code Support

Result<T> carries semantic HTTP intent with automatic status code handling:

```csharp
// Success status codes
Result<User>.Ok(user);              // 200 OK
Result<User>.Created(user);         // 201 Created
Result<User>.Accepted(user);        // 202 Accepted
Result<User>.NoContent();           // 204 No Content

// Redirect status codes
Result<string>.Redirect(location);           // 302 Found (temporary)
Result<string>.RedirectPermanent(location);  // 301 Moved Permanently

// Error status codes (automatic from factory methods)
Result<User>.NotFound("User not found");     // 404 Not Found
Result<User>.BadRequest("Invalid data");     // 400 Bad Request
Result<User>.Unauthorized("Not authenticated"); // 401 Unauthorized
```

### NoContent Example - DELETE Operations

```csharp
[HttpDelete("users/{id}")]
public IActionResult DeleteUser(int id)
{
    var user = _repository.FindById(id);
    if (user == null)
        return Result<User>.NotFound($"User {id} not found").ToActionResult(HttpContext);
    
    _repository.Delete(user);
    return Result<User>.NoContent().ToActionResult(HttpContext); // Returns 204
}
```

### Redirect Examples

```csharp
// Temporary redirect (302) - for moved resources
[HttpGet("docs")]
public IActionResult RedirectToDocs()
{
    return Result<string>.Redirect("https://routya.github.io/").ToActionResult(HttpContext);
}

// Permanent redirect (301) - for permanently moved endpoints
[HttpGet("old-users")]
public IActionResult OldEndpoint()
{
    var newLocation = $"{Request.Scheme}://{Request.Host}/api/users";
    return Result<string>.RedirectPermanent(newLocation).ToActionResult(HttpContext);
}

// HEAD request with NoContent
[HttpHead("users/check-email")]
public IActionResult CheckEmailExists([FromQuery] string email)
{
    var exists = _repository.EmailExists(email);
    return exists 
        ? Result<User>.NoContent().ToActionResult(HttpContext)
        : Result<User>.NotFound("Email not found").ToActionResult(HttpContext);
}
```

> **Note:** When using the `Routya.ResultKit.AspNetCore` package, `ToActionResult()` and `ToHttpResult()` automatically use the appropriate status code from the Result. See [ASP.NET Core Integration](https://www.nuget.org/packages/Routya.ResultKit.AspNetCore) for details.

---

## 🔄 Migrating from v1.x to v2.0

v2.0 introduces RFC 7807 ProblemDetails and a cleaner API. Here's what changed:

### Key Changes

1. **ProblemDetails replaces old error format**
   ```csharp
   // ❌ v1.x - Simple error dictionary
   Result.Fail("Validation Failed", 400, errors)
   
   // ✅ v2.0 - RFC 7807 ProblemDetails
   Result.Fail(ProblemDetailsBuilder.ValidationError("Validation Failed")
       .WithErrors(errors)
       .Build())
   ```

2. **Result factory methods renamed**
   ```csharp
   // ❌ v1.x
   Result.Success(data)
   Result.Failure("Error", 400)
   
   // ✅ v2.0
   Result.Ok(data)
   Result.Created(data)  // New: Sets 201 status
   Result.Accepted(data) // New: Sets 202 status
   Result.NoContent()    // New in v2.1: Sets 204 status
   Result.Redirect(location)        // New in v2.1: Sets 302 status
   Result.RedirectPermanent(location) // New in v2.1: Sets 301 status
   Result.Fail(problemDetails)
   ```

3. **Validation returns ProblemDetails**
   ```csharp
   // ❌ v1.x
   var result = request.ValidateObject();
   if (!result.Success)
       return result; // Returned Result<T>
   
   // ✅ v2.0
   var result = request.Validate();
   if (!result.Success)
       return result.Error; // Returns ProblemDetails
   ```

4. **ASP.NET Core Integration (New Package)**
   ```bash
   dotnet add package Routya.ResultKit.AspNetCore
   ```
   
   ```csharp
   // Automatic conversion to IResult/IActionResult
   return result.ToActionResult(HttpContext);
   
   // Automatic exception handling
   builder.Services.AddResultKitProblemDetails();
   app.UseResultKitExceptionHandler();
   ```

### Quick Migration Steps

1. Update package: `dotnet add package Routya.ResultKit --version 2.1.0`
2. Replace `Result.Success` → `Result.Ok`
3. Replace `Result.Failure` → `Result.Fail` (use ProblemDetailsBuilder)
4. Replace `ValidateObject()` → `Validate()`
5. For ASP.NET Core, add `Routya.ResultKit.AspNetCore` package

**[📖 Full Migration Guide](https://github.com/HBartosch/Routya.ResultKit/blob/main/docs/MIGRATION_V2.md)** - Complete migration documentation with examples

---

## 📚 Documentation & Resources

- **[Official Website](https://routya.github.io/)** - Routya project homepage
- **[Migration Guide to v2.0](https://github.com/HBartosch/Routya.ResultKit/blob/main/docs/MIGRATION_V2.md)** - Complete guide for upgrading from v1.x
- **[ASP.NET Core Integration](https://www.nuget.org/packages/Routya.ResultKit.AspNetCore)** - ProblemDetails, middleware, IResult/IActionResult extensions
- **[Demo API](https://github.com/HBartosch/Routya.ResultKit/tree/main/Routya.ResultKit.Demo.Api)** - Comprehensive examples of all features

---


