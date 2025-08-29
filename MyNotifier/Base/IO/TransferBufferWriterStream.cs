using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MyNotifier.Base.IO
{
    public sealed class TransferBufferWriterStream : Stream, ITransferBufferStream
    {
        private readonly TransferBuffer transferBuffer;
        public TransferBufferWriterStream(TransferBuffer transferBuffer)
        {
            this.transferBuffer = transferBuffer;
            this.transferBuffer.Attach(this);
        }

        protected override void Dispose(bool disposing)
        {
            this.Flush();

            transferBuffer.Detach(this);
            base.Dispose(disposing);
        }

        public TransferBuffer TransferBuffer { get => this.transferBuffer; }


        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
            this.FlushAsync().GetAwaiter().GetResult();
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public sealed override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) =>
            TransferBuffer.TaskToApm.Begin(WriteAsync(buffer, offset, count, default), callback, state);

        public sealed override void EndWrite(IAsyncResult asyncResult) =>
            TransferBuffer.TaskToApm.End(asyncResult);

        public override void Write(byte[] buffer, int offset, int count)
        {
            var writeTask = this.transferBuffer.WriteAsync(buffer, offset, count);
            if (writeTask.IsCompletedSuccessfully)
            {
                return;
            }

            writeTask.GetAwaiter().GetResult();
        }


        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return this.transferBuffer.WriteAsync(buffer, offset, count, cancellationToken).AsTask();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            this.transferBuffer.SetWriteCompleted();
            return this.transferBuffer.WaitForReadCompletedAsync(this.FlushTimeoutMsec).AsTask();
        }

        public ValueTask<bool> FlushAsync(int timeoutMsec)
        {
            this.transferBuffer.SetWriteCompleted();
            return this.transferBuffer.WaitForReadCompletedAsync(timeoutMsec);
        }

        public int FlushTimeoutMsec { get; set; } = 30000;
    }
}
