using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Updaters
{
    public interface IUpdaterSet  //updaters that share parameter values ie {pd0} -> updater0, updater1, ...  updaterN
    {
        IDefinition Definition { get; }
        IUpdaterDefinition UpdaterDefinitions { get; }
        IParameterDefinition CommonParameterDefinitions { get; }
    }
}
