using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Interests
{
    public interface IInterestProvider
    {
        ValueTask<ICallResult> GetInterestAsync(Guid interestId);
    }
}
