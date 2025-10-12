using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Notifications
{
    //public class NotificationDefinition : Definition //interestId/updaterId are specific to eventUpdate-type notifications, meaningless with commands/exceptions, etc...
    //{
    //    public Guid InterestId { get; set; }
    //    public Guid EventModuleId { get; set; }
    //    public Guid UpdaterId { get; set; }
    //    //public Definition InterestDefinition { get; set; }

    //    //public Definition UpdaterDefinition { get; set; }
    //    //public Definition[] TypeHierarchy { get; set; } add hierarchy later 
    //}

    //public class UpdateNotificationDefinition : NotificationDefinition
    //{
    //    public Guid InterestDefinitionId { get; set; }
    //    public Guid EventModuleDefinitionId { get; set; }
    //    public Guid UpdaterDefinitionId { get; set; }
    //}

    public class NotificationHeader
    {
        public Guid Id { get; set; }
        public long Ticks { get; set; } //publishedAt ticks
        public NotificationType Type { get; set; }
    }

    public class NotificationDescription
    {
        public NotificationHeader Header { get; set; }
        public DateTime PublishedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public object PublishedTo { get; set; } //maybe will eventually need this. later versions, publish location/publisher type will be configurable 
    }

    public class UpdateNotificationDescription : NotificationDescription
    {
        public Guid InterestDefinitionId { get; set; }
        public Guid InterestId { get; set; }

        public Guid EventModuleDefinitionId { get; set; }
        public Guid EventModuleId { get; set; }

        public Guid UpdaterDefinitionId { get; set; }
        public Guid UpdaterId { get; set; }
    }

    public class CommandResultNotificationDescription : NotificationDescription
    {
        public Guid CommandNotificationId { get; set; }
    }

    public class NotificationMetadata
    {
        public NotificationDescription Description { get; set; }
        public DataTypeArgs DataTypeArgs { get; set; }
        public double SizeBytes { get; set; }
        public bool Encrypted { get; set; }
    }

    public class NotificationDescriptionModel
    {
        public Guid Id { get; set; }

        //missing notification type !!! will probably need custom converter to do this properly 
        public DateTime UpdatedAt { get; set; }
        public DateTime PublishedAt { get; set; }
        public object PublishedTo { get; set; } //maybe will eventually need this. later versions, publish location/publisher type will be configurable 

        //Update
        public Guid InterestDefinitionId { get; set; }
        public Guid InterestId { get; set; }
        public Guid EventModuleDefinitionId { get; set; }
        public Guid EventModuleId { get; set; }
        public Guid UpdaterDefinitionId { get; set; }
        public Guid UpdaterId { get; set; }

        //CommandResult 
        public Guid CommandNotificationId { get; set; }
    }

    public class NotificationMetadataModel
    {
        public NotificationDescriptionModel Description { get; set; }
        //public DateTime UpdatedAt { get; set; }
        //public DateTime PublishedAt { get; set; }
        //public object PublishedTo { get; set; } //maybe will eventually need this. later versions, publish location/publisher type will be configurable 
        //public NotificationType NotificationType { get; set; }
        public DataTypeArgsModel DataTypeArgs { get; set; }
        public double SizeBytes { get; set; }
        public LargeDataDownloadOptions LargeDataDownloadOptions { get; set; }
        public bool Encrypted { get; set; }
    }

    //public class NotificationMetadata : NotificationMetadata<NotificationDescription> { }
    //public class UpdateNotificationMetadata : NotificationMetadata<UpdateNotificationDescription> { }
    //public class CommandResultNotificationMetadata : NotificationMetadata<CommandResultNotificationDescription> { }

    //public class NotificationMetadata
    //{
    //    public NotificationDescription Description { get; set; }
    //    public DateTime UpdatedAt { get; set; }
    //    public DateTime PublishedAt { get; set; } 
    //    public object PublishedTo { get; set; } //maybe will eventually need this. later versions, publish location/publisher type will be configurable 
    //    public double SizeBytes { get; set; }
    //    public LargeDataDownloadOptions LargeDataDownloadOptions { get; set; }
    //    public bool Encrypted { get; set; }
    //    public NotificationType NotificationType { get; set; }
    //    public DataTypeArgs DataTypeArgs { get; set; }

    //}

    public class LargeDataDownloadOptions
    {
        //large file size definition
        //stream or download 
        //dispose ? 
    }

    //public class LargeDataNotificationMetadata : NotificationMetadata
    //{
    //    public LargeDataDownloadOptions LargeDataFileDownloadOptions { get; set; }
    //}

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

    public class DataTypeArgsModel
    {
        public string DataType { get; set; }
        public string Description { get; set; }
    }

    //public class GenericNotificationDataType
    //{
    //    public NotificationDataType DataType { get; set; }
    //    public string Description { get; set; }
    //}

    //public class StringNotificationDataType
    //{
    //    public NotificationDataType DataType => NotificationDataType.String_Generic;
    //    public string Description { get; set; }
    //    public Encoding Encoding { get; set; }
    //}

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
