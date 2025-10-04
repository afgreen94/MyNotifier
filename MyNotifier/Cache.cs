using MyNotifier.Contracts;
using MyNotifier.Contracts.EventModules;
using MyNotifier.Contracts.Interests;
using MyNotifier.Contracts.Updaters;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier
{
    public class Cache : ICache
    {
        private readonly SemaphoreSlim writeSemaphore = new(1, 1);
        private readonly IDictionary<Type, ConcurrentCacheCore> innerCache = new Dictionary<Type, ConcurrentCacheCore>();

        public bool TryGetValue(Guid id, out IInterest value) => this.TryGetValue<IInterest>(id, out value);
        public void Add(Guid id, IInterest value) => this.Add<IInterest>(id, value);

        public bool TryGetValue(Guid id, out IEventModule value) => this.TryGetValue<IEventModule>(id, out value);
        public void Add(Guid id, IEventModule value) => this.Add<IEventModule>(id, value);

        public bool TryGetValue(Guid id, out IEventModuleDefinition value) => this.TryGetValue<IEventModuleDefinition>(id, out value);
        public void Add(Guid id, IEventModuleDefinition value) => this.Add<IEventModuleDefinition>(id, value);

        public bool TryGetValue(Guid id, out IUpdater value) => this.TryGetValue<IUpdater>(id, out value);
        public void Add(Guid id, IUpdater value) => this.Add<IUpdater>(id, value);
        public bool TryGetValue(Guid id, out IUpdaterDefinition value) => this.TryGetValue<IUpdaterDefinition>(id, out value);
        public void Add(Guid id, IUpdaterDefinition value) => this.Add<IUpdaterDefinition>(id, value);


        private bool TryGetValue<T>(Guid id, out T value)
            where T : class
        {
            value = default;

            var coreExists = this.TryGetCacheCore<T>(out var core);
            if (!coreExists) return false; //throw on null?

            var cached = core.TryGetValue(id, out var cachedValue);
            if (!cached ||
                cachedValue == null ||
                cachedValue is not T cachedTypedValue) return false; //throw on null?

            value = cachedTypedValue;
            return true;
        }
        private void Add<T>(Guid id, T value)
            where T : class
        {
            var coreExists = this.TryGetCacheCore<T>(out var core);

            if (!coreExists)
            {
                try
                {
                    this.writeSemaphore.Wait();
                    core = new ConcurrentCacheCore<T>();
                    this.innerCache.Add(typeof(T), core);
                }
                finally { this.writeSemaphore.Release(); }
            }

            core.Add(id, value);
        }
        private bool TryGetCacheCore(Type t, out ConcurrentCacheCore core)
        {
            var coreExists = this.innerCache.TryGetValue(t, out core);
            if (!coreExists || core == null) return false; //throw on null ? 

            return true;
        }

        private bool TryGetCacheCore<T>(out ConcurrentCacheCore<T> core) where T : class
        {
            core = default;

            var coreExists = TryGetCacheCore(typeof(T), out var cachedCore);
            if (!coreExists ||
                cachedCore == null ||
                cachedCore is not ConcurrentCacheCore<T> cachedTypedCore) return false;

            core = cachedTypedCore;
            return true;
        }


        private class ConcurrentCacheCore
        {
            private readonly SemaphoreSlim cacheWriteSemaphore = new(1, 1);
            private readonly IDictionary<Guid, object> innerCache = new Dictionary<Guid, object>();

            public bool TryGetValue<T>(Guid id, out T value) where T : class
            {
                value = default;

                var cached = this.innerCache.TryGetValue(id, out var cachedValue); //should throw in case of value == null/default ?
                if (!cached ||
                    cachedValue == null ||
                    cachedValue is not T cachedTypedValue) return false;

                value = cachedTypedValue;
                return true;
            }
            public void Add<T>(Guid id, T value) where T : class
            {
                try
                {
                    this.cacheWriteSemaphore.Wait();
                    this.innerCache.Add(id, value);
                }
                finally { this.cacheWriteSemaphore.Release(); }
            }
        }

        private class ConcurrentCacheCore<T> : ConcurrentCacheCore where T : class 
        {
            public bool TryGetValue(Guid id, out T value) => base.TryGetValue<T>(id, out value);
            public void Add(Guid id, T value) => base.Add(id, value);
        }
    }


    public interface ICache : Contracts.Interests.ICache, Contracts.EventModules.ICache, Contracts.Updaters.ICache { }
}
