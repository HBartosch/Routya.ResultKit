using Routya.ResultKit.Attributes;
using Routya.ResultKit.Test.Helpers;

namespace Routya.ResultKit.Test.AttributeTests.SingularTests;
public class RequiredIfAttributeTests
{
    [Fact]
    public void RequiredIf_Valid_ShouldPass()
    {
        var model = new TestModel { Status = "Active", Email = "test@example.com" };
        var result = model.Validate();
        Assert.True(result.Success);
    }

    [Fact]
    public void RequiredIf_Invalid_ShouldFail()
    {
        var model = new TestModel { Status = "Active", Email = null };
        var result = model.Validate();

        Assert.False(result.Success);
        var errors = ValidationTestHelper.GetErrors(result);
        Assert.Contains("Email", errors.Keys);
    }

    private class TestModel
    {
        public string Status { get; set; }

        [RequiredIf("Status", "Active")]
        public string? Email { get; set; }
    }
}