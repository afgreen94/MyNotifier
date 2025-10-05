using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Updaters
{
    public class UpdaterParametersWrapper
    {
        public IUpdater Updater { get; set; }
        public Parameter[] Parameters { get; set; } //parameter[][] !!! for now, 1 set of parameters per updater. 1 updater per module. eventually make graph or something to allow for multiple sets of parameters for same updater  
    }
}
