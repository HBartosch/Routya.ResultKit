using Routya.ResultKit.Test.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Routya.ResultKit.Test.Tests.BuiltInTests;
public class StringLengthAttributeTests
{
    [Fact]
    public void StringLength_Valid_ShouldPass()
    {
        var model = new TestModel { Username = "Henry" };
        var result = model.Validate();
        Assert.True(result.Success);
    }

    [Fact]
    public void StringLength_Invalid_ShouldFail()
    {
        var model = new TestModel { Username = "ThisUsernameIsWayTooLongForTheLimitSet" };
        var result = model.Validate();

        Assert.False(result.Success);
        var errors = ValidationTestHelper.GetErrors(result);
        Assert.Contains("Username", errors.Keys);
    }

    private class TestModel
    {
        [StringLength(20)]
        public string Username { get; set; }
    }
}