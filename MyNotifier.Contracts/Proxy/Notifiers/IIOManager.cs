using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Proxy.Notifiers
{
    public interface IIOManager
    {
        Task<ICallResult<bool>> NotificationDirectoryExistsAsync();  //Assert ? //Ensure? create if does not exist ? 

        Task<ICallResult<NotificationHeader[]>> RetrieveNotificationHeadersAsync();

        Task<ICallResult<Notification>> ReadInNotificationAsync(NotificationHeader header);
    }


    public class FolderObjectDescBase<T> 
        where T : NotificationDescription
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public T Description { get; set; }
    }

    public class FolderObjectDesc : FolderObjectDescBase<NotificationDescription>
    {

    }

    public class UpdateFolderObjectDesc : FolderObjectDescBase<UpdateNotificationDescription>
    {

    }
}
