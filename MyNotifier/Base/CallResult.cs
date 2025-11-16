using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.Base;

namespace MyNotifier.Base
{
    public class CallResult : ICallResult
    {
        public bool Success { get; set; }
        public string ErrorText { get; set; } = string.Empty;

        public CallResult()
        {
            this.Success = true;
        }

        public CallResult(bool success, string errorText)
        {
            this.Success = success;
            this.ErrorText = errorText;
        }

        //replace message format with innerMessageFormat, factor out "{0}" !!! 
        public static CallResult BuildFailedCallResult(ICallResult innerCallResult, string messageFormat) => new(false, string.Format(messageFormat, innerCallResult.ErrorText));

        // => new(false, string.Format(new StringBuilder(messageFormat).Append(" {0}").ToString(), innerCallResult.ErrorText);

        public static CallResult FromException(Exception ex, string format = "")
        {
            var message = string.IsNullOrEmpty(format) ? ex.Message : string.Format(format, ex.Message);
            return new CallResult(false, message);
        }
    }

    public class CallResult<TResult> : CallResult, ICallResult<TResult>
    {
        public TResult Result { get; set; }

        public CallResult() : base() { }
        public CallResult(bool success, string errorText) : base(success, errorText) { }
        public CallResult(TResult result) : base() { this.Result = result; }

        public static CallResult<TResult> BuildFailedCallResult(ICallResult innerCallResult, string messageFormat) => new(false, string.Format(messageFormat, innerCallResult.ErrorText));
        public static CallResult<TResult> FromException(Exception ex, string format = "")
        {
            var message = string.IsNullOrEmpty(format) ? ex.Message : string.Format(format, ex.Message);
            return new CallResult<TResult>(false, message);
        }
    }
}
