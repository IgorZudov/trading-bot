using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CryptoTrader.Utils.Results
{
    [PublicAPI]
    public static class AsyncResultExtensionsR
    {
        public static bool DefaultConfigureAwait;

        public static async Task<Result<TOut>> OnSuccess<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> func)
        {
            if (!result.Success)
                return Result.Fail<TOut>(result.Error);

            var value = await func(result.Value).ConfigureAwait(DefaultConfigureAwait);

            return Result.Ok(value);
        }

        public static async Task<Result<TIn>> OnSuccess<TIn>(this Result result, Func<Task<TIn>> func)
        {
            if (!result.Success)
                return Result.Fail<TIn>(result.Error);

            var value = await func().ConfigureAwait(DefaultConfigureAwait);

            return Result.Ok(value);
        }

        public static async Task<Result<TOut>> OnSuccess<TIn, TOut>(this Result<TIn> result,
            Func<TIn, Task<Result<TOut>>> func)
        {
            if (!result.Success)
                return Result.Fail<TOut>(result.Error);

            return await func(result.Value).ConfigureAwait(DefaultConfigureAwait);
        }

        public static async Task<Result<TIn>> OnSuccess<TIn>(this Result result, Func<Task<Result<TIn>>> func)
        {
            if (!result.Success)
                return Result.Fail<TIn>(result.Error);

            return await func().ConfigureAwait(DefaultConfigureAwait);
        }

        public static async Task<Result<TOut>> OnSuccess<TIn, TOut>(this Result<TIn> result,
            Func<Task<Result<TOut>>> func)
        {
            if (!result.Success)
                return Result.Fail<TOut>(result.Error);

            return await func().ConfigureAwait(DefaultConfigureAwait);
        }

        public static async Task<Result> OnSuccess<TIn>(this Result<TIn> result, Func<TIn, Task<Result>> func)
        {
            if (!result.Success)
                return Result.Fail(result.Error);

            return await func(result.Value).ConfigureAwait(DefaultConfigureAwait);
        }

        public static async Task<Result> OnSuccess(this Result result, Func<Task<Result>> func)
        {
            if (!result.Success)
                return result;

            return await func().ConfigureAwait(DefaultConfigureAwait);
        }

        public static async Task<Result<TIn>> Ensure<TIn>(this Result<TIn> result, Func<TIn, Task<bool>> predicate,
            Error error, Action<TIn> actionOnError = null)
        {
            if (!result.Success)
                return result;

            if (!await predicate(result.Value).ConfigureAwait(DefaultConfigureAwait))
            {
                actionOnError?.Invoke(result.Value);
                return Result.Fail<TIn>(error);
            }

            return result;
        }

        public static async Task<Result> Ensure(this Result result, Func<Task<bool>> predicate, Error error,
            Action actionOnError = null)
        {
            if (!result.Success)
                return result;

            if (!await predicate().ConfigureAwait(DefaultConfigureAwait))
            {
                actionOnError?.Invoke();
                return Result.Fail(error);
            }

            return result;
        }

        public static Task<Result<TOut>> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> func)
            => result.OnSuccess(func);

        public static Task<Result<TIn>> Map<TIn>(this Result result, Func<Task<TIn>> func)
            => result.OnSuccess(func);

        public static async Task<Result<TIn>> OnSuccess<TIn>(this Result<TIn> result, Func<TIn, Task> action)
        {
            if (result.Success)
            {
                await action(result.Value).ConfigureAwait(DefaultConfigureAwait);
            }

            return result;
        }

        public static async Task<Result> OnSuccess(this Result result, Func<Task> action)
        {
            if (result.Success)
            {
                await action().ConfigureAwait(DefaultConfigureAwait);
            }

            return result;
        }

        public static async Task<TIn> OnBoth<TIn>(this Result result, Func<Result, Task<TIn>> func)
        {
            return await func(result).ConfigureAwait(DefaultConfigureAwait);
        }

        public static async Task<TOut> OnBoth<TIn, TOut>(this Result<TIn> result, Func<Result<TIn>, Task<TOut>> func)
        {
            return await func(result).ConfigureAwait(DefaultConfigureAwait);
        }

        public static async Task<Result<TIn>> OnFailure<TIn>(this Result<TIn> result, Func<Task> func)
        {
            if (!result.Success)
            {
                await func().ConfigureAwait(DefaultConfigureAwait);
            }

            return result;
        }

        public static async Task<Result> OnFailure(this Result result, Func<Task> func)
        {
            if (!result.Success)
            {
                await func().ConfigureAwait(DefaultConfigureAwait);
            }

            return result;
        }

        public static async Task<Result<TIn>> OnFailure<TIn>(this Result<TIn> result, Func<Error, Task> func)
        {
            if (!result.Success)
            {
                await func(result.Error).ConfigureAwait(DefaultConfigureAwait);
            }

            return result;
        }

        public static async Task<Result<TIn>> OnFailureCompensate<TIn>(this Result<TIn> result,
            Func<Task<Result<TIn>>> func)
        {
            if (!result.Success)
                return await func().ConfigureAwait(DefaultConfigureAwait);

            return result;
        }

        public static async Task<Result> OnFailureCompensate(this Result result, Func<Task<Result>> func)
        {
            if (!result.Success)
                return await func().ConfigureAwait(DefaultConfigureAwait);

            return result;
        }

        public static async Task<Result<TIn>> OnFailureCompensate<TIn>(this Result<TIn> result,
            Func<Error, Task<Result<TIn>>> func)
        {
            if (!result.Success)
                return await func(result.Error).ConfigureAwait(DefaultConfigureAwait);

            return result;
        }
    }
}
