using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts
{
    public interface ICache<T>
    {
        bool TryGetValue(Guid id, out T value);
        void Add(Guid id, T value);
    }
}
