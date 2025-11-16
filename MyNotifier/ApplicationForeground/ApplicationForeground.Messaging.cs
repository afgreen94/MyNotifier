using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier
{
    public partial class ApplicationForeground
    {
        public abstract class Message
        {

            protected Guid id = Guid.NewGuid(); //provide default value? use Guid.Empty?

            public virtual Guid Id
            {
                get { return this.id; }
                set { this.id = value; }  //have setter ? 
            }
            public virtual MessageStatus Status { get; set; } = MessageStatus.Unprocessed;
            public abstract MessageType Type { get; }


            public static bool TryCastAs<TMessage>(Message message, out TMessage cast) where TMessage : Message  //assumes message is not null 
            {
                cast = message as TMessage;

                return cast == null;
            }

        }
        public abstract class Message<TArgs> : Message
        {
            protected readonly TArgs args;

            protected bool locked = false;
            protected SemaphoreSlim semaphore = new(1, 1);

            public virtual TArgs Value => this.args;
        
            protected Message(TArgs args) { this.args = args; }

            //careful not to get deadlocked 
            protected virtual async Task WaitCoreAsync()
            {
                if (this.locked) return;  //?

                this.locked = true;

                await this.semaphore.WaitAsync().ConfigureAwait(false);
            }
            protected virtual void ReleaseCore()
            {
                if (!this.locked) return;

                this.locked = false;

                this.semaphore.Release();
            }
        }

        public abstract class Message<TArgs, TResult> : Message<TArgs>
        {
            protected readonly bool expectingResult;

            protected TResult result;

            public virtual TResult Result => this.result;

            public virtual bool ExpectingResult => this.expectingResult;
            public virtual bool ResultBeingAwaited => this.locked;  //redundant...in message status 
            public virtual bool ContainsResult => this.result != null;

            protected Message(TArgs args, bool expectingResult = false) : base(args) { this.expectingResult = expectingResult; }

            //idk if this is a good idea tbh.. would at very least want timed deadlock revisor in foreground 
            public virtual async Task AwaitResultAsync() => await this.WaitCoreAsync().ConfigureAwait(false);
            public virtual void Release() => this.ReleaseCore();

            public virtual void SetResult(TResult result) => this.result = result;  //this is gettings sloppy
        }

        public class UpdateAvailableMessage : Message<UpdateAvailableArgs>
        {
            public UpdateAvailableMessage(UpdateAvailableArgs args) : base(args) { }

            public override MessageType Type => MessageType.Update;
            public override UpdateAvailableArgs Value => this.args;
        }

        public class CommandIssuedMessage : Message<ICommand, ICommandResult>
        {
            public override MessageType Type => MessageType.Command;

            public CommandIssuedMessage(ICommand args, bool expectingResult = false) : base(args, expectingResult) { }
        }

        public class TaskCompleteMessage : Message<ICallResult>
        {
            public override MessageType Type => MessageType.TaskComplete;

            public TaskCompleteMessage(ICallResult result) : base(result) { }
        }

        public class FailureMessage : Message<FailureArgs, HandleFailureArgs>
        {
            public override MessageType Type => MessageType.Failure;

            public FailureMessage(FailureArgs args, bool expectingResult = false) : base(args, expectingResult) { }
        }

        //rename 
        public enum MessageStatus  //status 
        {
            Unprocessed,
            Processing,
            ProcessedAwaitingResultRetrieval,
            Processed,
            Faulted
        }
        public enum MessageType  //type 
        {
            Update,
            Command,
            TaskComplete,
            Failure
        }

        public enum Priority
        {
            Highest=0,
            High,
            Medium,
            Low,
            Lowest
        }

        //make priority queue, sort by message priority, then message time, then element priority, then element time 
        public class MessageQueue  //there are many more efficient ways of doing r/w without locking every time, will handle later !!! 
        {
            private readonly Queue<Message> innerQueue = new();

            public void Enqueue(Message message) { lock (this.innerQueue) { this.innerQueue.Enqueue(message); } }
            public bool TryDequeue(out Message message) { lock (this.innerQueue) { return this.innerQueue.TryDequeue(out message); } }
        }

        //public class MessageResultsDictionary
        //{
        //    private readonly IDictionary<Guid, Message> innerDictionary = new Dictionary<Guid, Message>();

        //    public void Add(Message message) { lock (this.innerDictionary) { this.innerDictionary.Add(message.Id, message); } }
        //    public bool TryAdd(Message message) { bool added = false; lock (this.innerDictionary) { added = this.innerDictionary.TryAdd(message.Id, message); } return added; }
        //    public bool ContainsKey(Guid messageId) => this.innerDictionary.ContainsKey(messageId);
        //    public bool TryGetValue(Guid messageId, out Message message) => this.innerDictionary.TryGetValue(messageId, out message);
        //}
    }
}
