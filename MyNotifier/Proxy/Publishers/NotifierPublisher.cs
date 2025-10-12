using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Publishers;
using MyNotifier.Publishers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IIOManager = MyNotifier.Contracts.Proxy.Publishers.IIOManager;

namespace MyNotifier.Proxy.Publishers
{
    //could appropriately derive Proxy/NotifierPublisher from FileNotifierPublisher and have IOManager implement IFileIOManager, use IOManager as IFileIOManager dependency to FileNotifierPublisher
    //could do same with Proxy/Notifier
    //for now, seems convoluted, though probably better practice 

    public class NotifierPublisher : MyNotifier.Publishers.NotifierPublisher 
    {
        private readonly IIOManager ioManager;
        private readonly ICallContext<NotifierPublisher> callContext;

        public NotifierPublisher(IIOManager ioManager, ICallContext<NotifierPublisher> callContext) { this.ioManager = ioManager; this.callContext = callContext; }

        protected override async ValueTask<ICallResult> PublishCoreAsync(PublishArgs publishArgs) //handle try/catch? init, etc... ? 
        {
            var notification = this.BuildNotification(publishArgs);
            return await this.ioManager.WriteNotificationFilesAsync(notification).ConfigureAwait(false);
        }
    }
}
