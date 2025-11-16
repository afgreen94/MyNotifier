using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts
{
    public interface ISubscriber
    {
        Guid Id { get; }
    }
}
