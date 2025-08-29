using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts
{


    //public interface IFactory<TModel, TResult>
    //{
    //    ValueTask<TResult> GetAsync(TModel model);
    //    ValueTask<TResult> GetAsync(Guid id, ParameterValue[] parameterValues);
    //    ValueTask<TResult> GetAsync(string hash);
    //}    


    //public interface IFactoryCache : IInterestCache, IEventModuleCache, IUpdaterCache
    //{

    //}


    //public interface IInterestParameterValues
    //{
    //    IReadOnlyDictionary<Guid, IEventModuleParameterValues> EventModuleParameters { get; }

    //}





    public class InterestParameters
    {
        IReadOnlyDictionary<Guid, EventModuleParameters> EventModuleParameters { get; set; }
    }

    public class EventModuleParameters
    {
        IReadOnlyDictionary<Guid, Parameter[]> UpdaterParameters { get; set; }
    }
}
