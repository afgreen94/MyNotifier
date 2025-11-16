using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IInterestFactory = MyNotifier.Contracts.Interests.IFactory;
using IUpdateSubscriber = MyNotifier.Contracts.Updaters.ISubscriber;

namespace MyNotifier.Contracts.Interests
{
    public interface IManager
    {

        //expose readonly collection of active interests?   

        ICallResult AddStartInterest(IInterest interest);

        //expose factory, have caller create interests then pass to manager's addStart methods, rather than getAddStart? 
        //IInterestFactory InterestFactory { get; }

        Task<ICallResult> GetAddStartInterestAsync(Guid interestId);
        Task<ICallResult> GetAddStartInterestAsync(InterestModel interest);

        ICallResult StopRemoveInterest(Guid interestId, TimeSpan taskKillWaitTimeoutOverride = default);

        void RegisterUpdateSubscriber(IUpdateSubscriber subscriber);

        //ICallResult<InterestDescription> AddStartInterests(params IInterest[] interests);
        //ICallResult StopRemoveInterest(InterestDescription interestDescription);

        //void Subscribe();
        //void Unsubscribe();

        IControllable Controllable { get; }
    }

    public class InterestDescription : Base.Definition
    {
        InterestDefinition Definition { get; set; }
    }
}
