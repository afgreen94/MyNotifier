using MyNotifier.Contracts.EventModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Updaters
{
    public interface ISubscriber : Contracts.ISubscriber
    {
        void OnUpdateAvailable(UpdateAvailableArgs args);
    }
}
