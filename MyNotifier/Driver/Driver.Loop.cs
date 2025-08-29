//using MyNotifier.Contracts.Publishers;
//using MyNotifier.Contracts.Updaters;
//using MyNotifier.Contracts;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using MyNotifier.Contracts.Notifications;

//namespace MyNotifier
//{
//    public partial class Driver
//    {
//        protected class Loop(INotifierPublisher publisher,
//                             IUpdaterFactory updaterFactory,
//                             Interest[] sessionInterests,
//                             DriverLoopSettings settings)
//        {
//            private readonly INotifierPublisher publisher = publisher;
//            private readonly IUpdaterFactory updaterFactory = updaterFactory;
//            private readonly DriverLoopSettings settings = settings;

//            public async Task RunAsync(bool cancellationToken = false)
//            {
//                //driverLoop
//                while (!cancellationToken)
//                {
//                    //PARALLELIZE 
//                    //for now, foreground, synchronous, periodic scan thru interests
//                    //could parallelize/background updater scans, run individually as daemons, rather than looped thru in foreground. for now, leave alone 

//                    foreach (var interest in sessionInterests)
//                    {
//                        foreach (var updaterDefinition in interest.UpdaterArgs.Definitions)
//                        {
//                            //var publisher = this.publisherFactory.GetNotifierPublisher(interest.NotifierArgs.FactoryArg); //not using dynamic publishers for now //dynamic publisher => interest maps to channel[], given to publisher factory, publisher -> channel
//                            var updater = this.updaterFactory.GetUpdater(updaterDefinition.Id);

//                            var updaterInitializeResult = await updater.InitializeAsync().ConfigureAwait(false);
//                            if (!updaterInitializeResult.Success) { /*skip, log, throw? idk*/ }

//                            var updateResult = await updater.TryGetUpdateAsync(interest.UpdaterArgs.Parameters[updaterDefinition.Id]).ConfigureAwait(false);
//                            if (!updateResult.Success) { /*again, idk...*/ }

//                            if (updateResult.UpdateAvailable)
//                            {
//                                var publishResult = await this.publisher.PublishAsync(new PublishArgs()  //build notification here. publisher should receive notification to publish
//                                {
//                                    InterestId = interest.Definition.Id,
//                                    UpdaterId = updaterDefinition.Id,
//                                    UpdateTime = updateResult.UpdatedAt,
//                                    Data = updateResult.Data,
//                                    TypeArgs = new TypeArgs() //maybe get from interest definition
//                                    {
//                                        NotificationType = NotificationType.Update, 
//                                        NotificationDataTypeArgs = updateResult.TypeArgs
//                                    }
//                                }).ConfigureAwait(false);

//                                if (!publishResult.Success) { /* grrrr.... */ }
//                            }
//                        }
//                    }

//                    await Task.Delay(this.settings.SessionInterestPollingDelayMS).ConfigureAwait(false);
//                }
//            }
//        }
//    }
//}
