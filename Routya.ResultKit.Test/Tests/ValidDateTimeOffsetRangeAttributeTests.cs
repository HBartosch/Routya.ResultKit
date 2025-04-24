using Routya.ResultKit.Attributes;
using Routya.ResultKit.Test.Helpers;

namespace Routya.ResultKit.Test.Tests;
public class ValidDateTimeOffsetRangeAttributeTests
{
    [Fact]
    public void ValidDateTimeOffsetRange_Valid_ShouldPass()
    {
        var model = new TestModel
        {
            Start = DateTimeOffset.UtcNow,
            End = DateTimeOffset.UtcNow.AddHours(1)
        };

        var result = model.Validate();
        Assert.True(result.Success);
    }

    [Fact]
    public void ValidDateTimeOffsetRange_Invalid_ShouldFail()
    {
        var model = new TestModel
        {
            Start = DateTimeOffset.UtcNow.AddDays(1),
            End = DateTimeOffset.UtcNow
        };

        var result = model.Validate();
        Assert.False(result.Success);

        var errors = ValidationTestHelper.GetErrors(result);
        Assert.Contains("Start", errors.Keys);
        Assert.Contains("earlier than", errors["Start"][0]);
    }

    private class TestModel
    {
        [ValidDateTimeOffsetRange("End")]
        public DateTimeOffset Start { get; set; }

        public DateTimeOffset End { get; set; }
    }
}