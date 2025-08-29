using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Interests
{
    public interface IInterestCache
    {
        bool TryGetValue(Guid interestId, out IInterest interest);
        void Add(IInterest interest);
    }
}
