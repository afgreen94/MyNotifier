using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Base
{
    public interface IInitializeable
    {
        bool Initialized { get; }
        int InitializerOrder { get; }

        ValueTask<ICallResult> InitializeAsync(bool forceReinitialize = false);
    }

    public interface IInitializeable<TInitializerProperties>
    {
        bool Initialized { get; }
        int InitializerOrder { get; }

        ValueTask<ICallResult> InitializeAsync(TInitializerProperties initializerProperties, bool forceReinitialize = false);
    }
}
