using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MyNotifier.NotifierSystemInitializer;

namespace MyNotifier
{

    public abstract class NotifierSystemInitializer : INotifierSystemInitializer
    {


        public virtual async ValueTask<IResult> InitializeSystemAsync() => await this.InitializeSystemCoreAsync().ConfigureAwait(false);

        protected abstract ValueTask<IResult> InitializeSystemCoreAsync();

        public abstract class Args { }

        public interface IConfiguration : IApplicationConfigurationWrapper { }

        public class Configuration : ApplicationConfigurationWrapper, IApplicationConfigurationWrapper
        {

            public Configuration(IApplicationConfiguration applicationConfiguration) : base(applicationConfiguration) { }
        }

        public interface IResult : ICallResult
        {
            Catalog Catalog { get; }
            InterestModel[] InterestModels { get; }
            ServiceProvider ServiceProvider { get; set; }
        }

        public class Result : CallResult, IResult
        {
            public Catalog Catalog { get; set; }
            public InterestModel[] InterestModels { get; set; }
            public ServiceProvider ServiceProvider { get; set; }

            public Result() { }
            public Result(Catalog catalog, InterestModel[] interests) : base()
            {
                Catalog = catalog;
                InterestModels = interests;
            }
            public Result(bool success, string errorText) : base(success, errorText) { }

            public static Result FromFailedCallResult(ICallResult failedResult) => new(false, failedResult.ErrorText);
        }

    }

    public abstract class NotifierSystemInitializer<TArgs> : NotifierSystemInitializer
        where TArgs : Args
    {



        public virtual async ValueTask<IResult> InitializeSystemAsync(TArgs args) => await InitializeSystemCoreAsync(args).ConfigureAwait(false);


        protected abstract ValueTask<IResult> InitializeSystemCoreAsync(TArgs args);

    }


    //public interface ISystemInitializer
    //{
    //    ValueTask InitializeSystemAsync();
    //}

    public interface INotifierSystemInitializer //: ISystemInitializer
    {
        ValueTask<IResult> InitializeSystemAsync();
    }

    public interface INotifierSystemInitializer<TArgs> : INotifierSystemInitializer
    {
        ValueTask<IResult> InitializeSystemAsync(TArgs args);
    }

    //public interface INotifierSystemInitializer<TArgs>
    //    where TArgs : NotifierSystemInitializer.Args
    //{ }
    //public interface INotifierSystemInitializer : INotifierSystemInitializer<NotifierSystemInitializer.Args> 
    //{ }
}
