using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CryptoTrader.Utils.Results
{
    [PublicAPI]
    public static class AsyncResultExtensions
    {
        public static bool DefaultConfigureAwait;

        public static async Task<Result<TOut>> OnSuccess<TIn, TOut>(this Task<Result<TIn>> resultTask,
            Func<TIn, Task<TOut>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success)
                return Result.Fail<TOut>(result.Error);

            var value = await func(result.Value).ConfigureAwait(DefaultConfigureAwait);

            return Result.Ok(value);
        }

        public static async Task<Result<TIn>> OnSuccess<TIn>(this Task<Result> resultTask, Func<Task<TIn>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success)
                return Result.Fail<TIn>(result.Error);

            var value = await func().ConfigureAwait(DefaultConfigureAwait);

            return Result.Ok(value);
        }

        public static async Task<Result<TOut>> OnSuccess<TIn, TOut>(this Task<Result<TIn>> resultTask,
            Func<TIn, Task<Result<TOut>>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success)
                return Result.Fail<TOut>(result.Error);

            return await func(result.Value).ConfigureAwait(DefaultConfigureAwait);
        }

        public static async Task<Result<TIn>> OnSuccess<TIn>(this Task<Result> resultTask, Func<Task<Result<TIn>>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success)
                return Result.Fail<TIn>(result.Error);

            return await func().ConfigureAwait(DefaultConfigureAwait);
        }

        public static async Task<Result<TOut>> OnSuccess<TIn, TOut>(this Task<Result<TIn>> resultTask,
            Func<Task<Result<TOut>>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success)
                return Result.Fail<TOut>(result.Error);

            return await func().ConfigureAwait(DefaultConfigureAwait);
        }

        public static async Task<Result> OnSuccess<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, Task<Result>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success)
                return Result.Fail(result.Error);

            return await func(result.Value).ConfigureAwait(DefaultConfigureAwait);
        }

        public static async Task<Result> OnSuccess(this Task<Result> resultTask, Func<Task<Result>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success)
                return result;

            return await func().ConfigureAwait(DefaultConfigureAwait);
        }

        public static async Task<Result<TIn>> Ensure<TIn>(this Task<Result<TIn>> resultTask,
            Func<TIn, Task<bool>> predicate,
            Error errorMessage, Action<TIn> actionOnError = null)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success)
                return result;

            if (!await predicate(result.Value).ConfigureAwait(DefaultConfigureAwait))
            {
                actionOnError?.Invoke(result.Value);
                return Result.Fail<TIn>(errorMessage);
            }

            return result;
        }

        public static async Task<Result> Ensure(this Task<Result> resultTask, Func<Task<bool>> predicate,
            Error errorMessage, Action actionOnError = null)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success)
                return result;

            if (!await predicate().ConfigureAwait(DefaultConfigureAwait))
            {
                actionOnError?.Invoke();
                return Result.Fail(errorMessage);
            }

            return result;
        }

        public static Task<Result<TOut>> Map<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<TOut>> func)
            => resultTask.OnSuccess(func);

        public static Task<Result<TIn>> Map<TIn>(this Task<Result> resultTask, Func<Task<TIn>> func)
            => resultTask.OnSuccess(func);

        public static async Task<Result<TIn>> OnSuccess<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, Task> action)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (result.Success)
            {
                await action(result.Value).ConfigureAwait(DefaultConfigureAwait);
            }

            return result;
        }

        public static async Task<Result> OnSuccess(this Task<Result> resultTask, Func<Task> action)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (result.Success)
            {
                await action().ConfigureAwait(DefaultConfigureAwait);
            }

            return result;
        }

        public static async Task<TIn> OnBoth<TIn>(this Task<Result> resultTask, Func<Result, Task<TIn>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return await func(result).ConfigureAwait(DefaultConfigureAwait);
        }

        public static async Task<TOut> OnBoth<TIn, TOut>(this Task<Result<TIn>> resultTask,
            Func<Result<TIn>, Task<TOut>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return await func(result).ConfigureAwait(DefaultConfigureAwait);
        }

        public static async Task<Result<TIn>> OnFailure<TIn>(this Task<Result<TIn>> resultTask, Func<Task> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success)
            {
                await func().ConfigureAwait(DefaultConfigureAwait);
            }

            return result;
        }

        public static async Task<Result> OnFailure(this Task<Result> resultTask, Func<Task> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success)
            {
                await func().ConfigureAwait(DefaultConfigureAwait);
            }

            return result;
        }

        public static async Task<Result<TIn>> OnFailure<TIn>(this Task<Result<TIn>> resultTask, Func<Error, Task> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success)
                await func(result.Error).ConfigureAwait(DefaultConfigureAwait);

            return result;
        }

        public static async Task<Result<TIn>> OnFailureCompensate<TIn>(this Task<Result<TIn>> resultTask,
            Func<Task<Result<TIn>>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success)
                return await func().ConfigureAwait(DefaultConfigureAwait);

            return result;
        }

        public static async Task<Result<TIn>> OnFailureCompensate<TIn>(this Task<Result<TIn>> resultTask,
            Func<Error, bool> errorFilter, Func<Task<Result<TIn>>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success && errorFilter(result.Error))
                return await func().ConfigureAwait(DefaultConfigureAwait);

            return result;
        }

        public static Task<Result<TIn>> OnFailureCompensate<TIn>(this Task<Result<TIn>> resultTask,
            int errorCode, Func<Task<Result<TIn>>> func)
        {
            return resultTask.OnFailureCompensate(err => err.Code == errorCode, func);
        }

        public static async Task<Result> OnFailureCompensate(this Task<Result> resultTask, Func<Task<Result>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success)
                return await func().ConfigureAwait(DefaultConfigureAwait);

            return result;
        }

        public static async Task<Result> OnFailureCompensate(this Task<Result> resultTask,
            Func<Error, bool> errorFilter, Func<Task<Result>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success && errorFilter(result.Error))
                return await func().ConfigureAwait(DefaultConfigureAwait);

            return result;
        }

        public static Task<Result> OnFailureCompensate(this Task<Result> resultTask, int errorCode,
            Func<Task<Result>> func)
        {
            return resultTask.OnFailureCompensate(err => err.Code == errorCode, func);
        }

        public static async Task<Result<TIn>> OnFailureCompensate<TIn>(this Task<Result<TIn>> resultTask,
            Func<Error, Task<Result<TIn>>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);

            if (!result.Success)
                return await func(result.Error).ConfigureAwait(DefaultConfigureAwait);

            return result;
        }
    }
}
