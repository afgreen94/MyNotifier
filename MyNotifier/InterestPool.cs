using MyNotifier.CommandAndControl;
using MyNotifier.CommandAndControl.Commands;
using MyNotifier.Contracts.CommandAndControl;
using MyNotifier.Contracts.CommandAndControl.Commands;
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
using MyNotifier.Contracts.Base;

namespace MyNotifier
{
    //concurrent interest pool 
    public partial class InterestPool
    {

        private readonly SemaphoreSlim accessSemaphore = new(1, 1);
        private readonly IDictionary<Guid, IInterest> innerDictionary = new Dictionary<Guid, IInterest>();

        public Contracts.Base.IDefinition Definition => throw new NotImplementedException();

        public bool Contains(Guid interestId) => this.ContainsCore(interestId);

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

        //public void Update() { }
        //public async Task UpdateAsync() { }
        //public bool TryUpdate() { }
        //public async Task<bool> TryUpdateAsync() { }
        //TryGetValue(Id)

        protected bool ContainsCore(Guid interestId) => this.innerDictionary.ContainsKey(interestId);


        protected void AddCore(IInterest interest, bool forceOverwrite = false)
        {

        }

        protected async Task AddCoreAsync(IInterest interest, bool forceOverwrite = false)
        {

        }

        protected void AddCore(IInterest[] interests, bool forceOverwrite = false)
        {

        }

        protected async Task AddCoreAsync(IInterest[] interests, bool forceOverwrite = false)
        {

        }

        protected bool TryAddCore(IInterest interest, bool forceOverwrite = false)
        {
            if (this.innerDictionary.ContainsKey(interest.Definition.Id) && !forceOverwrite) return false;

            this.innerDictionary[interest.Definition.Id] = interest;

            return true;
        }

        protected async Task<bool> TryAddCoreAsync(IInterest interest, bool forceOverwrite = false)
        {
            throw new NotImplementedException();
        }

        protected bool TryAddCore(IInterest[] interests, bool forceOverwrite = false)
        {
            throw new NotImplementedException();
        }

        protected async Task<bool> TryAddCoreAsync(IInterest[] interests, bool forceOverwrite = false)
        {
            throw new NotImplementedException();
        }


        protected void RemoveCore(Guid interestId, bool suppressException = false)
        {

        }

        protected async Task RemoveCoreAsync(Guid interestId, bool suppressException = false)
        {

        }

        protected void RemoveCore(Guid[] interestIds, bool suppressException = false)
        {

        }

        protected async Task RemoveCoreAsync(Guid[] interestIds, bool suppressException = false)
        {

        }


        protected bool TryRemoveCore(Guid interestId)
        {
            if (!this.innerDictionary.ContainsKey(interestId)) return false;

            this.innerDictionary.Remove(interestId);

            return true;
        }

        protected bool TryRemoveCore(Guid[] interestIds)
        {
            throw new NotImplementedException();
        }
    }
}
