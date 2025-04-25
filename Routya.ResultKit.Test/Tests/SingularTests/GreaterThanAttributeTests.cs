using Routya.ResultKit.Attributes;
using Routya.ResultKit.Test.Helpers;

namespace Routya.ResultKit.Test.Tests.SingularTests;

public class GreaterThanAttributeTests
{
    [Fact]
    public void GreaterThan_Valid_ShouldPass()
    {
        var model = new TestModel { Min = 1, Max = 5 };
        var result = model.Validate();
        Assert.True(result.Success);
    }

    [Fact]
    public void GreaterThan_Invalid_ShouldFail()
    {
        var model = new TestModel { Min = 10, Max = 5 };
        var result = model.Validate();
        Assert.False(result.Success);

        var errors = ValidationTestHelper.GetErrors(result);
        Assert.Contains("Max", errors.Keys);
    }

    private class TestModel
    {
        public int Min { get; set; }

        [GreaterThan("Min")]
        public int Max { get; set; }
    }
}