using Routya.ResultKit.Attributes;
using Routya.ResultKit.Test.Helpers;

namespace Routya.ResultKit.Test.Tests.SingularTests;
public class LessThanAttributeTests
{
    [Fact]
    public void LessThan_Valid_ShouldPass()
    {
        var model = new TestModel { Max = 100, Min = 50 };
        var result = model.Validate();
        Assert.True(result.Success);
    }

    [Fact]
    public void LessThan_Invalid_ShouldFail()
    {
        var model = new TestModel { Max = 50, Min = 100 };
        var result = model.Validate();

        Assert.False(result.Success);
        var errors = ValidationTestHelper.GetErrors(result);
        Assert.Contains("Min", errors.Keys);
    }

    private class TestModel
    {
        public int Max { get; set; }

        [LessThan("Max")]
        public int Min { get; set; }
    }
}