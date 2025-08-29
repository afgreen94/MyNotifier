using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.Base;

namespace MyNotifier.Base
{
    public class CallContext : ICallContext
    {
    }

    public class CallContext<TService> : CallContext, ICallContext<TService>
    {

    }
}
