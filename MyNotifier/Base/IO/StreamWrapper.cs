using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyNotifier.Base.IO
{
    public class StreamWrapper : Stream
    {
        private Stream innerStream;
        private bool leaveStreamOpen;
        private bool isWriteStream;

        public StreamWrapper(Stream innerStream, bool leaveStreamOpen, bool isWriteStream)
        {
            this.innerStream = innerStream;
            this.leaveStreamOpen = leaveStreamOpen;
            this.isWriteStream = isWriteStream;
        }

        public override bool CanRead => !this.isWriteStream;

        public override bool CanSeek => this.innerStream.CanSeek;

        public override bool CanWrite => this.isWriteStream;

        public override long Length => this.innerStream.Length;

        public override long Position { get => this.innerStream.Position; set => throw new NotImplementedException(); }

        public override void Flush()
        {
            this.innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.isWriteStream == true)
            {
                throw new InvalidOperationException("cannot read from a write stream");
            }

            return this.innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.isWriteStream == false)
            {
                throw new InvalidOperationException("cannot write to a read stream");
            }

            this.innerStream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            base.Close();

            if (this.leaveStreamOpen == false && this.innerStream != null)
            {
                this.innerStream.Close();
            }
        }
    }

}