using System.ComponentModel.DataAnnotations;
using Routya.ResultKit.Attributes;

namespace Routya.ResultKit.Demo.Api.Models;

/// <summary>
/// User model demonstrating validation attributes
/// </summary>
public class User
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;
    
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int Age { get; set; }
    
    [MatchRegex(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// Product model demonstrating custom validation attributes
/// </summary>
public class Product
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
    
    [Range(0, 10000, ErrorMessage = "Stock must be between 0 and 10000")]
    public int Stock { get; set; }
    
    [StringEnum(typeof(ProductCategory), ErrorMessage = "Invalid category")]
    public string Category { get; set; } = string.Empty;
    
    [MinItems(1, ErrorMessage = "At least one tag is required")]
    [MaxItems(10, ErrorMessage = "Maximum 10 tags allowed")]
    public List<string> Tags { get; set; } = new();
}

public enum ProductCategory
{
    Electronics,
    Clothing,
    Food,
    Books,
    Toys
}

/// <summary>
/// Order model demonstrating date validation and conditional validation
/// </summary>
public class Order
{
    public int Id { get; set; }
    
    [Required]
    public DateTime OrderDate { get; set; }
    
    public DateTime? DeliveryDate { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
    public decimal TotalAmount { get; set; }
    
    [Required]
    public string Status { get; set; } = "Pending";
    
    [RequiredIf(nameof(Status), "Shipped", ErrorMessage = "Tracking number is required when status is Shipped")]
    public string? TrackingNumber { get; set; }
    
    [RequiredIfEmpty(nameof(Email), ErrorMessage = "Phone is required if email is not provided")]
    public string? Phone { get; set; }
    
    [RequiredIfEmpty(nameof(Phone), ErrorMessage = "Email is required if phone is not provided")]
    public string? Email { get; set; }
    
    [MinItems(1, ErrorMessage = "At least one item is required")]
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    [Required]
    public string ProductName { get; set; } = string.Empty;
    
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
    public decimal UnitPrice { get; set; }
}

/// <summary>
/// Models for transformation examples
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}
