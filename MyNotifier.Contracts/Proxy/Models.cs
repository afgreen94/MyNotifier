using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.Notifications;

namespace MyNotifier.Contracts.Proxy
{
    //File System Objects 

    //folder name scheme 
    // {TICKS}_{NOTIFICATION_TYPE}_{INTEREST_ID}_{UPDATER_ID}_{INSTANCE_NUM}

    //file name scheme 
    // [metadata|data]

    //EXCEPTIONS AND COMMANDS WILL FOLLOW A DIFFERENT FORMAT! MUST ACCOUNT FOR THIS!

    public class FileSystemObjectDescription
    {
        public string Path { get; set; } //should be made redundant 
        public string Name { get; set; }
    }

    public class NotificationFolderObjectDescription : FileSystemObjectDescription  //maybe should expose parseType() methods, or encapsulate somewhere 
    {
        public long Ticks { get; set; }
        public NotificationType Type { get; set; }
        public Guid InterestId { get; set; }
        public Guid UpdaterId { get; set; }

        //public static NotificationFolderObjectDescription FromPath(string path, char directorySeparator, char propertyDelimiter)
        //{
        //    var notificationDirectoryParts = path.Split(directorySeparator);
        //    var notificationDirectoryName = notificationDirectoryParts[^1];
        //    var notificationDirectoryNameParts = notificationDirectoryName.Split(propertyDelimiter);

        //    return new()
        //    {
        //        Path = path,
        //        Name = notificationDirectoryName,
        //        Ticks = long.Parse(notificationDirectoryNameParts[0]),
        //        Type = EnumStringMaps.GetNotificationType(notificationDirectoryNameParts[1]),
        //        InterestId = Guid.Parse(notificationDirectoryNameParts[2]),
        //        UpdaterId = Guid.Parse(notificationDirectoryNameParts[3]),
        //        InstanceId = long.Parse(notificationDirectoryNameParts[4])
        //    };
        //}
    }

    public class NotificationFileObjectDescription : FileSystemObjectDescription
    {
        public NotificationFileType FileType { get; set; }

        //public static NotificationFileObjectDescription FromPath(string path, char directorySeparator, char propertyDelimiter)
        //{
        //    var notificationFileName = path.Split(directorySeparator)[^1];
        //    var notificationFileNameParts = notificationFileName.Split(propertyDelimiter);

        //    return new()
        //    {
        //        Path = path,
        //        Name = notificationFileName,
        //        FileType = EnumStringMaps.GetNotificationFileType(notificationFileNameParts[2])
        //    };
        //}
    }

    public class NotificationFileObject
    {
        public NotificationFileObjectDescription Description { get; set; }
        public NotificationMetadata NotificationMetadata { get; set; }
        public byte[] Data { get; set; }
    }

    public class NotificationFolderObject
    {
        public NotificationFolderObjectDescription Description { get; set; }
        public NotificationFileObject MetadataFileObject { get; set; }
        public NotificationFileObject DataFileObject { get; set; }
    }

    public class NotificationFilepaths
    {
        public string NotificationFilepath { get; set; }
        public string NotificationMetadataFilepath { get; set; }
    }

    public enum NotificationFileType //data, metadata could eventually be combined 
    {
        Metadata,
        Data
    }
}
