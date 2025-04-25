using Newtonsoft.Json;
using Routya.ResultKit.Attributes;
using Routya.ResultKit.Test.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Routya.ResultKit.Test.AttributeTests.CombinationTests;
public class CombinedValidationTests
{
    [Fact]
    public void CombinedValidation_InvalidModel_ShouldCatchAllErrors()
    {
        var model = new TestModel
        {
            Name = null,                         
            Email = "invalid",                   
            Age = 10,                            
            Role = "SuperAdmin",                 
            Password = "123",                    
            ConfirmPassword = "456",
            MaxPurchase = 50,                    
            MinPurchase = 100
        };

        var result = model.Validate();

        Assert.False(result.Success);
        var errors = ValidationTestHelper.GetErrors(result);

        Assert.Contains("Name", errors.Keys);
        Assert.Contains("Email", errors.Keys);
        Assert.Contains("Age", errors.Keys);
        Assert.Contains("Role", errors.Keys);
        Assert.Contains("ConfirmPassword", errors.Keys);
        Assert.Contains("MaxPurchase", errors.Keys);
    }

    [Fact]
    public void CombinedValidation_ValidModel_ShouldPass()
    {
        var model = new TestModel
        {
            Name = "Henry",
            Email = "henry@example.com",
            Age = 30,
            Role = "Admin",
            Password = "abc123",
            ConfirmPassword = "abc123",
            MinPurchase = 100,
            MaxPurchase = 200
        };

        var result = model.Validate();

        var test = JsonConvert.SerializeObject(result);

        Assert.True(result.Success);
    }

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
}