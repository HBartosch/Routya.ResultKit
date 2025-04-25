using Routya.ResultKit.Test.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Routya.ResultKit.Test.Tests.BuiltInTests;
public class RequiredAttributeTests
{
    [Fact]
    public void Required_Valid_ShouldPass()
    {
        var model = new TestModel { Name = "Valid" };
        var result = model.Validate();
        Assert.True(result.Success);
    }

    [Fact]
    public void Required_Invalid_ShouldFail()
    {
        var model = new TestModel { Name = null };
        var result = model.Validate();

        Assert.False(result.Success);
        var errors = ValidationTestHelper.GetErrors(result);
        Assert.Contains("Name", errors.Keys);
    }

    private class TestModel
    {
        [Required]
        public string? Name { get; set; }
    }
}