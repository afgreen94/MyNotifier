using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MyNotifier.Contracts;

namespace MyNotifier.Base
{
    public abstract class ConfigurationWrapper(IConfiguration innerConfiguration)
    {
        protected readonly IConfiguration innerConfiguration = innerConfiguration;
        protected virtual string SectionKey => string.Empty;

        public virtual IConfiguration InnerConfiguration => this.innerConfiguration;
    }

    public abstract class ApplicationConfigurationWrapper : ConfigurationWrapper
    {
        protected readonly IApplicationConfiguration innerApplicationConfiguration;

        public virtual IApplicationConfiguration InnerApplicationConfiguration => this.innerApplicationConfiguration;

        protected ApplicationConfigurationWrapper(IApplicationConfiguration innerApplicationConfiguration) : base(innerApplicationConfiguration.InnerConfiguration) { this.innerApplicationConfiguration = innerApplicationConfiguration; }
    }
}
