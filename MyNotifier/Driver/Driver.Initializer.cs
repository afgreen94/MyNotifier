using Microsoft.Extensions.DependencyInjection;
using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using MyNotifier.Contracts.Proxy;
using MyNotifier.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MyNotifier.FileIOManager;
using MyNotifier.Proxy;
using System.IO;
using MyNotifier.Contracts.Updaters;

namespace MyNotifier
{
    public partial class Driver
    {

        //essentially a proxy fileserver intitializer //encapsulate in proxyFileServerIntializer class !!! 

        //should initializer load updater dlls? or inner systemIniailizer? for now it is inner SystemInitializer 
        protected class Initializer
        {
            private readonly IApplicationConfiguration configuration;
            private readonly SecureString sessionKey;  //for encryptions & client keys...etc 

            public Initializer(IApplicationConfiguration configuration, SecureString sessionKey) { this.configuration = configuration; this.sessionKey = sessionKey; }

            public async ValueTask<Result> InitializeAsync()
            {
                try
                {

                    var notifierSystemInitializer = this.BuildInitializerServiceProvider()
                                                        .CreateScope()
                                                        .ServiceProvider
                                                        .GetRequiredService<INotifierSystemInitializer>();

                    var initializeResult = await notifierSystemInitializer.InitializeSystemAsync().ConfigureAwait(false); if (!initializeResult.Success) CallResult.BuildFailedCallResult(initializeResult, "Failed to initialize notifier system: {0}");

                    //use asyncEnumerator to provide dlls
                    //load assemblies here ??
                    //var updaterAssembliesResult = new CallResult<Assembly[]>(null);
                    //var applicationServiceProvider = this.BuildApplicationServiceProvider(initializeResult.Catalog, initializeResult.Interests, updaterAssembliesResult.Result);

                    var sessionInterests = this.BuildSessionInterests(initializeResult.InterestModels);

                    return new Result()
                    {
                        Catalog = initializeResult.Catalog,
                        //SessionInterests = initializeResult.Interests,
                        ServiceProvider = initializeResult.ServiceProvider
                    };
                }
                catch (Exception ex) { return new Result(false, ex.Message); }
            }


            private ServiceProvider BuildInitializerServiceProvider()
            {
                var services = new ServiceCollection();
                new Registrar().RegisterServices(services, this.configuration);
                return services.BuildServiceProvider();
            }


            private ISessionInterests BuildSessionInterests(InterestModel[] interestModels)
            {
                throw new NotImplementedException();
            }


            protected class Registrar
            {
                public void RegisterServices(IServiceCollection services, IApplicationConfiguration configuration)
                {
                    services.AddSingleton(configuration.InnerConfiguration);
                    services.AddSingleton(configuration);

                    switch (configuration.SystemSettings.Scheme)
                    {
                        case SystemScheme.ProxyFileIOServer:
                            this.RegisterProxyFileIOServerServices(services, configuration);
                            break;
                        case SystemScheme.DirectToClient:
                            this.RegisterDirectToClientServices(services);
                            break;
                        default:
                            throw new Exception("Invalid system scheme."); //should never be reached 
                    }

                    //notifierInitializer
                }

                private void RegisterProxyFileIOServerServices(IServiceCollection services, IApplicationConfiguration configuration)
                {

                    //determine proxy 
                    //add proxy config (initializer, publisher, etc...)
                    //proxySettings 

                    var proxyInitializerConfiguration = new ProxyFileServerInitializer.Configuration(configuration);
                    var proxySettings = proxyInitializerConfiguration.ProxySettings;

                    services.AddSingleton(proxyInitializerConfiguration);

                    this.RegisterFileIOManagerServices(services, proxySettings.ProxyHost);

                    services.AddScoped<ICallContext<ProxyFileServerInitializer.ProxyFileServerInitializerIOManager>, CallContext<ProxyFileServerInitializer.ProxyFileServerInitializerIOManager>>();
                    services.AddScoped<ProxyFileServerInitializer.IProxyFileServerInitializerIOManager, ProxyFileServerInitializer.ProxyFileServerInitializerIOManager>();

                    services.AddScoped<ICallContext<ProxyFileServerInitializer>, CallContext<ProxyFileServerInitializer>>();
                    services.AddScoped<INotifierSystemInitializer, ProxyFileServerInitializer>();

                }

                private void RegisterDirectToClientServices(IServiceCollection services) => throw new Exception("Direct to client system scheme not implemented");
                private void RegisterFileIOManagerServices(IServiceCollection services, FileStorageProvider fileStorageProvider) => new FileIOManager.Registrar().RegisterFileIOManagerServices(services, fileStorageProvider);
            }



            public interface ISessionInterests
            {
                public IInterest[] Interests { get; }
            }

            public interface INotifierSystemInitializerResult : ICallResult
            {
                Catalog Catalog { get; }
                ISessionInterests SessionInterests { get; }
                ServiceProvider ServiceProvider { get; }
            }

            public class Result : CallResult, INotifierSystemInitializerResult
            {
                public Catalog Catalog { get; set; }
                public ISessionInterests SessionInterests { get; set; }
                public ServiceProvider ServiceProvider { get; set; }

                public Result() : base() { }
                public Result(bool success, string errorText) : base(success, errorText) { }
            }
        }
    }
}
