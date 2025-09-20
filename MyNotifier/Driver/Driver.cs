using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security;
using System.Text;
using System.Text.Json;
using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Publishers;
using MyNotifier.Contracts.Notifiers;
using MyNotifier.Contracts.Updaters;
using MyNotifier.Contracts.Proxy;
using System.Threading;
using MyNotifier.Contracts.FileIOManager;
using System.Reflection;
using MyNotifier.Notifiers;
using MyNotifier.Commands;

namespace MyNotifier
{

    //not using fileiofactory for now 
    //nore notifierpublisherfactory
    //ie 1 notifierPublisher for all interests //for now 
    //per interest publishers later. not really necessary 
    //maybe don't need callresults for fileIOmanagers //actually should rework whole call result system 

    //still need command system !!! 
    //logging, etc.

    public partial class Driver
    {
        private readonly IApplicationConfiguration configuration;

        public Driver(IApplicationConfiguration configuration) { this.configuration = configuration; }
        public Driver(IConfiguration innerConfiguration) : this(new ApplicationConfiguration(innerConfiguration)) { }

        public async Task DriveAsync(SecureString sessionKey)
        {
            //this.ValidateConfiguration();
            var initializeResult = await new Initializer(this.configuration, sessionKey).InitializeAsync().ConfigureAwait(false); 
            if (!initializeResult.Success) throw new Exception($"Initialization failed: {initializeResult.ErrorText}");

            //await this.DriveCoreAsync(initializeResult.ServiceProvider, initializeResult.SessionInterests).ConfigureAwait(false);

            await this.DriveCore0Async(initializeResult.ServiceProvider, []);
        }


        //private async Task DriveCoreAsync(IServiceProvider serviceProvider, Interest[] sessionInterests)
        //{
        //    using var scope = serviceProvider.CreateScope();

        //    var publisher = scope.ServiceProvider.GetRequiredService<INotifierPublisher>();  //should publisher have initialize? probably 
        //    var updaterFactory = scope.ServiceProvider.GetRequiredService<IUpdaterFactory>();

        //    var cancellationToken = false;

        //    await new Loop(publisher,
        //                   updaterFactory,
        //                   sessionInterests,
        //                   this.configuration.DriverLoopSettings)
        //                  .RunAsync(cancellationToken).ConfigureAwait(false);
        //}



        

        public class ModuleLoader { } //easier to register all updater definitions at initialize step. Better to register lazily 

        



        private InterestPool interestPool = new();

        private async Task DriveCore0Async(IServiceProvider serviceProvider, InterestModel[] sessionInterests)
        {


            //using var scope = serviceProvider.CreateScope();

            //var applicationConfiguration = scope.ServiceProvider.GetRequiredService<IApplicationConfiguration>();
            //var updaterFactory = scope.ServiceProvider.GetRequiredService<IUpdaterFactory>();
            //var commandNotifierWrapper = scope.ServiceProvider.GetRequiredService<CommandNotifierWrapper>();
            //var controlObject = scope.ServiceProvider.GetRequiredService<CommandObject>();


            //foreach (var sessionInterest in sessionInterests)
            //{
            //    var interest = sessionInterest.BuildInterest(updaterFactory);

            //    foreach(var eventModule in interest.EventModules)
            //    {
            //        foreach (var kvp in eventModule.UpdaterParameterWrappers)
            //        {
            //            UpdaterParametersWrapper wrapper = kvp.Value;
            //            IUpdater updater = wrapper.Updater;


            //            await updater.InitializeAsync().ConfigureAwait(false);

            //        }
            //    }

            //    this.interestPool.TryAdd(interest);
            //}


            //await commandNotifierWrapper.ConnectAsync(null).ConfigureAwait(false);


            //var interestPoolController = new InterestPool.Controller();

            //controlObject.RegisterControllable<RegisterNewInterests>(interestPoolController);




        }

        //private ICallResult ValidateConfiguration() //encapsulated within config itself? 
        //{
        //    try
        //    {
        //        //validation logic 

        //        return new CallResult();
        //    }
        //    catch (Exception ex) { return CallResult.FromException(ex); }
        //}
    }
}
