using Routya.ResultKit.Attributes;
using Routya.ResultKit.Test.Helpers;

namespace Routya.ResultKit.Test.Tests;
public class MatchRegexAttributeTests
{
    [Fact]
    public void MatchRegex_Valid_ShouldPass()
    {
        var model = new TestModel { Username = "Alpha123" };
        var result = model.Validate();
        Assert.True(result.Success);
    }

    [Fact]
    public void MatchRegex_Invalid_ShouldFail()
    {
        var model = new TestModel { Username = "Alpha 123" };
        var result = model.Validate();

        Assert.False(result.Success);
        var errors = ValidationTestHelper.GetErrors(result);
        Assert.Contains("Username", errors.Keys);
    }

    private class TestModel
    {
        [MatchRegex(@"^[a-zA-Z0-9]+$")]
        public string Username { get; set; }
    }
}