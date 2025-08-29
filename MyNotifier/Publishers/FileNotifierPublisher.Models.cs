using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Publishers
{
    public partial class FileNotifierPublisher
    {
        public new interface IConfiguration : NotifierPublisher.IConfiguration  //should wrap in proxySettings, set earlier in config, provide all relevant values, already set 
        {
            string PublishDirectoryRoot { get; }
            string DefaultDataFileName { get; }
            string DefaultMetadataFileName { get; }
            bool AllowOverwriteExistingNotification { get; } //default to false 
            WriteCompleteSignalArgs WriteCompleteSignalArgs { get; }
        }
        public new class Configuration : NotifierPublisher.Configuration, IConfiguration
        {

            public string PublishDirectoryRoot { get; set; }
            public bool AllowOverwriteExistingNotification { get; set; }
            public string DefaultDataFileName { get; set; }
            public string DefaultMetadataFileName { get; set; }
            public WriteCompleteSignalArgs WriteCompleteSignalArgs { get; set; }

            public Configuration(Microsoft.Extensions.Configuration.IConfiguration innerConfiguration) : base(innerConfiguration) { }
        }
        public class WriteCompleteSignalArgs
        {
            public string Name { get; set; } = "write_complete";
        }
    }
}
