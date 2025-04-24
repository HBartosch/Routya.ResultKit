namespace Routya.ResultKit.Test.Helpers;
public static class ValidationTestHelper
{
    public static IDictionary<string, string[]> GetErrors<T>(Result<T> result)
    {
        if (result?.Error?.Extensions == null)
            return new Dictionary<string, string[]>();

        if (result.Error.Extensions.TryGetValue("errors", out var value) &&
            value is IDictionary<string, string[]> typed)
        {
            return typed;
        }

        return new Dictionary<string, string[]>();
    }
}