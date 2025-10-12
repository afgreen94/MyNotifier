using MyNotifier.Contracts.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Proxy
{
    public interface INotificationFileSystemObjectTranslator
    {
        string ToFolderName(NotificationFolderObjectDescription description);
        string ToFolderName(NotificationDescription description);

        NotificationFolderObjectDescription ToFolderObjectDescription(string path, char directorySeparator);
        NotificationDescription ToNotificationDescription(string path, char directorySeparator);

        string ToFileName(NotificationFileObjectDescription description);
        string ToFileName(NotificationDescription description);

        NotificationFileObjectDescription ToFileObjectDescription(string path, char directorySeparator);
        NotificationDescription ToNotificationComponentDescription(string path, char directorySeparator);
    }
}
