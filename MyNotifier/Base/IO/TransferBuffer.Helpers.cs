using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace MyNotifier.Base.IO
{
    public partial class TransferBuffer
    {
        private class Range
        {
            private int maxSize;

            public Range(int maxSize)
            {
                this.Start = 0;
                this.EndInclusive = -1;
                this.maxSize = maxSize;
            }

            public Range(Range copy)
            {
                this.Start = copy.Start;
                this.EndInclusive = copy.EndInclusive;

                this.Count = copy.Count;

                this.Sequence = copy.Sequence;
                this.OtherSequence = copy.OtherSequence;

                this.maxSize = copy.maxSize;
            }

            public int Count { get; private set; }
            public int EndInclusive { get; private set; }
            public ulong OtherSequence { get; set; }
            public ulong Sequence { get; private set; }
            public int Start { get; private set; }
            public override bool Equals(object obj)
            {
                var other = obj as Range;
                return other.Start == this.Start && other.Count == this.Count;
            }

            public override int GetHashCode()
            {
                return this.Start.GetHashCode() ^ this.Count.GetHashCode();
            }

            public void IncrementStart()
            {
                var newStart = this.Start + 1;
                if (newStart >= this.maxSize)
                    newStart = 0;

                this.SetRangeByCount(newStart, this.Count - 1);

                this.Sequence++;
            }

            public void SetRangeByCount(int start, int count)
            {
                this.Start = start;
                this.Count = count;
                if (count == 0)

                {
                    this.EndInclusive = -1;
                }
                else
                {
                    var end = start + count;
                    if (end > this.maxSize)
                    {
                        this.EndInclusive = (end - this.maxSize) - 1;
                    }
                    else
                    {
                        this.EndInclusive = end - 1;
                    }
                }
            }

            public void SetRangeByEndInclusive(int start, int endInclusive, ulong otherSequence)
            {
                int count = (endInclusive == start) ? 1 : (endInclusive > start) ? (endInclusive - start) + 1 : (this.maxSize - start) + (endInclusive + 1);

                this.SetRangeByCount(start, count);
                this.OtherSequence = otherSequence;
                this.Sequence++;
            }
        }


        private enum BufferState
        {
            None,
            Writting,
            WriteCompleted,
            Reading,
            ReadingCompleted,
        }

        private class Buffer
        {
            public int ByteCount { get; set; }
            public byte[] Bytes { get; set; }
            public bool IsLastWriteBuffer { get; set; }
            public int ReadCount { get; set; }

            public BufferState State { get; set; }
        }

        private class OtherSideException : Exception
        {
            public OtherSideException(Exception innerException) : base(innerException.Message, innerException)
            { }
        }

        internal static class TaskToApm
        {
            /// <summary>
            /// Marshals the Task as an IAsyncResult, using the supplied callback and state
            /// to implement the APM pattern.
            /// </summary>
            /// <param name="task">The Task to be marshaled.</param>
            /// <param name="callback">The callback to be invoked upon completion.</param>
            /// <param name="state">The state to be stored in the IAsyncResult.</param>
            /// <returns>An IAsyncResult to represent the task's asynchronous operation.</returns>
            public static IAsyncResult Begin(Task task, AsyncCallback callback, object state) =>
                new TaskAsyncResult(task, state, callback);

            /// <summary>Processes an IAsyncResult returned by Begin.</summary>
            /// <param name="asyncResult">The IAsyncResult to unwrap.</param>
            public static void End(IAsyncResult asyncResult)
            {
                if (GetTask(asyncResult) is Task t)
                {
                    t.GetAwaiter().GetResult();
                    return;
                }

                ThrowArgumentException(asyncResult);
            }

            /// <summary>Processes an IAsyncResult returned by Begin.</summary>
            /// <param name="asyncResult">The IAsyncResult to unwrap.</param>
            public static TResult End<TResult>(IAsyncResult asyncResult)
            {
                if (GetTask(asyncResult) is Task<TResult> task)
                {
                    return task.GetAwaiter().GetResult();
                }

                ThrowArgumentException(asyncResult);
                return default!; // unreachable
            }

            /// <summary>Gets the task represented by the IAsyncResult.</summary>
            public static Task GetTask(IAsyncResult asyncResult) => (asyncResult as TaskAsyncResult)?._task;

            /// <summary>Throws an argument exception for the invalid <paramref name="asyncResult"/>.</summary>
            [DoesNotReturn]
            private static void ThrowArgumentException(IAsyncResult asyncResult) =>
                throw (asyncResult is null ?
                    new ArgumentNullException(nameof(asyncResult)) :
                    new ArgumentException(null, nameof(asyncResult)));

            /// <summary>Provides a simple IAsyncResult that wraps a Task.</summary>
            /// <remarks>
            /// We could use the Task as the IAsyncResult if the Task's AsyncState is the same as the object state,
            /// but that's very rare, in particular in a situation where someone cares about allocation, and always
            /// using TaskAsyncResult simplifies things and enables additional optimizations.
            /// </remarks>
            internal sealed class TaskAsyncResult : IAsyncResult
            {
                /// <summary>The wrapped Task.</summary>
                internal readonly Task _task;
                /// <summary>Callback to invoke when the wrapped task completes.</summary>
                private readonly AsyncCallback _callback;

                /// <summary>Initializes the IAsyncResult with the Task to wrap and the associated object state.</summary>
                /// <param name="task">The Task to wrap.</param>
                /// <param name="state">The new AsyncState value.</param>
                /// <param name="callback">Callback to invoke when the wrapped task completes.</param>
                internal TaskAsyncResult(Task task, object state, AsyncCallback callback)
                {
                    Debug.Assert(task != null);
                    _task = task;
                    AsyncState = state;

                    if (task.IsCompleted)
                    {
                        // Synchronous completion.  Invoke the callback.  No need to store it.
                        CompletedSynchronously = true;
                        callback?.Invoke(this);
                    }
                    else if (callback != null)
                    {
                        // Asynchronous completion, and we have a callback; schedule it. We use OnCompleted rather than ContinueWith in
                        // order to avoid running synchronously if the task has already completed by the time we get here but still run
                        // synchronously as part of the task's completion if the task completes after (the more common case).
                        _callback = callback;
                        _task.ConfigureAwait(continueOnCapturedContext: false)
                             .GetAwaiter()
                             .OnCompleted(InvokeCallback); // allocates a delegate, but avoids a closure
                    }
                }

                /// <summary>Invokes the callback.</summary>
                private void InvokeCallback()
                {
                    Debug.Assert(!CompletedSynchronously);
                    Debug.Assert(_callback != null);
                    _callback.Invoke(this);
                }

                /// <summary>Gets a user-defined object that qualifies or contains information about an asynchronous operation.</summary>
                public object AsyncState { get; }
                /// <summary>Gets a value that indicates whether the asynchronous operation completed synchronously.</summary>
                /// <remarks>This is set lazily based on whether the <see cref="_task"/> has completed by the time this object is created.</remarks>
                public bool CompletedSynchronously { get; }
                /// <summary>Gets a value that indicates whether the asynchronous operation has completed.</summary>
                public bool IsCompleted => _task.IsCompleted;
                /// <summary>Gets a <see cref="WaitHandle"/> that is used to wait for an asynchronous operation to complete.</summary>
                public WaitHandle AsyncWaitHandle => ((IAsyncResult)_task).AsyncWaitHandle;
            }
        }
    }
}
