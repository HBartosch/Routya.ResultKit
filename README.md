# 📦 Routya.ResultKit

**Lightweight result wrapper and validation toolkit for C# **  
Brings clean `Result<T>` handling and extensible validation with custom attributes.

---

## ✨ Features

✅ Consistent `Result<T>` response pattern  
✅ One-line `.Validate()` extension for request models  
✅ Rich built-in and custom validation attributes  

---

## 📥 Installation

```bash
dotnet add package Routya.ResultKit --version 1.0.0
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
| `[ValidDateTimeRange("End")]` | Validate DateTimeOffset ranges |

---
