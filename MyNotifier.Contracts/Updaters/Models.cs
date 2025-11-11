using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;

namespace MyNotifier.Contracts.Updaters
{
    public class UpdaterArgs //parameters system
    {
        public IUpdaterDefinition[] Definitions { get; set; }
        public IDictionary<Guid, Parameter[]> Parameters { get; set; }
    }


    //public abstract class UpdaterModuleDescription : IUpdaterModuleDescription
    //{
    //    public abstract string AssemblyName { get; }
    //    public abstract string TypeFullName { get; }
    //    public abstract string DefinitionTypeFullName { get; }
    //}

    public class UpdaterModuleLibrary
    {
        public IUpdaterDefinition Updaters { get; set; }
        public string[] Dlls { get; set; }
        public IDictionary<string, Guid[]> DllToUpdaterByUpdaterIdMap { get; set; }
    }

    public class ModuleLibraryArgs
    {

    }

    public class UpdaterModule 
    {

    }

    public class UpdaterSet
    {
        public IUpdater[] Updaters { get; }
        public Parameter[] CommonParameters { get; }
    }

    public class UpdaterParameterWrapper
    {
        public IUpdater Updater { get; set; }
        public Parameter[] Parameters { get; set; }
    }

    public interface ITaskSettings
    {
        int DelayMilliseconds { get; }
        string ReturnProtocol { get; }
    }
}
