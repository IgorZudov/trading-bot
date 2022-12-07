using System;
using System.Threading.Tasks;

namespace CryptoTrader.Utils.Results
{
    public static class AsyncResultExtensionsL
    {
        public static bool DefaultConfigureAwait;

        public static async Task<Result<TOut>> OnSuccess<TIn, TOut>(this Task<Result<TIn>> resultTask,
            Func<TIn, TOut> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnSuccess(func);
        }

        public static async Task<Result<TIn>> OnSuccess<TIn>(this Task<Result> resultTask, Func<TIn> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnSuccess(func);
        }

        public static async Task<Result<TOut>> OnSuccess<TIn, TOut>(this Task<Result<TIn>> resultTask,
            Func<TIn, Result<TOut>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnSuccess(func);
        }

        public static async Task<Result<TIn>> OnSuccess<TIn>(this Task<Result> resultTask, Func<Result<TIn>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnSuccess(func);
        }

        public static async Task<Result<TOut>> OnSuccess<TIn, TOut>(this Task<Result<TIn>> resultTask,
            Func<Result<TOut>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnSuccess(func);
        }

        public static async Task<Result> OnSuccess<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, Result> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnSuccess(func);
        }

        public static async Task<Result> OnSuccess(this Task<Result> resultTask, Func<Result> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnSuccess(func);
        }

        public static async Task<Result<TIn>> Ensure<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, bool> predicate,
            Error error, Action<TIn> actionOnError = null)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.Ensure(predicate, error, actionOnError);
        }

        public static async Task<Result<TIn>> Ensure<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, bool> predicate)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.Ensure(predicate);
        }

        public static async Task<Result> Ensure(this Task<Result> resultTask, Func<bool> predicate, Error error,
            Action actionOnError = null)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.Ensure(predicate, error, actionOnError);
        }

        public static Task<Result<TOut>> Map<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, TOut> func)
            => resultTask.OnSuccess(func);

        public static Task<Result<TIn>> Map<TIn>(this Task<Result> resultTask, Func<TIn> func)
            => resultTask.OnSuccess(func);

        public static async Task<Result<TIn>> OnSuccess<TIn>(this Task<Result<TIn>> resultTask, Action<TIn> action)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnSuccess(action);
        }

        public static async Task<Result> OnSuccess(this Task<Result> resultTask, Action action)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnSuccess(action);
        }

        public static async Task<TIn> OnBoth<TIn>(this Task<Result> resultTask, Func<Result, TIn> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnBoth(func);
        }

        public static async Task<TOut> OnBoth<TIn, TOut>(this Task<Result<TIn>> resultTask,
            Func<Result<TIn>, TOut> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnBoth(func);
        }

        public static async Task<Result<TIn>> OnFailure<TIn>(this Task<Result<TIn>> resultTask, Action action)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnFailure(action);
        }

        public static async Task<Result> OnFailure(this Task<Result> resultTask, Action action)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnFailure(action);
        }

        public static async Task<Result<TIn>> OnFailure<TIn>(this Task<Result<TIn>> resultTask, Action<Error> action)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnFailure(action);
        }

        public static async Task<Result> OnFailure(this Task<Result> resultTask, Action<Error> action)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnFailure(action);
        }

        public static async Task<Result<TIn>> OnFailureCompensate<TIn>(this Task<Result<TIn>> resultTask,
            Func<Result<TIn>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnFailureCompensate(func);
        }

        public static async Task<Result> OnFailureCompensate(this Task<Result> resultTask, Func<Result> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnFailureCompensate(func);
        }

        public static async Task<Result<TIn>> OnFailureCompensate<TIn>(this Task<Result<TIn>> resultTask,
            Func<Error, Result<TIn>> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnFailureCompensate(func);
        }

        public static async Task<Result> OnFailureCompensate(this Task<Result> resultTask, Func<Error, Result> func)
        {
            var result = await resultTask.ConfigureAwait(DefaultConfigureAwait);
            return result.OnFailureCompensate(func);
        }
    }
}
