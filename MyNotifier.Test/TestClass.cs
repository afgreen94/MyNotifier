using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Test
{
    public abstract class TestClass
    {

        protected virtual string AppsettingsPath => "appsettings.test.json";

        protected IServiceProvider serviceProvider;
        protected IConfiguration configuration;

        protected Action<IServiceCollection> registerTestServices = (sc) => { };

        protected virtual void Initialize()
        {
            this.configuration = this.GetConfiguration();

            var services = new ServiceCollection();

            services.AddSingleton(this.configuration);

            this.registerTestServices(services);

            this.RegisterServices(services);

            this.serviceProvider = services.BuildServiceProvider();


            this.InitializeCore();
        }

        protected virtual IConfiguration GetConfiguration() => new ConfigurationBuilder().AddJsonFile(this.AppsettingsPath).Build();
        protected virtual void InitializeCore() { }

        protected abstract void RegisterServices(IServiceCollection services);

    }
}
