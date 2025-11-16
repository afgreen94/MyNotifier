using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier
{

    #region Background Task Manager
    public interface IBackgroundTaskManager
    {
        //ValueTask<ICallResult> InitializeAsync(ApplicationForeground.MessageQueue foregroundMessageQueue, bool forceReinitialize = false); //foreground messagequeue or foreground or foregound handlers ? //see note in implementation!
       
        //how to make visible to only foreground ??
        //have constructor receiving either handlers or message queue ?
        ValueTask<ICallResult> InitializeAsync(ApplicationForeground.ITaskCompleteSubscriber taskCompleteSubscriber, ApplicationForeground.IFailureSubscriber failureSubscriber, bool forceReinitialize = false);

        //bool TryGetTask(Guid taskId, out BackgroundTaskWrapper task);

        void AddTask(Func<ValueTask> taskSource);
        void StartTask(Guid taskId);
        void KillTask(Guid taskId);
        void Wait(Guid taskId);
        void Wait(Guid taskId, TimeSpan timeout);
        Task WaitAsync(Guid taskId);
        Task WaitAsync(Guid taskId, TimeSpan timeout);
        void AddStartTask();
        void KillTaskWait(Guid taskId);
        void KillTaskWait(Guid taskId, TimeSpan timeout);
        Task KillTaskWaitAsync(Guid taskId);
        Task KillTaskWaitAsync(Guid taskId, TimeSpan timeout);
    }

    //may use backgroundTaskManager as static class for now, eventually have instance class in call context, want initialization to only be visible in ApplicationForeground layer
    public class BackgroundTaskManager : IBackgroundTaskManager
    {

        private readonly IConfiguration configuration;

        //private ApplicationForeground.MessageQueue foregroundMessageQueue; //maybe use in different design 
        private ApplicationForeground.ITaskCompleteSubscriber foregroundTaskCompleteSubscriber;
        private ApplicationForeground.IFailureSubscriber foregroundFailureSubscriber;

        private IDictionary<Guid, BackgroundTaskKillFlagWrapper> tasks = new Dictionary<Guid, BackgroundTaskKillFlagWrapper>();

        private bool isInitialized = false;

        public BackgroundTaskManager(IConfiguration configuration) { this.configuration = configuration; }


        //Could use direct reference to message queue, seems appropriate for application-generic layer. for now, using foreground handlers, could change later, probably more elegant 
        //public ValueTask<ICallResult> InitializeAsync(ApplicationForeground.MessageQueue foregroundMessageQueue, bool forceReinitialize = false)
        //{
        //    try
        //    {
        //        if(!this.isInitialized || forceReinitialize)
        //        {
        //            this.foregroundMessageQueue = foregroundMessageQueue;
        //            this.isInitialized = true;
        //        }

        //        return new ValueTask<ICallResult>(new CallResult());
        //    }
        //    catch(Exception ex) { return new ValueTask<ICallResult>(CallResult.FromException(ex)); }
        //}

        public ValueTask<ICallResult> InitializeAsync(ApplicationForeground.ITaskCompleteSubscriber foregroundTaskCompleteSubscriber, ApplicationForeground.IFailureSubscriber foregroundFailureSubscriber, bool forceReinitialize = false)
        {
            try
            {
                if (!this.isInitialized || forceReinitialize)
                {
                    this.foregroundTaskCompleteSubscriber = foregroundTaskCompleteSubscriber;
                    this.foregroundFailureSubscriber = foregroundFailureSubscriber;

                    this.isInitialized = true;
                }

                return new ValueTask<ICallResult>(new CallResult());
            }
            catch (Exception ex) { return new ValueTask<ICallResult>(CallResult.FromException(ex)); }
        }

        public void AddTask(Func<ValueTask> source)
        {
            var wrapper = new CustomBackgroundTaskWrapper(source);
            this.AddTaskCore(wrapper);
        }

        public void AddTask(Func<ValueTask> source, Guid id)
        {
            var wrapper = new CustomBackgroundTaskWrapper(source, new BackgroundTaskData(id), new());
            this.AddTaskCore(wrapper);
        }

        public void AddTask(Func<ValueTask> source, BackgroundTaskData data)
        {
            this.AddTaskCore(new CustomBackgroundTaskWrapper(source, data, new()));
        }

        public void AddTask(BackgroundTaskWrapper wrapper)
        {
            this.AddTaskCore(wrapper);
        }

        //public bool TryGetTask(Guid taskId, out BackgroundTaskWrapper task)
        //{
        //    throw new NotImplementedException();
        //}

        public void StartTask(Guid id)
        {
            this.StartTaskCore(id);
        }

        public void KillTask(Guid id)
        {
            var wrapper = this.tasks[id];
            wrapper.KillFlag.Value = true;
        }

        public void Wait(Guid id)
        {
            var wrapper = this.tasks[id];
            wrapper.BackgroundTask.Wait();
        }

        public async Task WaitAsync(Guid id)
        {
            var wrapper = this.tasks[id];
            await wrapper.BackgroundTask.WaitAsync().ConfigureAwait(false);
        }
        public void AddStartTask(Func<ValueTask> source)
        {
           
        }
        public void KillTaskWait(Guid taskId) 
        { 
            throw new NotImplementedException(); 
        }

        public Task KillTaskWaitAsync(Guid taskId)
        {
            throw new NotImplementedException();
        }


        protected void AddTaskCore(BackgroundTaskWrapper wrapper)
        {

        }

        protected void StartTaskCore(Guid id)
        {
            var wrapper = this.tasks[id];
            wrapper.BackgroundTask.Start();
        }

        private void OnTaskComplete(BackgroundTaskWrapper task)
        {
            this.foregroundTaskCompleteSubscriber.OnTaskComplete(new ApplicationForeground.TaskCompleteArgs() { TaskData = task.Data, Result = task.Result });
        }

        //should await onFailure instructions from foreground?
        //or offload to foreground completely and move on?
        private void OnTaskFailure(BackgroundTaskWrapper task)
        {
            //await handle failure args ?
            this.foregroundFailureSubscriber.OnFailure(new ApplicationForeground.FailureArgs() { FailedResult = task.Result });
        }

        public void AddStartTask()
        {
            throw new NotImplementedException();
        }

        public void Wait(Guid taskId, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public Task WaitAsync(Guid taskId, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public void KillTaskWait(Guid taskId, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public Task KillTaskWaitAsync(Guid taskId, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public interface IConfiguration : IConfigurationWrapper
        {

        }
        public class Configuration : ApplicationConfigurationWrapper, IConfiguration
        {
            public Configuration(IApplicationConfiguration innerApplicationConfiguration) : base(innerApplicationConfiguration) { }
        }

        //use cancellation tokens instead of boolean flags !!!
        private class BackgroundTaskKillFlagWrapper
        {
            public BackgroundTaskWrapper BackgroundTask { get; set; }
            public BooleanFlag KillFlag { get; set; }
        }

        private class BackgroundTaskManagerException : Exception
        {
            private const string BackgroundTaskManagerExecptionFormat = "Background Task Manager Exception: {0}"; //extend to include task data 
            public BackgroundTaskManagerException(string message) : base(string.Format(BackgroundTaskManagerExecptionFormat, message)) { }
            public BackgroundTaskManagerException(Exception innerException) : base(string.Format(BackgroundTaskManagerExecptionFormat, innerException.Message), innerException) { }
            public BackgroundTaskManagerException(string message, Exception innerException) : base(string.Format(BackgroundTaskManagerExecptionFormat, message), innerException) { }
        }
    }

    #endregion Background Task Manager

    #region Background Task Wrapper

    //extend to allow return values ie BackgroundTaskWrapper<TReturnValue> => ValueTask<TReturnValue>  //<TArgs, TReturnValue>  //make disposable ? 

    public abstract class BackgroundTaskWrapperBase
    {
        protected virtual BackgroundTaskData data { get; } = new();
        protected virtual BackgroundTaskSettings settings { get; } = new(); //data should not be exposed, but maybe settings for realtime changes? 

        protected Task task;
        protected Exception exception;
        protected BooleanFlag killFlag = new(); //should killFlag be exposed ? //killflag should be cancellation token 

        //if these are exposed properties should be readonly ! 
        public BackgroundTaskData Data => this.data;
        public BackgroundTaskSettings Settings => this.settings;

        public Guid Id => this.data.Id;

        //public Task Task => this.task; //should task be exposed ? probably not 
        public TaskStatus Status => this.task.Status;

        public BackgroundTaskWrapperBase() { }
        public BackgroundTaskWrapperBase(BackgroundTaskData data) { this.data = data; }
        public BackgroundTaskWrapperBase(BackgroundTaskSettings settings) { this.settings = settings; }
        public BackgroundTaskWrapperBase(BackgroundTaskData data, BackgroundTaskSettings settings) { this.data = data; this.settings = settings; }
    }

    public abstract class BackgroundTaskWrapper0 : BackgroundTaskWrapperBase
    {
        protected abstract ValueTask ActionAsync();
    }

    public abstract class BackgroundTaskWrapper0<TResult> : BackgroundTaskWrapperBase where TResult : ICallResult
    {

        protected TResult result;
        public virtual TResult Result => this.result; 

        protected abstract ValueTask<TResult> ActionAsync();
    }

    public abstract class BackgroundTaskWrapper  //KillWaits should return callresults 
    {
        protected virtual BackgroundTaskData data { get; } = new();
        protected virtual BackgroundTaskSettings settings { get; } = new(); //data should not be exposed, but maybe settings for realtime changes? 

        protected Task task;
        protected Exception exception;
        protected BooleanFlag killFlag = new(); //should killFlag be exposed ? //killflag should be cancellation token 

        //if these are exposed properties should be readonly ! 
        public BackgroundTaskData Data => this.data;
        public BackgroundTaskSettings Settings => this.settings;

        public Guid Id => this.data.Id;

        //public Task Task => this.task; //should task be exposed ? probably not 

        public TaskStatus Status => this.task.Status;

        public ICallResult Result; //??

        //could nest btw in bm to hide events, then expose events through bm as intermediary, probably better practice //should not be generally exposed. may nest Wrapper in Manager, or provide manager ref to wrapper ... idk 
        public event EventHandler OnTaskCompleteHandler;
        public event EventHandler<CrashReport> OnExceptionRaisedHandler;

        public BackgroundTaskWrapper() { }
        public BackgroundTaskWrapper(BackgroundTaskData data) { this.data = data; }
        public BackgroundTaskWrapper(BackgroundTaskSettings settings) { this.settings = settings; }
        public BackgroundTaskWrapper(BackgroundTaskData data, BackgroundTaskSettings settings) { this.data = data; this.settings = settings; }

        protected abstract ValueTask ActionAsync(); //killflag should be cancellation token 

        //protected abstract Func<ValueTask> TaskDelegate { get; }

        public void Start()
        {
            this.task = Task.Run(async () =>
            {
                try { await ActionAsync().ConfigureAwait(false); this.OnTaskComplete(); }
                catch (Exception ex) { this.OnExceptionRaised(ex); }  //OnFailureAsync()
            });
        }

        public void Kill() => this.KillCore();

        public virtual void KillWait()
        {
            this.KillCore();
            this.Wait();
        }

        public virtual async Task KillWaitAsync()  //KillWaitAsync should return ICallResult ?
        {
            this.KillCore();
            await this.WaitAsync().ConfigureAwait(false);
        }

        private TimeSpan defaultTimeout = new();

        public virtual bool KillWait(TimeSpan waitTimeout = default)
        {
            this.KillCore();

            return this.task.Wait((waitTimeout == default) ? this.defaultTimeout : waitTimeout);
        }

        public virtual void KillWait(BooleanFlag waitCancelFlag)
        {
            this.KillCore();

            while (!waitCancelFlag.Value && this.task.Status != TaskStatus.Running) ; //?
        }

        public virtual async Task KillWaitAsync(TimeSpan waitTimeout) 
        {
            this.KillCore();

            await this.task.WaitAsync((waitTimeout == default) ? this.defaultTimeout : waitTimeout).ConfigureAwait(false);
        }

        public virtual async Task KillWaitAsync(BooleanFlag waitCancelFlag)
        {
            this.KillCore();
        }

        //careful not to get deadlocked
        public virtual void Wait() => this.task.Wait();
        public virtual async Task WaitAsync() => await this.task.ConfigureAwait(false); //not technically a "wait". technically an "await"...lol 

        protected void OnTaskComplete() => this.OnTaskCompleteHandler(this, EventArgs.Empty);
        protected void OnExceptionRaised(Exception ex, bool suppressKillTask = false)  //ValueTask<HandleFailureArgs> OnFailureAsync() !!! OnException is vestige of former application using BTM/BW. Make consistent with existing scheme !!!
        {
            this.exception = ex;

            if (!suppressKillTask) this.KillCore();

            this.OnExceptionRaisedHandler(this, new CrashReport() { Exception = this.exception });
        }

        //protected void OnFailedCallResult() { }

        protected void KillCore() => this.killFlag.Value = true; //need thread safety here ???
    }

    public class CustomBackgroundTaskWrapper : BackgroundTaskWrapper
    {
        private readonly Func<ValueTask> taskAction;

        public CustomBackgroundTaskWrapper(Func<ValueTask> taskAction) : base() { this.taskAction = taskAction; }
        public CustomBackgroundTaskWrapper(Guid taskId, Func<ValueTask> taskAction) : base(new BackgroundTaskData(taskId), new BackgroundTaskSettings()) { this.taskAction = taskAction; }
        public CustomBackgroundTaskWrapper(Func<ValueTask> taskAction, BackgroundTaskData data, BackgroundTaskSettings settings) : base(data, settings) { this.taskAction = taskAction; }

        protected override async ValueTask ActionAsync() => await this.taskAction().ConfigureAwait(false);
    }

    #endregion Background Task Wrapper

    #region Models

    public struct BackgroundTaskData
    {
        public Guid Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string CallingTypeName { get; }
        public string CallingFunctionName { get; }

        public BackgroundTaskData(Guid id = default, 
                                  string name = default, 
                                  string description = default, 
                                  string callingTypeName = default, 
                                  string callingFunctionName = default)
        {
            this.Id = (id == default) ? Guid.NewGuid() : id;
            this.Name = name;
            this.Description = description;
            this.CallingTypeName = callingTypeName;
            this.CallingFunctionName = callingFunctionName;
        }
    }

    public struct BackgroundTaskSettings
    {
        public bool CrashMain { get; } = false;
        public BackgroundTaskSettings(bool crashMain = false) { this.CrashMain = false; }
    }

    public class CrashReport : EventArgs
    {
        public Exception Exception { get; set; }
    }

    #endregion Models

    #region Old Background Task Manager

    //OLD BTM 

    //will eventually be instance class in call context 
    //for now treat as static class 

    //need to extend to handle security for references that could disappear while BTs are still running 
    //public class BackgroundTaskManager0 //: IBackgroundTaskManager
    //{

    //    private static readonly Dictionary<Guid, BackgroundTaskKillFlagWrapper> backgroundTasks = new();

    //    private static Settings settings;
    //    private static bool isInitiailized = false;


    //    //events to publish task events to concerned callers 
    //    //subscriber methods should be allowed to return ValueTasks instead of void 

    //    public event EventHandler OnTaskCompleteHandler;
    //    public event EventHandler<CrashReport> OnTaskCrashedHandler;


    //    //public BackgroundTaskManager() { }
    //    //public BackgroundTaskManager(Settings settings) { this.settings = settings; }


    //    //!!! build event overrides into BTM, maybe dont expose wrapper event handlers to caller ... ie onComplete and onError delegates from caller 
    //    public static bool Initialize(bool forceReinitialize = false) => Initialize(new(), forceReinitialize);

    //    public static bool Initialize(Settings settings, bool forceReinitialize = false)
    //    {
    //        try
    //        {
    //            if (!isInitiailized || forceReinitialize) { BackgroundTaskManager0.settings = settings; return true; }
    //            else return false;
    //        }
    //        catch (Exception) { return false; }
    //    }

    //    public static Guid AddTask(Func<ValueTask> task)
    //    {
    //        var id = Guid.NewGuid();
    //        AddTask(task, id);
    //        return id;
    //    }

    //    public static void AddTask(Func<ValueTask> task, Guid taskId) => AddTask(new CustomBackgroundTaskWrapper(task, new BackgroundTaskData() { Id = taskId }, new BackgroundTaskSettings()));

    //    public static void AddTask(BackgroundTaskWrapper task)
    //    {
    //        task.OnTaskCompleteHandler += OnTaskComplete;
    //        task.OnExceptionRaisedHandler += OnExceptionRaised;

    //        lock (backgroundTasks)
    //            backgroundTasks.Add(task.Id, new BackgroundTaskKillFlagWrapper() { BackgroundTask = task, KillFlag = new()});
    //    }

    //    public static Guid AddStartTask(Func<ValueTask> task)
    //    {
    //        var id = Guid.NewGuid();
    //        AddStartTask(task, id);
    //        return id;
    //    }

    //    public static void AddStartTask(Func<ValueTask> task, Guid taskId) => AddStartTask(new CustomBackgroundTaskWrapper(task, new BackgroundTaskData() { Id = taskId }, new BackgroundTaskSettings()));

    //    public static void AddStartTask(BackgroundTaskWrapper task)
    //    {
    //        AddTask(task);
    //        task.StartTask();
    //    }

    //    public static void StartTask(Guid id) { lock (backgroundTasks) { backgroundTasks[id].BackgroundTask.StartTask(); } }

    //    public static void KillTask(Guid taskId)
    //    {
    //        var task = backgroundTasks[taskId];

    //        lock (task)
    //            task.KillFlag.Value = true;
    //    }

    //    public static void Wait(Guid taskId)
    //    {
    //        var task = backgroundTasks[taskId];

    //        lock (task)
    //            task.BackgroundTask.Task.Wait();
    //    }

    //    public static void KillTaskWait(Guid taskId)
    //    {
    //        var task = backgroundTasks[taskId];

    //        lock (task)
    //        {
    //            task.KillFlag.Value = true;
    //            task.BackgroundTask.Task.Wait();
    //        }
    //    }


    //    //start all 

    //    public static async Task StartPollingForExceptions()
    //    {

    //        while (true)
    //        {
    //            await Task.Delay(settings.PollingDelayMilliseconds);
    //            ;
    //        }
    //    }

    //    private static void OnTaskComplete(object sender, EventArgs args)
    //    {

    //    }

    //    private static void OnExceptionRaised(object sender, CrashReport crashReport)
    //    {
    //        BackgroundTaskWrapper task;

    //        lock (backgroundTasks)
    //            task = backgroundTasks[((BackgroundTaskWrapper)sender).Id].BackgroundTask;

    //        if (task.Settings.CrashMain)
    //            throw BuildException(task, crashReport);
    //    }

    //    private static Exception BuildException(BackgroundTaskWrapper task, CrashReport crashReport)
    //    {
    //        throw new NotImplementedException();
    //    }


    //    //use cancellation tokens instead of boolean flags !!!
    //    private class BackgroundTaskKillFlagWrapper
    //    {
    //        public BackgroundTaskWrapper BackgroundTask { get; set; }
    //        public BooleanFlag KillFlag { get; set; }
    //    }

    //    private class BackgroundTaskManagerException : Exception
    //    {
    //        private const string BackgroundTaskManagerExecptionFormat = "Background Task Manager Exception: {0}"; //extend to include task data 
    //        public BackgroundTaskManagerException(string message) : base(string.Format(BackgroundTaskManagerExecptionFormat, message)) { }
    //        public BackgroundTaskManagerException(Exception innerException) : base(string.Format(BackgroundTaskManagerExecptionFormat, innerException.Message), innerException) { }
    //        public BackgroundTaskManagerException(string message, Exception innerException) : base(string.Format(BackgroundTaskManagerExecptionFormat, message), innerException) { }
    //    }

    //    public class Settings
    //    {
    //        public int PollingDelayMilliseconds { get; set; } = 1000;
    //    }
    //}

    #endregion Old Background Task Manager
}
