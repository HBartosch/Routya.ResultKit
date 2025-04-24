using Routya.ResultKit.Attributes;
using Routya.ResultKit.Test.Helpers;

namespace Routya.ResultKit.Test.Tests;
public class ValidDateTimeRangeAttributeTests
{
    [Fact]
    public void ValidDateTimeRange_Valid_ShouldPass()
    {
        var model = new TestModel
        {
            Start = DateTime.UtcNow,
            End = DateTime.UtcNow.AddHours(1)
        };

        var result = model.Validate();
        Assert.True(result.Success);
    }

    [Fact]
    public void ValidDateTimeRange_Invalid_ShouldFail()
    {
        var model = new TestModel
        {
            Start = DateTime.UtcNow.AddDays(1),
            End = DateTime.UtcNow
        };

        var result = model.Validate();
        Assert.False(result.Success);

        var errors = ValidationTestHelper.GetErrors(result);
        Assert.Contains("Start", errors.Keys);
        Assert.Contains("earlier than", errors["Start"][0]);
    }

    private class TestModel
    {
        [ValidDateTimeRange("End")]
        public DateTime Start { get; set; }

        public DateTime End { get; set; }
    }
}