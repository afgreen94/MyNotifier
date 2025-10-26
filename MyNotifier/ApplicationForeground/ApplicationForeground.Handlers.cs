using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MyNotifier.ApplicationForeground;

namespace MyNotifier
{
    public partial class ApplicationForeground
    {

        //Handlers, can register and locate dynamically. for now ... !!! 
        private async Task<ICallResult> ProcessUpdateMessageAsync(Message message)
        {
            try
            {
                if (!Message.TryCastAs<UpdateAvailableMessage>(message, out var updateAvailableMessage)) { return new CallResult(false, "Failed Message cast to UpdateAvailable."); }

                var notification = this.notificationBuilder.Build(updateAvailableMessage.Value.Interest,
                                                                  updateAvailableMessage.Value.EventModule,
                                                                  updateAvailableMessage.Value.Updater,
                                                                  updateAvailableMessage.Value.Result);

                return await this.publisher.PublishAsync(notification).ConfigureAwait(false);
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        private async Task<ICallResult> ProcessCommandIssuedMessageAsync(Message message)
        {
            try
            {
                if (!Message.TryCastAs<CommandIssuedMessage>(message, out var commandIssuedMessage)) { return new CallResult(false, "Failed Message cast to CommandIssued."); }

                //implement command 

                //return command result if requested 

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        private async Task<ICallResult> ProcessFailureMessageAsync(Message message)
        {
            try
            {
                if (!Message.TryCastAs<FailureMessage>(message, out var failureMessage)) { return new CallResult(false, "Failed Message cast to Failure."); }


                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }
    }
}
