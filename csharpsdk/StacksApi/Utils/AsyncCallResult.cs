using System;

namespace StacksForce.Utils
{
    public struct AsyncCallResult<T>
    {
        public T Data;
        public Error? Error;

        public bool IsSuccess => Error == null;
        public bool IsError => Error != null;

        public AsyncCallResult(T data) : this()
        {
            this.Data = data;
            this.Error = null;
        }

        public AsyncCallResult(Error error) : this()
        {
            this.Error = error;
            this.Data = default!;
        }

        public AsyncCallResult(Exception e) : this()
        {
            this.Error = new Error(e);
            this.Data = default!;
        }

        public override string ToString()
        {
            return IsSuccess ? "Ok: " + Data : "Fail: " + Error;
        }
    }
}
