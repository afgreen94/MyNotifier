using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.Base;

namespace MyNotifier.Base
{
    public class Initializer
    {
        public static async ValueTask<ICallResult> InitializeAllAsync(IInitializeable[] initializeables, bool forceReinitialize = false, bool suppressOrdering = false)
        {
            try
            {
                if(!suppressOrdering) initializeables = initializeables.OrderBy(i => i.InitializerOrder).ToArray();

                foreach (var initializeable in initializeables)
                {
                    if (!initializeable.Initialized || forceReinitialize)
                    {
                        var result = await initializeable.InitializeAsync(forceReinitialize).ConfigureAwait(false);

                        if (!result.Success) { return CallResult.BuildFailedCallResult(result, $"Failed to initialize {initializeable.GetType().FullName}: {{0}}"); }
                    }
                }

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex);  }
        }
    }
}
