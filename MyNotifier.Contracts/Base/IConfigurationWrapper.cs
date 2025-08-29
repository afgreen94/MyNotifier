using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Base
{
    public interface IConfigurationWrapper
    {
        Microsoft.Extensions.Configuration.IConfiguration InnerConfiguration { get; }
    }

    public interface IApplicationConfigurationWrapper : IConfigurationWrapper
    {
        IApplicationConfiguration InnerApplicationConfiguration { get; }
    }
}
