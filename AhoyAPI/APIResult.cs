using System;
namespace AhoyAPI
{
    public abstract class APIResult
    {
        public bool Success { get; }

        public APIResult(bool success)
        {
            this.Success = success;
        }
    }

    public class APIFailureResult : APIResult
    {
        public string Message { get; }

        public APIFailureResult(string message) : base(false)
        {
            this.Message = message;
        }
    }

    public class APISuccessResult<T> : APIResult
    {
        public T Data { get; }

        public APISuccessResult(T data) : base(true)
        {
            this.Data = data;
        }
    }
}
