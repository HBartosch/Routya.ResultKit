using Microsoft.AspNetCore.Mvc;
using Routya.ResultKit;
using Routya.ResultKit.Demo.Api.Models;
using Routya.ResultKit.Validation;
using Routya.ResultKit.AspNetCore.Extensions;

namespace Routya.ResultKit.Demo.Api.Controllers;

/// <summary>
/// Demonstrates all validation features including built-in and custom attributes
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ValidationController : ControllerBase
{
    private static readonly List<User> Users = new();
    private static readonly List<Product> Products = new();
    private static readonly List<Order> Orders = new();
    private static int _userIdCounter = 1;
    private static int _productIdCounter = 1;
    private static int _orderIdCounter = 1;

    /// <summary>
    /// Create a user - demonstrates Required, StringLength, EmailAddress, Range, and MatchRegex attributes
    /// </summary>
    /// <remarks>
    /// Example valid request:
    /// {
    ///   "name": "John Doe",
    ///   "email": "john@example.com",
    ///   "age": 25,
    ///   "phoneNumber": "+1234567890"
    /// }
    /// 
    /// Example invalid request (multiple validation errors):
    /// {
    ///   "name": "J",
    ///   "email": "invalid-email",
    ///   "age": 15,
    ///   "phoneNumber": "invalid"
    /// }
    /// </remarks>
    [HttpPost("users")]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult CreateUser([FromBody] User user)
    {
        var validationResult = user.Validate();
        if (!validationResult.Success)
        {
            return validationResult.ToActionResult(HttpContext);
        }

        user.Id = _userIdCounter++;
        Users.Add(user);
        
        return Result<User>.Created(user).ToActionResult(HttpContext);
    }

    /// <summary>
    /// Create a product - demonstrates StringEnum, MinItems, and MaxItems attributes
    /// </summary>
    /// <remarks>
    /// Example valid request:
    /// {
    ///   "name": "Laptop",
    ///   "description": "High-performance laptop",
    ///   "price": 999.99,
    ///   "stock": 50,
    ///   "category": "Electronics",
    ///   "tags": ["laptop", "computer", "electronics"]
    /// }
    /// 
    /// Example invalid request:
    /// {
    ///   "name": "X",
    ///   "description": "Short",
    ///   "price": -10,
    ///   "stock": 15000,
    ///   "category": "InvalidCategory",
    ///   "tags": []
    /// }
    /// </remarks>
    [HttpPost("products")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult CreateProduct([FromBody] Product product)
    {
        var validationResult = product.Validate();
        if (!validationResult.Success)
        {
            return validationResult.ToActionResult(HttpContext);
        }

        product.Id = _productIdCounter++;
        Products.Add(product);
        
        return Result<Product>.Created(product).ToActionResult(HttpContext);
    }

    /// <summary>
    /// Create an order - demonstrates RequiredIf, RequiredIfEmpty, and nested validation
    /// </summary>
    /// <remarks>
    /// Example valid request with email:
    /// {
    ///   "orderDate": "2024-11-19T10:00:00",
    ///   "deliveryDate": "2024-11-25T10:00:00",
    ///   "totalAmount": 150.00,
    ///   "status": "Pending",
    ///   "email": "customer@example.com",
    ///   "items": [
    ///     {
    ///       "productName": "Product A",
    ///       "quantity": 2,
    ///       "unitPrice": 75.00
    ///     }
    ///   ]
    /// }
    /// 
    /// Example valid request with shipped status:
    /// {
    ///   "orderDate": "2024-11-19T10:00:00",
    ///   "totalAmount": 150.00,
    ///   "status": "Shipped",
    ///   "trackingNumber": "TRACK123456",
    ///   "phone": "+1234567890",
    ///   "items": [
    ///     {
    ///       "productName": "Product A",
    ///       "quantity": 2,
    ///       "unitPrice": 75.00
    ///     }
    ///   ]
    /// }
    /// 
    /// Example invalid request (missing required conditional fields):
    /// {
    ///   "orderDate": "2024-11-19T10:00:00",
    ///   "totalAmount": 150.00,
    ///   "status": "Shipped",
    ///   "items": []
    /// }
    /// </remarks>
    [HttpPost("orders")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult CreateOrder([FromBody] Order order)
    {
        var validationResult = order.Validate();
        if (!validationResult.Success)
        {
            return validationResult.ToActionResult(HttpContext);
        }

        order.Id = _orderIdCounter++;
        Orders.Add(order);
        
        return Result<Order>.Created(order).ToActionResult(HttpContext);
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(List<User>), StatusCodes.Status200OK)]
    public IActionResult GetUsers()
    {
        return Result<List<User>>.Ok(Users).ToActionResult(HttpContext);
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet("products")]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    public IActionResult GetProducts()
    {
        return Result<List<Product>>.Ok(Products).ToActionResult(HttpContext);
    }

    /// <summary>
    /// Get all orders
    /// </summary>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(List<Order>), StatusCodes.Status200OK)]
    public IActionResult GetOrders()
    {
        return Result<List<Order>>.Ok(Orders).ToActionResult(HttpContext);
    }

    /// <summary>
    /// Clear all data
    /// </summary>
    [HttpDelete("clear")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult ClearAll()
    {
        Users.Clear();
        Products.Clear();
        Orders.Clear();
        _userIdCounter = 1;
        _productIdCounter = 1;
        _orderIdCounter = 1;
        
        return NoContent();
    }
}
