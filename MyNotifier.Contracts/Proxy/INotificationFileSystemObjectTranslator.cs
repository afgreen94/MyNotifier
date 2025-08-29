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
        NotificationFolderObjectDescription ToFolderObjectDescription(string path, char directorySeparator);
        string ToFileName(NotificationFileObjectDescription description);
        NotificationFileObjectDescription ToFileObjectDescription(string path, char directorySeparator);
    }
}
