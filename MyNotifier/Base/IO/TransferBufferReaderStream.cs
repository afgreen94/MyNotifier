using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MyNotifier.Base.IO
{
    public sealed class TransferBufferReaderStream : Stream, ITransferBufferStream
    {
        private readonly TransferBuffer transferBuffer;

        public TransferBufferReaderStream(TransferBuffer transferBuffer)
        {
            this.transferBuffer = transferBuffer;
            this.transferBuffer.Attach(this);
        }

        protected override void Dispose(bool disposing)
        {
            transferBuffer.Detach(this);
            base.Dispose(disposing);
        }

        public TransferBuffer TransferBuffer { get => this.transferBuffer; }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        internal bool leaveOpen { get; set; }

        public override void Flush()
        {
        }

        

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public sealed override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) =>
            TransferBuffer.TaskToApm.Begin(ReadAsync(buffer, offset, count, default), callback, state);

        public sealed override int EndRead(IAsyncResult asyncResult) =>
            TransferBuffer.TaskToApm.End<int>(asyncResult);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return this.transferBuffer.ReadAsync(buffer, offset, count, cancellationToken).AsTask();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readTask = this.transferBuffer.ReadAsync(buffer, offset, count);
            if(readTask.IsCompletedSuccessfully)
            {
                return readTask.Result;
            }

            return readTask.GetAwaiter().GetResult();
        }
    }

   
}
