using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Notifications
{
    public class NotificationDefinition : Definition //interestId/updaterId are specific to eventUpdate-type notifications, meaningless with commands/exceptions, etc...
    {
        public Guid InterestId { get; set; }
        public Guid UpdaterId { get; set; }
        public Definition InterestDefinition { get; set; }
        public Definition UpdaterDefinition { get; set; }
        //public Definition[] TypeHierarchy { get; set; } add hierarchy later 
    }

    public class NotificationMetadata
    {
        public NotificationDefinition Definition { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime PublishedAt { get; set; } 
        //public object PublishedTo { get; set; } //maybe will eventually need this. later versions, publish location/publisher type will be configurable 
        public double SizeBytes { get; set; }
        public bool Encrypted { get; set; }
        public TypeArgs TypeArgs { get; set; }
    }

    public class TypeArgs
    {
        public NotificationType NotificationType { get; set; }
        public DataTypeArgs NotificationDataTypeArgs { get; set; }
    }

    public abstract class DataTypeArgsBase
    {
        public abstract NotificationDataType DataType { get; }
        public abstract string Description { get; }
    }
    public class DataTypeArgs
    {
        public NotificationDataType DataType { get; set; }
        public string Description { get; set; }
    }

    public class GenericNotificationDataType
    {
        public NotificationDataType DataType { get; set; }
        public string Description { get; set; }
    }

    public class StringNotificationDataType
    {
        public NotificationDataType DataType => NotificationDataType.String_Generic;
        public string Description { get; set; }
        public Encoding Encoding { get; set; }
    }

    public interface INotification
    {
        NotificationMetadata Metadata { get; }
        byte[] Data { get; }
    }
    public class Notification : INotification
    {
        public NotificationMetadata Metadata { get; set; }
        public byte[] Data { get; set; }
    }

    public enum NotificationType
    {
        Update = 0b00000001, //event update 
        Command = 0b00000010, //client -> server command 
        CommandResult = 0b00000100, //server -> client command result 
        Exception = 0b00001000 //server exception
    }
    public enum NotificationDataType
    {
        String_Generic, //text //link? //specificity? //string type could be given in TypeArgs description field (eg json in utf8, link in utf8, etc...)
        String_Json,
        String_Link, //specify datatype linked to ? maybe have composite enum for this ... 
        Image,
        Audio,
        Video
        //more ? 
    }

    public enum Priority //practically will not make difference, at least at early scale. eventually, if system handles high notification volume, will qualify notifications 
    {
        Highest = 5,
        High = 4,
        Normal = 3,
        Low = 2,
        Lowest = 1
    }
}
