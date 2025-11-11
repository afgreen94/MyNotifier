using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Interests
{
    public interface IManager
    {
        //ICallResult<InterestDescription> AddStartInterests(params IInterest[] interests);
        //ICallResult StopRemoveInterest(InterestDescription interestDescription);

        //void Subscribe();
        //void Unsubscribe();
    }

    public class InterestDescription : Base.Definition
    {
        InterestDefinition Definition { get; set; }
    }
}
