using MyNotifier.Commands;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Notifiers;
using static MyNotifier.Driver;
using MyNotifier.Contracts.Interests;

namespace MyNotifier
{
    public class InterestPool //concurrent interest pool 
    {

        private readonly SemaphoreSlim accessSemaphore = new(1, 1);
        private readonly IDictionary<Guid, IInterest> innerDictionary = new Dictionary<Guid, IInterest>();

        public bool Contains(Guid interestId) => this.innerDictionary.ContainsKey(interestId);

        public bool TryAdd(IInterest interest, bool forceOverwrite = false)
        {
            try
            {
                this.accessSemaphore.Wait();

                return this.TryAddCore(interest, forceOverwrite);
            }
            finally { this.accessSemaphore.Release(); }
        }

        public async Task<bool> TryAddAsync(IInterest interest, bool forceOverwrite = false)
        {
            try
            {
                await this.accessSemaphore.WaitAsync().ConfigureAwait(false);

                return this.TryAddCore(interest, forceOverwrite);
            }
            finally { this.accessSemaphore.Release(); }
        }

        public bool TryRemove(Guid interestId)
        {
            try
            {
                this.accessSemaphore.Wait();

                return this.TryRemoveCore(interestId);
            }
            finally { this.accessSemaphore.Release(); }
        }

        public async Task<bool> TryRemoveAsync(Guid interestId)
        {
            try
            {
                await this.accessSemaphore.WaitAsync().ConfigureAwait(false);

                return this.TryRemoveCore(interestId);
            }
            finally { this.accessSemaphore.Release(); }
        }

        private bool TryAddCore(IInterest interest, bool forceOverwrite = false)
        {
            if (this.innerDictionary.ContainsKey(interest.Definition.Id) && !forceOverwrite) return false;

            this.innerDictionary[interest.Definition.Id] = interest;

            return true;
        }

        private bool TryRemoveCore(Guid interestId)
        {
            if (!this.innerDictionary.ContainsKey(interestId)) return false;

            this.innerDictionary.Remove(interestId);

            return true;
        }

        public class Controller : IControllable<RegisterAndSubscribeToNewInterests>, IControllable<SubscribeToInterestsByDefinitionIds>, IControllable<UnsubscribeFromInterests>
        {

            private readonly InterestPool interestPool;
            private readonly IInterestFactory interestFactory;

            public Controller(IInterestFactory interestFactory) { this.interestFactory = interestFactory; }

            public void OnCommand(RegisterAndSubscribeToNewInterests command)
            {
                var interestModels = command.InterestModels;

                foreach (var interestModel in interestModels)
                {
                    if (this.interestPool.Contains(interestModel.Definition.Id)) { continue; } //what do ?

                    IInterest interest = default;
                    
                    //var interest = await this.interestFactory.GetInterestAsync(interestModel).ConfigureAwait(false); //make callback async 

                    var added = this.interestPool.TryAdd(interest);
                }
            }

            public void OnCommand(UnsubscribeFromInterests command)
            {
                var interestModels = command.InterestModels;

                foreach (var interestModel in interestModels)
                {
                    if (!this.interestPool.Contains(interestModel.Definition.Id)) { continue; } //what do ?

                    var removed = this.interestPool.TryRemove(interestModel.Definition.Id);
                }
            }

            public void OnCommand(SubscribeToInterestsByDefinitionIds command)
            {
                throw new NotImplementedException();
            }
        }
    }
}
