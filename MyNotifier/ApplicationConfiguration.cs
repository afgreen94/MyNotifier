using Microsoft.Extensions.Configuration;
using MyNotifier.Base;
using MyNotifier.Commands;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Proxy;
using MyNotifier.Notifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier
{
    public class ApplicationConfiguration : ConfigurationWrapper, IApplicationConfiguration
    {

        //private const string SystemSettingsKey = "SystemSettings";
        //private const string DriveLoopSettingsKey = "DriveLoopSettings";

        private SystemSettings systemSettings;
        private DriverLoopSettings driverLoopSettings;

        public SystemSettings SystemSettings 
        {
            get { return this.systemSettings; }
            set { this.systemSettings = value; }
        }
        public DriverLoopSettings DriverLoopSettings
        {
            get { return this.driverLoopSettings; }
            set { this.DriverLoopSettings = value; }
        }

        public ApplicationConfiguration(IConfiguration innerConfiguration) : base(innerConfiguration) => this.BuildAndValidate(); //logic in constructor 


        private void BuildAndValidate()
        {

            this.systemSettings = new();

            var systemSettingsSection = this.innerConfiguration.GetSection(nameof(SystemSettings)) ?? throw new Exception("Could not retrieve system settings section from application configuration.");
            var systemSchemeStr = systemSettingsSection.GetValue<string>(nameof(SystemSettings.Scheme)); if (string.IsNullOrEmpty(systemSchemeStr)) throw new Exception("Could not retrieve system scheme value from application configuration.");
            var settings = systemSettingsSection.GetValue<object>(nameof(SystemSettings.Settings)) ?? throw new Exception("Could not retrieve settings from system settings.");

            this.systemSettings = new()
            {
                Scheme = EnumStringMaps.GetSystemScheme(systemSchemeStr),
                Settings = settings
            };

            this.driverLoopSettings = this.innerConfiguration.GetValue<DriverLoopSettings>(nameof(DriverLoopSettings)) ?? throw new Exception("Could not retrieve driver loop settings from application configuration.");
        }

        //private T GetFromInnerConfiguration<T>(bool suppressException = false)
        //{
        //    var tName = nameof(T);

        //    var ret = this.innerConfiguration.GetValue<T>(tName);

        //    if (ret == null && !suppressException) throw new Exception($"Could not retrieve {tName} from application configuration");

        //    return ret;
        //}

        public class Controller : IControllable<ChangeApplicationConfiguration>
        {
            private readonly IApplicationConfiguration applicationConfiguration;

            public void OnCommand(ChangeApplicationConfiguration command)
            {

            }
        }
    }
}
