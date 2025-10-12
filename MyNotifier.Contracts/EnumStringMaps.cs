using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Proxy;

namespace MyNotifier.Contracts
{
    /*
        NotificationType.Update => "UPDATE",
        NotificationType.Command => "COMMAND",
        NotificationType.Exception => "EXCEPTION",
         */

    //should be built dynamically by initializer using exposed properties of relevant classes, this way additions can be centralized in their respective classes. eventually use attribute for enum <-> string conversion 
    //also...can do this with attributes rather than map class //NEED TO HANDLE THIS !!! 
    public class EnumStringMaps
    {
        private static readonly IReadOnlyDictionary<SystemScheme, string> systemSchemeToString = new Dictionary<SystemScheme, string>() { { SystemScheme.ProxyFileIOServer, "PROXYFILEIOSERVER" }, { SystemScheme.DirectToClient, "DIRECTTOCLIENT" } };
        private static readonly IReadOnlyDictionary<string, SystemScheme> stringToSystemScheme = new Dictionary<string, SystemScheme>() { { "PROXYFILEIOSERVER", SystemScheme.ProxyFileIOServer }, { "DIRECTTOCLIENT", SystemScheme.DirectToClient } };
        private static readonly IReadOnlyDictionary<NotificationType, string> notificationTypeToString = new Dictionary<NotificationType, string>() { { NotificationType.Update, "UPDATE" }, { NotificationType.Command, "COMMAND" }, { NotificationType.CommandResult, "COMMANDRESULT" }, { NotificationType.Exception, "EXCEPTION" } };
        private static readonly IReadOnlyDictionary<string, NotificationType> stringToNotificationType = new Dictionary<string, NotificationType>() { { "UPDATE", NotificationType.Update }, { "COMMAND", NotificationType.Command }, { "COMMANDRESULT", NotificationType.CommandResult }, { "EXCEPTION", NotificationType.Exception } };
        private static readonly IReadOnlyDictionary<NotificationFileType, string> notificationFileTypeToString = new Dictionary<NotificationFileType, string>() { { NotificationFileType.Metadata, "METADATA" }, { NotificationFileType.Data, "DATA" } };
        private static readonly IReadOnlyDictionary<string, NotificationFileType> stringToNotificationFileType = new Dictionary<string, NotificationFileType>() { { "METADATA", NotificationFileType.Metadata }, { "DATA", NotificationFileType.Data } };

        public static IReadOnlyDictionary<SystemScheme, string> SystemSchemeToString => systemSchemeToString;
        public static IReadOnlyDictionary<string, SystemScheme> StringToSystemScheme => stringToSystemScheme;
        public static IReadOnlyDictionary<NotificationType, string> NotificationTypeToString => notificationTypeToString;
        public static IReadOnlyDictionary<string, NotificationType> StringToNotificationType => stringToNotificationType;
        public static IReadOnlyDictionary<NotificationFileType, string> NotificationFileTypeToString => notificationFileTypeToString;
        public static IReadOnlyDictionary<string, NotificationFileType> StringToNotificationFileType => stringToNotificationFileType;

        public static string GetString(SystemScheme systemScheme) => systemSchemeToString[systemScheme];
        public static SystemScheme GetSystemScheme(string str) => stringToSystemScheme[str];
        public static string GetString(NotificationType type) => notificationTypeToString[type];
        public static NotificationType GetNotificationType(string str) => stringToNotificationType[str.ToUpper()];
        public static string GetString(NotificationFileType type) => notificationFileTypeToString[type];
        public static NotificationFileType GetNotificationFileType(string str) => stringToNotificationFileType[str.ToUpper()];
        public static string GetString(NotificationDataType type) => throw new NotImplementedException();
        public static NotificationDataType GetNotificationDataType(string str) => throw new NotImplementedException();
    }
}
