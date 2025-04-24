using Routya.ResultKit.Attributes;
using Routya.ResultKit.Test.Helpers;

namespace Routya.ResultKit.Test.Tests;
public class MaxItemsAttributeTests
{
    [Fact]
    public void MaxItems_Valid_ShouldPass()
    {
        var model = new TestModel { Items = new[] { 1, 2 } };
        var result = model.Validate();
        Assert.True(result.Success);
    }

    [Fact]
    public void MaxItems_Invalid_ShouldFail()
    {
        var model = new TestModel { Items = new[] { 1, 2, 3, 4 } };
        var result = model.Validate();

        Assert.False(result.Success);
        var errors = ValidationTestHelper.GetErrors(result);
        Assert.Contains("Items", errors.Keys);
    }

    private class TestModel
    {
        [MaxItems(3)]
        public int[] Items { get; set; }
    }
}