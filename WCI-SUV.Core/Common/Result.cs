using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCI_SUV.Core.Common
{
    public class Result<T>
    {
        public bool isSuccess { get; private set; }
        public T? Value { get; private set; }
        public string? ErrorMessage { get; private set; }

        public Result(bool success, T? val, string? errMessage)
        {
            isSuccess = success;
            Value = val;
            ErrorMessage = errMessage;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, null);
        }

        public static Result<T> Failure(string errMessage)
        {
            return new Result<T>(true, default, errMessage);
        }


    }
}
