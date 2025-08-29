using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyNotifier.Base.IO
{

    public partial class TransferBuffer : IDisposable
    {
        private TransferBufferReaderStream attachedReadSteam;

        private TransferBufferWriterStream attachedWriteSteam;

        private Range availableToReadRange;

        private Range availableToWriteRange;

        private List<Buffer> buffers;

        private bool disposedValue;

        private int? finalWriteBuffer = null;

        private bool hasReadFirst;

        private int lastBufferWrittenIndex = -1;

        private int lastReadBufferIndex = -1;

        private SemaphoreSlim readCompletedSemaphore = new SemaphoreSlim(0, 1);

        private int readCompletedSemaphoreSingelWaitTimeoutMsec = 77;

        private Exception readException;

        private bool readStreamDetached = false;

        private int singleWaitForReadTimeoutMSec = 100;

        private int singleWaitForWriteTimeoutMSec = 100;

        private ulong totalBytesRead;

        private ulong totalBytesWritten;

        //private object updateRangeSync = new object();
        private int totalWaitForReadTimeoutMSec = 300000;
        private int totalWaitForWriteTimeoutMSec = 300000;
        private ManualResetEventSlim waitingForReadToCompleteMRE = new ManualResetEventSlim(false);
        private SemaphoreSlim waitingForReadToCompleteSemaphore = new SemaphoreSlim(0, 1);
        private ManualResetEventSlim waitingForWriteToCompleteMRE = new ManualResetEventSlim(false);
        private SemaphoreSlim waitingForWriteToCompleteSemaphore = new SemaphoreSlim(0, 1);
        private Exception writeException;
        private bool writeStreamDetached = false;

        public TransferBuffer(int bufferCount = 35, int bufferSize = 4096)
        {
            this.buffers = new List<Buffer>();
            for (int i = 0; i < bufferCount; i++)
            {
                buffers.Add(new Buffer() { Bytes = new byte[bufferSize] });
            }

            this.availableToWriteRange = new Range(bufferCount);
            this.availableToWriteRange.SetRangeByCount(0, bufferCount);

            this.availableToReadRange = new Range(bufferCount);
        }
        
        public Action FirstReadAction { get; set; }

        public Exception ReadException { get => this.readException; set => this.readException = value; }

        public ulong TotalBytesRead { get => this.totalBytesRead; }

        public ulong TotalBytesWritten { get => this.totalBytesWritten; }

        public Exception WriteExecption { get => this.writeException; set => this.writeException = value; }

        public void Attach(TransferBufferReaderStream stream)
        {
            lock (this)
            {
                if (this.attachedReadSteam != null)
                    throw new NotSupportedException("cannot reattach read stream");

                this.attachedReadSteam = stream;
            }
        }

        public void Attach(TransferBufferWriterStream stream)
        {
            lock (this)
            {
                if (this.attachedWriteSteam != null)
                    throw new NotSupportedException("cannot reattach write stream");

                this.attachedWriteSteam = stream;
            }
        }

        public void Detach(TransferBufferReaderStream stream)
        {
            bool disposeNeeded = false;
            lock (this)
            {
                if (this.attachedReadSteam == null)
                {
                    return;
                }
                
                if(this.attachedReadSteam != stream)
                    throw new NotSupportedException("cannot detach a different stream");

                this.attachedReadSteam = null;
                readStreamDetached = true;

                if (this.readStreamDetached && this.writeStreamDetached)
                    disposeNeeded = true;
            }

            if (disposeNeeded == true)
                this.Dispose();
        }


        public void Detach(TransferBufferWriterStream stream)
        {
            bool disposeNeeded = false;
            lock (this)
            {
                if (this.attachedWriteSteam == null)
                {
                    return;
                }

                if (this.attachedWriteSteam != stream)
                    throw new NotSupportedException("cannot detach a different stream");

                this.attachedWriteSteam = null;
                writeStreamDetached = true;

                if (this.readStreamDetached && this.writeStreamDetached)
                    disposeNeeded = true;
            }

            if (disposeNeeded == true)
                this.Dispose();
        }



        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        //public Action FirstReadAction { get; set; }

        public int Read(byte[] outBuffer, int offset, int count)
        {
            return this.ReadAsync(outBuffer, offset, count).GetAwaiter().GetResult();
        }

        public async ValueTask<int> ReadAsync(byte[] outBuffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            try
            {
                this.CheckReadAsyncValid();

                var waitingForWriteRemainingMSec = this.totalWaitForWriteTimeoutMSec;
                int remaining = count;
                int totalBytesRead = 0;

                var localReadRange = new Range(this.availableToReadRange);
                int originalStart = localReadRange.Start;
                int rangeIncrement = 0;

                if (this.hasReadFirst == false)
                {
                    this.hasReadFirst = true;

                    //if (this.FirstReadAction != null)
                    //    this.FirstReadAction();

                    if (this.FirstReadAction != null)
                        this.FirstReadAction();
                }

                while (remaining > 0)
                {
                    if (this.IsCompleted == true)
                    {
                        try
                        {
                            if (this.readCompletedSemaphore.CurrentCount > 0)
                                this.readCompletedSemaphore.Release();
                        }
                        catch
                        {
                        }
                        return 0;
                    }

                    while (localReadRange.Count > 0 && remaining > 0)
                    {
                        this.CheckReadAsyncValid();

                        var currentBuffer = this.buffers[localReadRange.Start];
                        if (!(currentBuffer.State == BufferState.WriteCompleted ||
                            currentBuffer.State == BufferState.Reading))
                            break;

                        int remainingToReadInBuffer = currentBuffer.ByteCount - currentBuffer.ReadCount;

                        int copyCount = System.Math.Min(remaining, remainingToReadInBuffer);

                        this.Copy(currentBuffer.Bytes, currentBuffer.ReadCount, outBuffer, offset, copyCount);
                        this.lastReadBufferIndex = localReadRange.Start;
                        this.totalBytesRead += (uint)copyCount;

                        currentBuffer.ReadCount += copyCount;
                        offset += copyCount;
                        remaining -= copyCount;
                        totalBytesRead += copyCount;

                        if (currentBuffer.ReadCount == currentBuffer.ByteCount)
                        {
                            currentBuffer.State = BufferState.ReadingCompleted;

                            localReadRange.IncrementStart();
                            rangeIncrement++;
                        }
                        else
                        {
                            currentBuffer.State = BufferState.Reading;
                        }
                    }


                    if (totalBytesRead > 0)
                    {
                        this.availableToReadRange = new Range(localReadRange);

                        this.waitingForReadToCompleteMRE.Set();
                        if (this.waitingForReadToCompleteSemaphore.CurrentCount == 0)
                        {
                            this.waitingForReadToCompleteSemaphore.Release();
                        }

                        return totalBytesRead;
                    }

                    bool waitResult = this.waitingForWriteToCompleteMRE.Wait(10);

                    if (waitResult == false)
                    {
                        waitResult = await this.waitingForWriteToCompleteSemaphore.WaitAsync(this.singleWaitForWriteTimeoutMSec).ConfigureAwait(false);
                    }

                    if (waitResult == false)
                        waitResult = this.waitingForWriteToCompleteMRE.Wait(10);


                    if (waitResult == false)
                    {
                        this.CheckReadAsyncValid();


                        waitingForWriteRemainingMSec -= this.singleWaitForWriteTimeoutMSec;
                        if (waitingForWriteRemainingMSec > 0)
                            continue;

                        if (this.writeException != null)
                            throw this.readException;

                        try
                        {
                            throw new TimeoutException();
                        }
                        catch (Exception ex)
                        {
                            this.readException = ex;
                            throw;
                        }
                    }
                    else
                    {
                        this.CheckReadAsyncValid();


                        this.waitingForWriteToCompleteMRE.Reset();
                        waitingForWriteRemainingMSec = this.totalWaitForReadTimeoutMSec;


                        localReadRange = this.ExpandLocalRange(localReadRange, originalStart, rangeIncrement, false, false);
                        originalStart = localReadRange.Start;
                        rangeIncrement = 0;
                        this.availableToReadRange = new Range(localReadRange);
                    }
                }
            }
            catch (OtherSideException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.readException = ex;
                throw;
            }
            finally
            {

            }
            return 0;
        }

        public void SetWriteCompleted()
        {
            if (this.finalWriteBuffer.HasValue == true)
                return;

            if (this.availableToWriteRange.Count == 0)
            {
                this.finalWriteBuffer = this.lastBufferWrittenIndex;
            }
            else
            {
                var lastBuffer = this.buffers[lastBufferWrittenIndex];

                if (lastBuffer.State == BufferState.Writting)
                {
                    lastBuffer.State = BufferState.WriteCompleted;
                    var localWriteRange = new Range(this.availableToWriteRange);
                    localWriteRange.IncrementStart();
                    this.availableToWriteRange = new Range(localWriteRange);
                }
                else
                {
                }
                this.finalWriteBuffer = this.lastBufferWrittenIndex;
            }

            this.waitingForWriteToCompleteMRE.Set();
            if (this.waitingForWriteToCompleteSemaphore.CurrentCount == 0)
            {
                this.waitingForWriteToCompleteSemaphore.Release();
            }

        }
        public async ValueTask<bool> WaitForReadCompletedAsync(int millisecondsTimeout, CancellationToken cancellationToken = default)
        {
            if (millisecondsTimeout <= 0)
                millisecondsTimeout = int.MinValue;

            while (true)
            {
                if (millisecondsTimeout != int.MinValue && millisecondsTimeout < 0)
                    throw new TimeoutException();

                if (this.IsCompleted == true)
                {
                    return true;
                }

                if(this.readStreamDetached == true)
                {
                    throw new TimeoutException("Read stream detached prior to write complete");
                }

                if (this.writeException != null)
                    throw new InvalidOperationException("cannot wait on Transferbuffer with exception", this.writeException);

                if (this.readException != null)
                    throw new InvalidOperationException("cannot wait on Transferbuffer with exception", this.readException);


                var waitResult = await this.readCompletedSemaphore.WaitAsync(this.readCompletedSemaphoreSingelWaitTimeoutMsec, cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested == true)
                    return waitResult;

                if (waitResult == true)
                {
                    try
                    {
                        this.readCompletedSemaphore.Release();
                    }
                    catch
                    {
                    }

                    return true;
                }

                if (this.disposedValue == true)
                {
                    if (this.IsCompleted == true)
                        return true;

                    throw new ObjectDisposedException(nameof(TransferBuffer));
                }

                if (millisecondsTimeout != int.MinValue)
                    millisecondsTimeout -= this.readCompletedSemaphoreSingelWaitTimeoutMsec;
            }
        }

        public void Write(byte[] inBuffer, int offset, int count)
        {
            this.WriteAsync(inBuffer, offset, count).GetAwaiter().GetResult();
        }

        public async ValueTask WriteAsync(byte[] inBuffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            if (count == 0)
                return;

            int waitingForReadRemainingMSec = this.totalWaitForReadTimeoutMSec;
            var localWriteRange = new Range(this.availableToWriteRange);
            int originalStart = localWriteRange.Start;
            int rangeIncrement = 0;

            bool hasWritten = false;

            try
            {
                int remaining = count;
                while (remaining > 0)
                {
                    this.CheckWriteAsyncValid();

                    if (localWriteRange.Count > 0)
                    {
                        var buffer = this.buffers[localWriteRange.Start];

                        var state = buffer.State;
                        if (state == BufferState.None ||
                            state == BufferState.ReadingCompleted ||
                            (state == BufferState.Writting && remaining == count))
                        {
                            if (state != BufferState.Writting)
                            {
                                buffer.State = BufferState.Writting;
                                buffer.ReadCount = 0;
                                buffer.ByteCount = 0;
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException($"{state}");
                        }
                        var availableInBuffer = buffer.Bytes.Length - buffer.ByteCount;

                        var canCopyCount = System.Math.Min(remaining, availableInBuffer);

                        this.Copy(inBuffer, offset, buffer.Bytes, buffer.ByteCount, canCopyCount);
                        this.lastBufferWrittenIndex = localWriteRange.Start;
                        this.totalBytesWritten += (uint)canCopyCount;

                        hasWritten = true;
                        buffer.ByteCount += canCopyCount;
                        remaining -= canCopyCount;
                        offset += canCopyCount;

                        if (buffer.ByteCount == buffer.Bytes.Length)
                        {
                            buffer.State = BufferState.WriteCompleted;

                            localWriteRange.IncrementStart();
                            rangeIncrement++;


                            var state0 = this.buffers[localWriteRange.Start].State;

                            if (state == BufferState.None ||
                                state == BufferState.ReadingCompleted ||
                                state == BufferState.Writting)
                            {

                            }
                            else
                            {
                                throw new InvalidOperationException($"{state}");
                            }

                        }


                        continue;
                    }


                    if (hasWritten == true)
                    {
                        hasWritten = false;
                        originalStart = localWriteRange.Start;
                        rangeIncrement = 0;
                        this.availableToWriteRange = new Range(localWriteRange);
                        this.waitingForWriteToCompleteMRE.Set();
                        if (this.waitingForWriteToCompleteSemaphore.CurrentCount == 0)
                        {
                            this.waitingForWriteToCompleteSemaphore.Release();
                        }

                    }

                    //if no blocks in buffer that can be written to then need to wait for some reads to complete

                    bool waitResult = this.waitingForReadToCompleteMRE.Wait(10);

                    if (waitResult == false)
                    {
                        waitResult = await this.waitingForReadToCompleteSemaphore.WaitAsync(this.singleWaitForReadTimeoutMSec).ConfigureAwait(false);
                    }

                    if (waitResult == false)
                        waitResult = this.waitingForReadToCompleteMRE.Wait(10);

                    if (waitResult == false)
                    {
                        this.CheckWriteAsyncValid();


                        waitingForReadRemainingMSec -= this.singleWaitForReadTimeoutMSec;
                        if (waitingForReadRemainingMSec > 0)
                        {
                            continue;
                        }

                        if (this.readException != null)
                            throw this.readException;

                        try
                        {
                            throw new TimeoutException();
                        }
                        catch (Exception ex)
                        {
                            this.writeException = ex;
                            throw;
                        }
                    }
                    else
                    {
                        this.CheckWriteAsyncValid();


                        this.waitingForReadToCompleteMRE.Reset();
                        localWriteRange = this.ExpandLocalRange(localWriteRange, originalStart, rangeIncrement, true, false);
                        originalStart = localWriteRange.Start;
                        rangeIncrement = 0;
                        this.availableToWriteRange = new Range(localWriteRange);

                        waitingForReadRemainingMSec = this.totalWaitForReadTimeoutMSec;
                    }
                }

                if (hasWritten == true)
                {
                    hasWritten = false;

                    localWriteRange = this.ExpandLocalRange(localWriteRange, originalStart, rangeIncrement, true, false);
                    originalStart = localWriteRange.Start;
                    rangeIncrement = 0;
                    this.availableToWriteRange = new Range(localWriteRange);


                    this.waitingForWriteToCompleteMRE.Set();
                    if (this.waitingForWriteToCompleteSemaphore.CurrentCount == 0)
                    {
                        this.waitingForWriteToCompleteSemaphore.Release();
                    }
                }

            }
            catch (OtherSideException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.writeException = ex;
                throw;
            }
            finally
            {
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
                if (disposing)
                {
                    this.waitingForReadToCompleteMRE.Dispose();
                    this.waitingForWriteToCompleteMRE.Dispose();
                    this.readCompletedSemaphore.Dispose();
                    this.waitingForReadToCompleteSemaphore.Dispose();
                    this.waitingForWriteToCompleteSemaphore.Dispose();
                }
            }
        }

        private void CheckReadAsyncValid()
        {
            if (this.disposedValue == true)
                throw new ObjectDisposedException(nameof(TransferBuffer));
            if (this.writeException != null)
                throw new OtherSideException(this.writeException);
        }

        private void CheckWriteAsyncValid()
        {
            if (this.disposedValue == true)
                throw new ObjectDisposedException(nameof(TransferBuffer));
            if (this.readException != null)
                throw new OtherSideException(this.readException);
        }

        private void Copy(byte[] sourceArray,
                  int sourceIndex,
                  byte[] destinationArray,
                  int destinationIndex,
                  int length)
        //bool log,
        //bool isRead,
        //[CallerMemberName] string callerName = null)
        {
            Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);

            //if (log)
            //{
            //    var sbSource = new StringBuilder();
            //    var sbDest = new StringBuilder();
            //    for (int i = 0; i < 20 && i < length; i++)
            //    {
            //        sbSource.Append(((int)sourceArray[sourceIndex + i]).ToString());
            //        sbSource.Append(";");

            //        sbDest.Append(((int)destinationArray[destinationIndex + i]).ToString());
            //        sbDest.Append(";");
            //    }

            //    System.Diagnostics.Debug.WriteLine($"{callerName}:{this.GetHashCode()} tr:{this.BytesRead + (isRead ? length : 0)} tw:{this.BytesWritten + (!isRead ? length : 0)} si:{sourceIndex} di:{destinationIndex} len:{length}  ri:{this.readIndex} wi:{this.writeIndex}\r\n" +
            //                                        $"    srcBytes:{sbSource.ToString()}\r\n" +
            //                                        $"    dstBytes:{sbDest.ToString()}");
            //}
        }

        private Range ExpandLocalRange(Range localRange0,
                                                    int originalStart, 
                                            int rangeIncrement, 
                                            bool isPrimaryWrite, 
                                            bool checkSequence)
        {
            if (localRange0.Count == buffers.Count)
                return localRange0;

            Range localRange = new Range(localRange0);
            var primaryRange = new Range(this.availableToWriteRange);
            var otherRange = new Range(this.availableToReadRange);

            

            var localStartState = this.buffers[localRange.Start].State;

            try
            {
                if (isPrimaryWrite == true)
                {
                    if (localStartState == BufferState.None ||
                       localStartState == BufferState.ReadingCompleted ||
                       localStartState == BufferState.Writting)
                    {

                    }
                    else
                    {
                        return localRange;
                    }
                   
                }
                else
                {
                    if (localStartState == BufferState.WriteCompleted ||
                       localStartState == BufferState.Reading)
                    {

                    }
                    else
                    {
                        return localRange;
                    }

                    var tmp = primaryRange;
                    primaryRange = otherRange;
                    otherRange = tmp;
                }

               
                if(otherRange.Start == localRange.Start)
                {
                    if(otherRange.Count == 0)
                    {
                        localRange.OtherSequence = otherRange.Sequence;
                        localRange.SetRangeByCount(localRange.Start, buffers.Count);
                    }
                    else if(otherRange.Count > 0 &&
                        localRange.Count == 0)
                    {
                        localRange.OtherSequence = otherRange.Sequence;
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
                else
                {
                    var otherRangePrevious = otherRange.Start - 1;
                    if (otherRangePrevious < 0)
                        otherRangePrevious = this.buffers.Count - 1;


                    localRange.SetRangeByEndInclusive(localRange.Start, otherRangePrevious, otherRange.Sequence);
                }
               

                return localRange;
            }
            finally
            {
            }
        }


        private bool IsCompleted
        {
            get
            {
                return this.lastReadBufferIndex != -1 &&
                            this.finalWriteBuffer.HasValue &&
                            this.finalWriteBuffer.Value == this.lastReadBufferIndex &&
                            this.buffers[this.lastReadBufferIndex].State != BufferState.Reading;
            }
        }

       
        
        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TransferBuffer()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
    }
}
