using Microsoft.AspNetCore.Mvc;
using Routya.ResultKit;
using Routya.ResultKit.Demo.Api.Models;
using Routya.ResultKit.AspNetCore.Extensions;

namespace Routya.ResultKit.Demo.Api.Controllers;

/// <summary>
/// Demonstrates transformation features for converting between types
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TransformationController : ControllerBase
{
    private static readonly List<User> Users = new()
    {
        new User { Id = 1, Name = "Alice Johnson", Email = "alice@example.com", Age = 28, PhoneNumber = "+1234567890" },
        new User { Id = 2, Name = "Bob Smith", Email = "bob@example.com", Age = 35, PhoneNumber = "+9876543210" },
        new User { Id = 3, Name = "Charlie Brown", Email = "charlie@example.com", Age = 42, PhoneNumber = "+5555555555" }
    };

    private static readonly List<Product> Products = new()
    {
        new Product 
        { 
            Id = 1, 
            Name = "Laptop", 
            Description = "High-performance laptop", 
            Price = 999.99m, 
            Stock = 50, 
            Category = "Electronics",
            Tags = new List<string> { "laptop", "computer", "electronics" }
        },
        new Product 
        { 
            Id = 2, 
            Name = "Book", 
            Description = "Programming guide", 
            Price = 49.99m, 
            Stock = 200, 
            Category = "Books",
            Tags = new List<string> { "book", "programming", "education" }
        }
    };

    /// <summary>
    /// Transform a User object to UserDto - demonstrates TransformObject
    /// </summary>
    /// <remarks>
    /// This endpoint shows how to transform a domain object to a DTO using TransformObject.
    /// The transformation handles mapping between compatible properties automatically.
    /// </remarks>
    [HttpGet("users/{id}/dto")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult GetUserAsDto(int id)
    {
        var user = Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            var problem = Routya.ResultKit.Builders.ProblemDetailsBuilder.NotFound("User not found")
                .WithExtension("userId", id)
                .Build();
            return Result<UserDto>.Fail(problem).ToActionResult(HttpContext);
        }

        // Transform User to UserDto
        var dtoResult = Result<User>.Ok(user)
            .Transform(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Age = u.Age
            });

        return dtoResult.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Transform multiple Users to DTOs - demonstrates TransformObject with collections
    /// </summary>
    [HttpGet("users/all-dto")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    public IActionResult GetAllUsersAsDtos()
    {
        var usersResult = Result<List<User>>.Ok(Users);
        
        // Transform List<User> to List<UserDto>
        var dtosResult = usersResult.Transform(users =>
            users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Age = u.Age
            }).ToList());

        return dtosResult.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Transform a Product using TransformResult - demonstrates Result transformation with error handling
    /// </summary>
    /// <remarks>
    /// This endpoint shows how TransformResult can handle transformations that might fail.
    /// If the transformation logic throws an exception or returns a failure, it's captured in the Result.
    /// </remarks>
    [HttpGet("products/{id}/dto")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult GetProductAsDto(int id)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            var problem = Routya.ResultKit.Builders.ProblemDetailsBuilder.NotFound("Product not found")
                .WithExtension("productId", id)
                .Build();
            return Result<ProductDto>.Fail(problem).ToActionResult(HttpContext);
        }

        // Transform allows for transformations that might include validation
        var dto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Category = product.Category
        };

        return Result<ProductDto>.Ok(dto).ToActionResult(HttpContext);
    }

    /// <summary>
    /// Transform with conditional logic - demonstrates complex transformation scenarios
    /// </summary>
    /// <remarks>
    /// Example showing transformation with business rules:
    /// - Products under $100 get a "Budget" prefix
    /// - Products over $500 get a "Premium" prefix
    /// </remarks>
    [HttpGet("products/categorized")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public IActionResult GetCategorizedProducts()
    {
        var productsResult = Result<List<Product>>.Ok(Products);
        
        var categorizedResult = productsResult.Transform(products =>
            products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Price < 100 ? $"Budget - {p.Name}" : 
                       p.Price > 500 ? $"Premium - {p.Name}" : 
                       p.Name,
                Price = p.Price,
                Category = p.Category
            }).ToList());

        return categorizedResult.ToActionResult(HttpContext);
    }

    /// <summary>
    /// Chain multiple transformations - demonstrates composition
    /// </summary>
    [HttpGet("users/{id}/summary")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult GetUserSummary(int id)
    {
        var user = Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            var problem = Routya.ResultKit.Builders.ProblemDetailsBuilder.NotFound("User not found")
                .WithExtension("userId", id)
                .Build();
            return Result<string>.Fail(problem).ToActionResult(HttpContext);
        }

        // Chain transformations: User -> UserDto -> Summary string
        var summaryResult = Result<User>.Ok(user)
            .Transform(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Age = u.Age
            })
            .Transform(dto => 
                $"User #{dto.Id}: {dto.Name} ({dto.Age} years old) - {dto.Email}");

        return summaryResult.ToActionResult(HttpContext);
    }
}
