using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Base
{
    public interface ICallResult
    {
        bool Success { get; }
        string ErrorText { get; }

    }

    public interface ICallResult<TResult> : ICallResult
    {
        TResult Result { get; }
    }
}
