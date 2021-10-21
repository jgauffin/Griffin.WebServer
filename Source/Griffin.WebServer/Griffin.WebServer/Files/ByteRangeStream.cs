﻿using System;
using System.IO;
using Griffin.Logging;
using Griffin.Net.Protocols.Http;

namespace Griffin.WebServer.Files
{
    /// <summary>
    /// Used to transfer a byte range
    /// </summary>
    /// <remarks>Will transfer the required ranges to the client. Do note that using multiple ranges means that the response will
    /// be sent as <c>multipart/byteranges</c>. You therefore have to set that header.</remarks>
    public class ByteRangeStream : Stream
    {
        private Stream _innerStream;
        private readonly RangeCollection _ranges;
        private int _currentRangeIndex;
        private int _bytesRead;
        private ILogger _logger = LogManager.GetLogger<ByteRangeStream>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteRangeStream" /> class.
        /// </summary>
        /// <param name="ranges">The HTTP range header contents.</param>
        /// <param name="innerStream">The inner stream which we should transfer a range from. The stream is owned by this class.</param>
        public ByteRangeStream(RangeCollection ranges, Stream innerStream)
        {
            if (innerStream == null) throw new ArgumentNullException(nameof(innerStream));
            if (innerStream.Position != 0)
                throw new ArgumentException("The stream must be at position 0 for this range class to work",
                                            nameof(innerStream));
            if (!innerStream.CanSeek) throw new ArgumentException("Stream must be seekable.", nameof(innerStream));
            if (!innerStream.CanRead) throw new ArgumentException("Stream must be readablle", nameof(innerStream));

            _ranges = ranges;
            _innerStream = innerStream;
        }

        public override void Close()
        {
            if (_innerStream != null)
            {
                _innerStream.Dispose();
                _innerStream = null;
            }

            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _innerStream != null)
            {
                _innerStream.Dispose();
                _innerStream = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>always false.</returns>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>always false.</returns>
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        public override long Length
        {
            get { return _ranges.TotalLength; }
        }

        /// <summary>
        /// Gets the position in the ranges to send
        /// </summary>
        /// <returns>The current position within the stream.</returns>
        /// <exception cref="System.NotSupportedException">this stream can only be used to read ranges.</exception>
        public override long Position
        {
            get { return _bytesRead; }
            set { throw new NotSupportedException("this stream can only be used to read ranges."); }
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <exception cref="System.NotSupportedException">You may not seek in this stream. The ranges are handled internally.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("You may not seek in this stream. The ranges are handled internally.");
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="System.NotSupportedException">You may not set length for this stream. The ranges are handled internally.</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException("You may not set length for this stream. The ranges are handled internally.");
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">count;Tried to read more than was configured for the range.</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count), offset + count, string.Format("Offset+Count larger than the buffer size ({0} bytes).", buffer.Length));
            if (count - offset > _bytesRead + _ranges.TotalLength)
                throw new ArgumentOutOfRangeException(nameof(count), count, string.Format("Trying to read more then is left in the ranges ({0} bytes).", (_ranges.TotalLength - _bytesRead)));

            var bytesToRead = count;
            while (true)
            {
                if (_currentRangeIndex >= _ranges.Count)
                    throw new ArgumentOutOfRangeException(nameof(count), count,
                                                          "Tried to read more bytes when all ranges has been read.");

                var range = _ranges[_currentRangeIndex];
                var read = range.Read(_innerStream, buffer, offset, bytesToRead);
                if (range.IsDone)
                    _currentRangeIndex++;

                _bytesRead += read;

                if (read == bytesToRead)
                    return read;

                bytesToRead -= read;
            }
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="System.NotSupportedException">This stream should only be used to read ranges.</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("This stream should only be used to read ranges.");
        }
    }
}