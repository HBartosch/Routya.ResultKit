using Routya.ResultKit.Test.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Routya.ResultKit.Test.Tests.BuiltInTests;
public class EmailAddressAttributeTests
{
    [Fact]
    public void EmailAddress_Valid_ShouldPass()
    {
        var model = new TestModel { Email = "test@example.com" };
        var result = model.Validate();
        Assert.True(result.Success);
    }

    [Fact]
    public void EmailAddress_Invalid_ShouldFail()
    {
        var model = new TestModel { Email = "invalid-email" };
        var result = model.Validate();

        Assert.False(result.Success);
        var errors = ValidationTestHelper.GetErrors(result);
        Assert.Contains("Email", errors.Keys);
    }

    private class TestModel
    {
        [EmailAddress]
        public string Email { get; set; }
    }
}