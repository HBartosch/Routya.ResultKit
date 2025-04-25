using Routya.ResultKit.Attributes;
using Routya.ResultKit.Test.Helpers;

namespace Routya.ResultKit.Test.Tests.SingularTests;
public class RequiredIfEmptyAttributeTests
{
    [Fact]
    public void RequiredIfEmpty_Valid_ShouldPass()
    {
        var model = new TestModel { Email = "a@b.com", AltEmail = "" };
        var result = model.Validate();
        Assert.True(result.Success);
    }

    [Fact]
    public void RequiredIfEmpty_Invalid_ShouldFail()
    {
        var model = new TestModel { Email = "", AltEmail = "" };
        var result = model.Validate();

        Assert.False(result.Success);
        var errors = ValidationTestHelper.GetErrors(result);
        Assert.Contains("AltEmail", errors.Keys);
    }

    private class TestModel
    {
        public string? Email { get; set; }

        [RequiredIfEmpty("Email")]
        public string? AltEmail { get; set; }
    }
}