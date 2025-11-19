namespace Routya.ResultKit.Test.Helpers;
public static class ValidationTestHelper
{
    public static IDictionary<string, string[]> GetErrors<T>(Result<T> result)
    {
        if (result?.Error == null)
            return new Dictionary<string, string[]>();

        if (result.Error.TryGetExtension<IDictionary<string, string[]>>("errors", out var errors))
        {
            return errors;
        }

        return new Dictionary<string, string[]>();
    }
}