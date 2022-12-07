namespace CryptoTrader.Utils
{
    public class Result
    {
        public bool Success { get; set; }

        public Error Error { get; set; }

        public static implicit operator Result(Error error)
        {
            return new Result
            {
                Success = false,
                Error = error
            };
        }

        public static Result Ok()
        {
            return new Result
            {
                Success = true
            };
        }

        public static Result<T> Ok<T>(T result)
        {
            return new Result<T>
            {
                Success = true,
                Value = result
            };
        }

        public static Result Fail(Error error)
        {
            return new Result
            {
                Success = false,
                Error = error
            };
        }

        public static Result<T> Fail<T>(Error error)
        {
            return new Result<T>
            {
                Success = false,
                Error = error
            };
        }
    }

    public class Result<T> : Result
    {
        public virtual T Value { get; set; }

        public static implicit operator Result<T>(T value)
        {
            return new Result<T>
            {
                Value = value,
                Success = true
            };
        }

        public static implicit operator Result<T>(Error error)
        {
            return new Result<T>
            {
                Success = false,
                Error = error
            };
        }
    }
}
