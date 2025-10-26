using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Notifications;

namespace MyNotifier.Contracts.Publishers
{
    public class PublicationDefinition : Definition { }
    //public class PublishArgs
    //{
    //    public Guid NotificationTypeId { get; set; }
    //    public string NotificationTypeName { get; set; }
    //    public string NotificationName { get; set; }
    //    public DateTime UpdateTime { get; set; }
    //    public byte[] Data { get; set; }
    //    public TypeArgs TypeArgs { get; set; }
    //}

    public class PublishArgs
    {
        public Notification Notification { get; }
        public PublicationChannelArgs Channel { get; }

    }

    public class PublicationChannelArgs
    {

    }

    //public class PublishResult
    //{
    //    public DateTime PublishedAt { get; set; }
    //    public NotificationFilepaths NotificationFilepaths { get; set; } //?? 
    //}
}
