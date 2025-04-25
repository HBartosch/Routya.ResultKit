namespace Routya.ResultKit.Test.TransformationTests;
public class TransformResultExtensionsTests
{
    [Fact]
    public void Transform_SuccessfulResult_ShouldTransformData()
    {
        var input = Result<string>.Ok("Hello");

        var result = input.Transform(x => x.Length);

        Assert.True(result.Success);
        Assert.Equal(input.Data.Length, result.Data);
    }

    [Fact]
    public void Transform_FailedResult_ShouldPreserveError()
    {
        var error = new Dictionary<string, string[]> { { "Name", new[] { "Required" } } };
        var input = Result<string>.Fail("Invalid", 400, error);

        var result = input.Transform(x => x.Length);

        Assert.False(result.Success);
        Assert.Equal(input.Error!.Title, result.Error!.Title);
        Assert.Equal(input.Error.Status, result.Error.Status);
        Assert.Contains("Name", ((IDictionary<string, string[]>)result.Error.Extensions["errors"]).Keys);
    }

    [Fact]
    public void Transform_ResultStringToGreeting_ShouldWork()
    {
        var input = Result<string>.Ok("Hello");

        var result = input.Transform(str => new Greeting
        {
            Message = str,
            Length = str.Length
        });

        Assert.True(result.Success);
        Assert.Equal(input.Data, result.Data.Message);
        Assert.Equal(input.Data.Length, result.Data.Length);
    }

    private class Greeting
    {
        public string Message { get; set; }
        public int Length { get; set; }
    }
}