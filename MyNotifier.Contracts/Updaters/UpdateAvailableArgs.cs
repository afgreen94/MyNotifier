using MyNotifier.Contracts.EventModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Updaters
{
    public class UpdateAvailableArgs
    {
        public IInterest Interest { get; set; }
        public IEventModule EventModule { get; set; }
        public IUpdater Updater { get; set; }
        public IUpdaterResult Result { get; set; }
    }
}
