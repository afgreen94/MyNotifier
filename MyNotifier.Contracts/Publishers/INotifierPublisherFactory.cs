using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Publishers
{
    public interface INotifierPublisherFactory
    {
        INotifierPublisher GetNotifierPublisher();

        //INotifierPublisher GetNotifierPublisher(object arg);
    }
}
