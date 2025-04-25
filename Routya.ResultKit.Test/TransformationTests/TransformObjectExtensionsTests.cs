namespace Routya.ResultKit.Test.TransformationTests;
public class TransformObjectExtensionsTests
{
    [Fact]
    public void Transform_ShouldConvertFromInputToOutput()
    {
        var input = new Source { Name = "John", Age = 35 };

        var result = input.Transform(x => new Target
        {
            FullName = x.Name,
            Years = x.Age
        });

        Assert.Equal(input.Name, result.FullName);
        Assert.Equal(input.Age, result.Years);
    }

    [Fact]
    public void Transform_StringToGreeting_ShouldWork()
    {
        var input = "Hello";

        var greeting = input.Transform(str => new Greeting
        {
            Message = str,
            Length = str.Length
        });

        Assert.Equal(input, greeting.Message);
        Assert.Equal(input.Length, greeting.Length);
    }

    private class Source
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    private class Target
    {
        public string FullName { get; set; }
        public int Years { get; set; }
    }

    private class Greeting
    {
        public string Message { get; set; }
        public int Length { get; set; }
    }
}