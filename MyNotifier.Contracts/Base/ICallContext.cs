using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Base
{
    public interface ICallContext { }

    public interface ICallContext<out T> : ICallContext { }
}
