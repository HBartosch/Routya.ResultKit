using System.ComponentModel.DataAnnotations;

namespace Routya.ResultKit.Test.ValidationAndTransformationTests;
public class ValidateAndTransformTests
{
    [Fact]
    public void ValidateAndTransform_FullFlow_ShouldSucceed()
    {
        var request = new CreateUserRequest
        {
            Name = "Henry",
            Email = "henry@example.com",
            Role = "Admin"
        };
        
        var result = request.Validate()
            .Transform(req => new CreateUserCommand
            {
                Name = req.Name,
                Email = req.Email,
                Role = Enum.Parse<UserRole>(req.Role, ignoreCase: true)
            });
        
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(request.Name, result.Data!.Name);
        Assert.Equal(UserRole.Admin, result.Data.Role);
    }

    [Fact]
    public void ValidateAndTransform_FullFlow_ShouldFailValidation()
    {
        var request = new CreateUserRequest
        {
            Name = "",
            Email = "not-an-email",
            Role = "InvalidRole"
        };

        var result = request.Validate().Transform(req => new CreateUserCommand
        {
            Name = req.Name,
            Email = req.Email,
            Role = Enum.Parse<UserRole>(req.Role, ignoreCase: true)
        });

        Assert.False(result.Success);
        var errors = (IDictionary<string, string[]>)result.Error!.Extensions["errors"];
        Assert.Contains("Name", errors.Keys);
        Assert.Contains("Email", errors.Keys);
    }

    private class CreateUserRequest
    {
        [Required]
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; }
    }

    private class CreateUserCommand
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
    }

    private enum UserRole
    {
        Admin,
        User,
        Guest
    }
}