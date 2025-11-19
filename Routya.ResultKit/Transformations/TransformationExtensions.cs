using System.Collections.Generic;
using System;

namespace Routya.ResultKit
{
    public static class TransformationExtensions
    {
        /// <summary>
        /// Transforms an input object of type <typeparamref name="TIn"/> into an output object of type <typeparamref name="TOut"/> using the specified selector function.
        /// </summary>
        /// <typeparam name="TIn">The type of the input object.</typeparam>
        /// <typeparam name="TOut">The type of the output object.</typeparam>
        /// <param name="input">The input object to transform.</param>
        /// <param name="selector">A function that defines how to transform the input object into the output object.</param>
        /// <returns>The transformed output object of type <typeparamref name="TOut"/>.</returns>
        public static TOut Transform<TIn, TOut>(this TIn input, Func<TIn, TOut> selector)
        {
            return selector(input);
        }

        /// <summary>
        /// Transforms a successful <see cref="Result{TIn}"/> into a <see cref="Result{TOut}"/> using the specified selector function. 
        /// If the original result is a failure, the error information is preserved including all extension members.
        /// </summary>
        /// <typeparam name="TIn">The type of the input data contained in the result.</typeparam>
        /// <typeparam name="TOut">The type of the output data to transform into.</typeparam>
        /// <param name="result">The result to transform if successful.</param>
        /// <param name="selector">A function that defines how to transform the input data into the output data.</param>
        /// <returns>
        /// A new <see cref="Result{TOut}"/> containing the transformed output if successful; 
        /// otherwise, a failed result with the original error information.
        /// </returns>
        public static Result<TOut> Transform<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> selector)
        {
            if (!result.Success)
            {
                // Preserve the original ProblemDetails with all extension members
                return Result<TOut>.Fail(result.Error!);
            }

            return Result<TOut>.Ok(selector(result.Data!));
        }
    }
}