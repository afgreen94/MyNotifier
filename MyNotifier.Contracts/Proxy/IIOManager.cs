using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Proxy
{
    public interface IIOManager : Updaters.IIOManager, EventModules.IIOManager
    {
    }
}
