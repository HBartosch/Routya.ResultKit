using Routya.ResultKit.Test.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Routya.ResultKit.Test.AttributeTests.BuiltInTests;
public class CompareAttributeTests
{
    [Fact]
    public void Compare_Valid_ShouldPass()
    {
        var model = new TestModel { Password = "123abc", ConfirmPassword = "123abc" };
        var result = model.Validate();
        Assert.True(result.Success);
    }

    [Fact]
    public void Compare_Invalid_ShouldFail()
    {
        var model = new TestModel { Password = "123abc", ConfirmPassword = "xyz123" };
        var result = model.Validate();

        Assert.False(result.Success);
        var errors = ValidationTestHelper.GetErrors(result);
        Assert.Contains("ConfirmPassword", errors.Keys);
    }

    private class TestModel
    {
        [Required]
        public string Password { get; set; }

        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}