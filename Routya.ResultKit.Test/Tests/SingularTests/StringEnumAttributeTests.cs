using Routya.ResultKit.Attributes;
using Routya.ResultKit.Test.Helpers;

namespace Routya.ResultKit.Test.Tests.SingularTests;
public class StringEnumAttributeTests
{
    public enum StatusType { Active, Inactive }

    [Fact]
    public void StringEnum_Valid_ShouldPass()
    {
        var model = new TestModel { Status = nameof(StatusType.Active) };
        var result = model.Validate();
        Assert.True(result.Success);
    }

    [Fact]
    public void StringEnum_Invalid_ShouldFail()
    {
        var model = new TestModel { Status = "Sleeping" };
        var result = model.Validate();

        Assert.False(result.Success);
        var errors = ValidationTestHelper.GetErrors(result);
        Assert.Contains("Status", errors.Keys);
    }

    private class TestModel
    {
        [StringEnum(typeof(StatusType))]
        public string Status { get; set; }
    }
}