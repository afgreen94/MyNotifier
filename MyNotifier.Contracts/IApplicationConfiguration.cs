using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts
{
    public interface IApplicationConfiguration : IConfigurationWrapper
    {
        SystemSettings SystemSettings { get; }
        DriverLoopSettings DriverLoopSettings { get; }


        IControllable Controllable { get; }
    }


    public class SystemSettings
    {
        public SystemScheme Scheme { get; set; }
        public object Settings { get; set; }  //generic to allow for differnt types according to system scheme 
    }
}
