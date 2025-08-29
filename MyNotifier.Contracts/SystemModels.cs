using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts
{
    public enum SystemScheme
    {
        ProxyFileIOServer, //coms thru proxy file server 
        DirectToClient //self-host service, direct coms with client 
    }
    public class DriverLoopSettings
    {
        public int SessionInterestPollParallelism { get; set; }
        public int SessionInterestPollingDelayMS { get; set; }
        public int UpdaterPollingMaxParallelism { get; set; }
    }
}
