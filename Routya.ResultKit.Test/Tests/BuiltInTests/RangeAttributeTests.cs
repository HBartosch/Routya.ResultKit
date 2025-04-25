using Routya.ResultKit.Test.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Routya.ResultKit.Test.Tests.BuiltInTests;
public class RangeAttributeTests
{
    [Fact]
    public void Range_Valid_ShouldPass()
    {
        var model = new TestModel { Age = 30 };
        var result = model.Validate();
        Assert.True(result.Success);
    }

    [Fact]
    public void Range_Invalid_ShouldFail()
    {
        var model = new TestModel { Age = 10 };
        var result = model.Validate();

        Assert.False(result.Success);
        var errors = ValidationTestHelper.GetErrors(result);
        Assert.Contains("Age", errors.Keys);
    }

    private class TestModel
    {
        [Range(18, 99)]
        public int Age { get; set; }
    }
}