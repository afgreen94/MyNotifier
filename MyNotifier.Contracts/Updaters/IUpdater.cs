using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.Updaters;
using MyNotifier.Contracts.Notifications;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;

namespace MyNotifier.Contracts.Updaters
{
    public interface IUpdater
    {
        IUpdaterDefinition Definition { get; }
        ValueTask<ICallResult> InitializeAsync(bool forceReinitialize = false);
        ValueTask<IUpdaterResult> TryGetUpdateAsync(Parameter[] parameters);
    }
    public interface IUpdaterResult : ICallResult
    {
        bool UpdateAvailable { get; }
        DateTime UpdatedAt { get; }
        DataTypeArgs TypeArgs { get; }
        byte[] Data { get; }
    }
    public interface IUpdaterArgs { object FactoryArgs { get; } object Arg { get; } }
}
