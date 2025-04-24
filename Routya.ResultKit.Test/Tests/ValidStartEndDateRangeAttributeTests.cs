using Routya.ResultKit.Attributes;
using Routya.ResultKit.Test.Helpers;

namespace Routya.ResultKit.Test.Tests;


public class ValidStartEndDateRangeAttributeTests
{
    [Fact]
    public void ValidDateRange_Valid_ShouldPass()
    {
        var model = new TestModel { Start = DateTime.UtcNow, End = DateTime.UtcNow.AddHours(1) };
        var result = model.Validate();
        Assert.True(result.Success);
    }

    [Fact]
    public void ValidDateRange_Invalid_ShouldFail()
    {
        var model = new TestModel { Start = DateTime.UtcNow.AddHours(2), End = DateTime.UtcNow };
        var result = model.Validate();

        Assert.False(result.Success);
        var errors = ValidationTestHelper.GetErrors(result);
        Assert.Contains("General", errors.Keys);
    }

    [ValidStartEndDateRange("Start", "End")]
    public class TestModel
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}