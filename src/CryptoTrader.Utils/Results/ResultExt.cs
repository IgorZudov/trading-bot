using System;

namespace CryptoTrader.Utils.Results
{
    public static class ResultExt
    {
        public static Result<TOut> OnSuccess<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> func)
        {
            if (!result.Success)
                return Result.Fail<TOut>(result.Error);

            return Result.Ok(func(result.Value));
        }

        public static Result<TIn> OnSuccess<TIn>(this Result result, Func<TIn> func)
        {
            if (!result.Success)
                return Result.Fail<TIn>(result.Error);

            return Result.Ok(func());
        }

        public static Result<TOut> OnSuccess<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> func)
        {
            if (!result.Success)
                return Result.Fail<TOut>(result.Error);

            return func(result.Value);
        }

        public static Result<TIn> OnSuccess<TIn>(this Result result, Func<Result<TIn>> func)
        {
            if (!result.Success)
                return Result.Fail<TIn>(result.Error);

            return func();
        }

        public static Result<TOut> OnSuccess<TIn, TOut>(this Result<TIn> result, Func<Result<TOut>> func)
        {
            if (!result.Success)
                return Result.Fail<TOut>(result.Error);

            return func();
        }

        public static Result OnSuccess<TIn>(this Result<TIn> result, Func<TIn, Result> func)
        {
            if (!result.Success)
                return Result.Fail(result.Error);

            return func(result.Value);
        }

        public static Result OnSuccess(this Result result, Func<Result> func)
        {
            if (!result.Success)
                return result;

            return func();
        }

        public static Result<TIn> Ensure<TIn>(this Result<TIn> result, Func<TIn, bool> predicate, Error error,
            Action<TIn> actionOnError = null)
        {
            if (!result.Success)
                return result;

            if (!predicate(result.Value))
            {
                actionOnError?.Invoke(result.Value);
                return Result.Fail<TIn>(error);
            }

            return result;
        }

        public static Result<TIn> Ensure<TIn>(this Result<TIn> result, Func<TIn, bool> predicate)
        {
            if (!result.Success)
                return result;

            return !predicate(result.Value) ? new Error() : result;
        }

        public static Result Ensure(this Result result, Func<bool> predicate, Error error, Action actionOnError)
        {
            if (!result.Success)
                return result;

            if (!predicate())
            {
                actionOnError?.Invoke();
                return Result.Fail(error);
            }

            return result;
        }

        public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> func)
            => result.OnSuccess(func);

        public static Result<TIn> Map<TIn>(this Result result, Func<TIn> func)
            => result.OnSuccess(func);

        public static Result<TIn> OnSuccess<TIn>(this Result<TIn> result, Action<TIn> action)
        {
            if (result.Success)
            {
                action(result.Value);
            }

            return result;
        }

        public static Result OnSuccess(this Result result, Action action)
        {
            if (result.Success)
            {
                action();
            }

            return result;
        }

        public static TOut OnBoth<TOut>(this Result result, Func<Result, TOut> func)
        {
            return func(result);
        }

        public static TOut OnBoth<TIn, TOut>(this Result<TIn> result, Func<Result<TIn>, TOut> func)
        {
            return func(result);
        }

        public static Result<TIn> OnFailure<TIn>(this Result<TIn> result, Action action)
        {
            if (!result.Success)
            {
                action();
            }

            return result;
        }

        public static Result OnFailure(this Result result, Action action)
        {
            if (!result.Success)
            {
                action();
            }

            return result;
        }

        public static Result<TIn> OnFailure<TIn>(this Result<TIn> result, Action<Error> action)
        {
            if (!result.Success)
            {
                action(result.Error);
            }

            return result;
        }

        public static Result OnFailure(this Result result, Action<Error> action)
        {
            if (!result.Success)
            {
                action(result.Error);
            }

            return result;
        }

        public static Result<TIn> OnFailureCompensate<TIn>(this Result<TIn> result, Func<Result<TIn>> func) =>
            !result.Success ? func() : result;

        public static Result OnFailureCompensate(this Result result, Func<Result> func) =>
            !result.Success ? func() : result;

        public static Result<TIn> OnFailureCompensate<TIn>(this Result<TIn> result, Func<Error, Result<TIn>> func) =>
            !result.Success ? func(result.Error) : result;

        public static Result OnFailureCompensate(this Result result, Func<Error, Result> func) =>
            !result.Success ? func(result.Error) : result;
    }
}
