using Routya.ResultKit.Attributes;
using Routya.ResultKit.Test.Helpers;

namespace Routya.ResultKit.Test.AttributeTests.SingularTests;
public class MinItemsAttributeTests
{
    [Fact]
    public void MinItems_Valid_ShouldPass()
    {
        var model = new TestModel { Items = new[] { 1 } };
        var result = model.Validate();
        Assert.True(result.Success);
    }

    [Fact]
    public void MinItems_Invalid_ShouldFail()
    {
        var model = new TestModel { Items = new int[0] };
        var result = model.Validate();

        Assert.False(result.Success);
        var errors = ValidationTestHelper.GetErrors(result);
        Assert.Contains("Items", errors.Keys);
    }

    private class TestModel
    {
        [MinItems(1)]
        public int[] Items { get; set; }
    }
}