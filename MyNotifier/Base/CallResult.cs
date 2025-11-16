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

        public static CallResult BuildFailedCallResult(ICallResult innerCallResult, string prefixMessage = "") => BuildFailedCallResultCore(innerCallResult.ErrorText, prefixMessage);
        public static CallResult FromException(Exception ex, string prefixMessage = "") => BuildFailedCallResultCore(ex.Message, prefixMessage);

        private static CallResult BuildFailedCallResultCore(string errorText, string prefixMessage = "")
        {
            if (string.IsNullOrEmpty(prefixMessage)) return new CallResult(false, errorText);

            var sb = new StringBuilder(prefixMessage);
            if (!string.IsNullOrEmpty(prefixMessage) && !prefixMessage.EndsWith(": {0}"))
            {
                sb.Append(": {0}");
            }
            return new CallResult(false, string.Format(sb.ToString(), errorText));
        }
    }

    public class CallResult<TResult> : CallResult, ICallResult<TResult>
    {
        public TResult Result { get; set; }

        public CallResult() : base() { }
        public CallResult(bool success, string errorText) : base(success, errorText) { }
        public CallResult(TResult result) : base() { this.Result = result; }

        public static CallResult<TResult> BuildFailedCallResult(ICallResult innerCallResult, string prefixMessage = "") => BuildFailedCallResultCore(innerCallResult.ErrorText, prefixMessage);
        public static CallResult<TResult> FromException(Exception ex, string prefixMessage = "") => BuildFailedCallResultCore(ex.Message, prefixMessage);

        private static CallResult<TResult> BuildFailedCallResultCore(string errorText, string prefixMessage = "")
        {
            if (string.IsNullOrEmpty(prefixMessage)) return new CallResult<TResult>(false, errorText);

            var sb = new StringBuilder(prefixMessage);
            if (!string.IsNullOrEmpty(prefixMessage) && !prefixMessage.EndsWith(": {0}"))
            {
                sb.Append(": {0}");
            }
            return new CallResult<TResult>(false, string.Format(sb.ToString(), errorText));
        }
    }
}
