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
dotnet add package Routya.ResultKit --version 1.0.2
```

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
app.MapPost("/users", (CreateUserRequest request) =>
{
    var validationResult = request.Validate();

    if (!validationResult.Success)
        return Results.BadRequest(validationResult);

    return Results.Ok(Result.Ok(new { Id = 1 }));
});
```

---

### ✅ Successful Response Example

```json
{
	"Success": true,
	"Data": {
		"Name": "Henry",
		"Email": "henry@example.com",
		"Age": 30,
		"Role": "Admin",
		"Password": "abc123",
		"ConfirmPassword": "abc123",
		"MinPurchase": 100.0,
		"MaxPurchase": 200.0
	},
	"Error": null
}
```

---

### ❌ Validation Error Response Example

```json
{
	"Success": false,
	"Data": null,
	"Error": {
		"Title": "Validation Failed",
		"Status": 400,
		"Extensions": {
			"errors": {
				"Name": [
					"The Name field is required."
				],
				"Email": [
					"The Email field is not a valid e-mail address."
				],
				"Age": [
					"The field Age must be between 18 and 120."
				],
				"Role": [
					"Role must be one of: Admin, User, Guest"
				],
				"ConfirmPassword": [
					"'ConfirmPassword' and 'Password' do not match."
				],
				"MaxPurchase": [
					"MaxPurchase must be greater than MinPurchase"
				]
			}
		}
	}
}
```

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


