using System.Collections.Generic;

namespace Routya.ResultKit
{
    public class Result<T>
    {
        public bool Success { get; }
        public T Data { get; }
        public ProblemDetails? Error { get; }

        private Result(bool success, T data, ProblemDetails? error)
        {
            Success = success;
            Data = data;
            Error = error;
        }

        public static Result<T> Ok(T data) => new Result<T>(true, data, null);

        public static Result<T> Fail(string title, int statusCode, IDictionary<string, string[]>? errors = null)
        {
            var problem = new ProblemDetails
            {
                Title = title,
                Status = statusCode,
                Extensions = { [Constants.ErrorDictionaryKey] = errors ?? new Dictionary<string, string[]>() }
            };
            return new Result<T>(false, default!, problem);
        }

        public static Result<T> Fail(ProblemDetails problem) => new Result<T>(false, default!, problem);
    }
}