//using MyNotifier.Contracts;
//using MyNotifier.Contracts.Proxy;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using MyNotifier.Contracts.Notifications;

//namespace MyNotifier
//{
//    //could make configurable. builds and applies formatting through config 
//    public class DefaultTranslator : INotificationFileSystemObjectTranslator //basically hard-encodes notification folder formatting convention, provides logic for translating between. Could encapsulate formatting separately  //public for debug 
//    {
//        private const string FolderNameFormatPrefixString = "{UPDATE_TICKS}_{NOTIFICATION_TYPE}";
//        private const string FolderNameUpdateNotificationFormatSuffixString = "{INTEREST_ID}_{UPDATER_ID}";
//        //private const string FolderNameFormatString = "{UPDATE_TICKS}_{NOTIFICATION_TYPE}_{INTEREST_ID}_{UPDATER_ID}"; //could come from config, but tightly coupled with translation logic 
//        private const char PropertyDelimiter = '_';

//        public string ToFolderName(NotificationFolderObjectDescription description)
//        {
//            var prefix = FolderNameFormatPrefixString.Replace("{UPDATE_TICKS}", description.Ticks.ToString())
//                                                     .Replace("{NOTIFICATION_TYPE}", EnumStringMaps.GetString(description.Type));

//            if (description.Type != NotificationType.Update) return prefix;

//            var suffix = FolderNameUpdateNotificationFormatSuffixString.Replace("{INTEREST_ID}", description.InterestId.ToString())
//                                                                       .Replace("{UPDATER_ID}", description.UpdaterId.ToString());

//            return $"{prefix}{PropertyDelimiter}{suffix}"; //new StringBuilder(prefix).Append(suffix).ToString();
//        }

//        public NotificationFolderObjectDescription ToFolderObjectDescription(string path, char directorySeparator)
//        {
//            var notificationDirectoryParts = path.Split(directorySeparator);
//            var notificationDirectoryName = notificationDirectoryParts[^1];
//            var notificationDirectoryNameParts = notificationDirectoryName.Split(PropertyDelimiter);

//            return new()
//            {
//                Path = path,
//                Name = notificationDirectoryName,
//                Ticks = long.Parse(notificationDirectoryNameParts[0]),
//                Type = EnumStringMaps.GetNotificationType(notificationDirectoryNameParts[1]),
//                InterestId = Guid.Parse(notificationDirectoryNameParts[2]),
//                UpdaterId = Guid.Parse(notificationDirectoryNameParts[3]),
//            };
//        }

//        //default
//        //{NotificationId}_{NotificationType}_{Ticks}

//        public NotificationDescription ToNotificationDescription(string path, char directorySeparator)
//        {
//            var notificationDirectoryParts = path.Split(directorySeparator);
//            var notificationDirectoryName = notificationDirectoryParts[^1];
//            var notificationDirectoryNameParts = notificationDirectoryName.Split(PropertyDelimiter);

//            return new()
//            {
//                Id = Guid.Parse(notificationDirectoryNameParts[0]),
//                Type = EnumStringMaps.GetNotificationType(notificationDirectoryNameParts[1]),
//                Ticks = long.Parse(notificationDirectoryNameParts[2])
//            };
//        }

//        public string ToFileName(NotificationFileObjectDescription description) => EnumStringMaps.GetString(description.FileType);

//        public NotificationFileObjectDescription ToFileObjectDescription(string path, char directorySeparator)
//        {
//            var notificationFileName = path.Split(directorySeparator)[^1];
//            //var notificationFileNameParts = notificationFileName.Split(PropertyDelimiter);

//            return new()
//            {
//                Path = path,
//                Name = notificationFileName,
//                FileType = EnumStringMaps.GetNotificationFileType(notificationFileName)
//            };
//        }

//        public string ToFolderName(NotificationDescription description)
//        {
//            throw new NotImplementedException();
//        }

//        public string ToFileName(NotificationDescription description)
//        {
//            throw new NotImplementedException();
//        }

//        public NotificationDescription ToNotificationComponentDescription(string path, char directorySeparator)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
