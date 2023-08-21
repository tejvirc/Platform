namespace Aristocrat.Monaco.Application.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Contracts.Localization;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     Directory stream.
    /// </summary>
    /// <seealso cref="System.IO.Stream" />
    internal class DirectoryStream : Stream
    {
        private const string DefaultSearchPattern = @"*";

        private readonly IEnumerable<FileInfo> _files;

        private FileStream _currentStream;
        private int _currentStreamIndex;
        private bool _disposed;
        private bool _endReached;
        private List<FileStream> _fileStreams;
        private long _length = -1;
        private long _position;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectoryStream" /> class.
        /// </summary>
        /// <param name="directory">The directory.</param>
        public DirectoryStream(DirectoryInfo directory)
            : this(directory, DefaultSearchPattern)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectoryStream" /> class.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="searchPattern">The search pattern.</param>
        public DirectoryStream(DirectoryInfo directory, string searchPattern)
            : this(directory, searchPattern, SearchOption.TopDirectoryOnly)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectoryStream" /> class.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="searchOption">The search option.</param>
        public DirectoryStream(DirectoryInfo directory, string searchPattern, SearchOption searchOption)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (string.IsNullOrEmpty(searchPattern))
            {
                searchPattern = DefaultSearchPattern;
            }

            _files = directory.GetFilesByPattern(searchPattern, searchOption);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectoryStream" /> class.
        /// </summary>
        /// <param name="files">The file list to include</param>
        public DirectoryStream(IReadOnlyCollection<FileInfo> files)
        {
            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            _files = new List<FileInfo>(files);
        }

        /// <summary>
        ///     Gets the length in bytes of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                if (_length == -1)
                {
                    _length = _files.Sum(f => f.Length);
                }

                return _length;
            }
        }

        /// <summary>
        ///     Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StreamNotSeekable));
        }

        /// <summary>
        ///     Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        ///     Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        ///     Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => false;

        private List<FileStream> FileStreams
        {
            get
            {
                return _fileStreams ??
                       (_fileStreams =
                           _files.Select(file => file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                               .ToList());
            }
        }

        /// <summary>
        ///     Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            _currentStream?.Flush();
        }

        /// <summary>
        ///     When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">
        ///     A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to
        ///     obtain the new position.
        /// </param>
        /// <returns>
        ///     The new position within the current stream.
        /// </returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StreamNotSeekable));
        }

        /// <summary>
        ///     When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        public override void SetLength(long value)
        {
            _length = value;
        }

        /// <summary>
        ///     When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position
        ///     within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">
        ///     An array of bytes. When this method returns, the buffer contains the specified byte array with the
        ///     values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced
        ///     by the bytes read from the current source.
        /// </param>
        /// <param name="offset">
        ///     The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read
        ///     from the current stream.
        /// </param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        ///     The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many
        ///     bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (FileStreams.Count == 0)
            {
                return 0;
            }

            if (_currentStream == null)
            {
                _currentStream = FileStreams[0];
            }

            if (_endReached)
            {
                for (var i = offset; i < offset + count; i++)
                {
                    buffer[i] = 0;
                }

                return 0;
            }

            var result = 0;
            var buffPosition = offset;

            while (count > 0)
            {
                var bytesRead = _currentStream.Read(buffer, buffPosition, count);

                result += bytesRead;
                buffPosition += bytesRead;
                _position += bytesRead;

                if (bytesRead <= count)
                {
                    count -= bytesRead;
                }

                if (count <= 0)
                {
                    continue;
                }

                if (_currentStreamIndex >= FileStreams.Count - 1)
                {
                    _endReached = true;
                    break;
                }

                _currentStream = FileStreams[++_currentStreamIndex];
            }

            return result;
        }

        /// <summary>
        ///     When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current
        ///     position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">
        ///     An array of bytes. This method copies <paramref name="count" /> bytes from
        ///     <paramref name="buffer" /> to the current stream.
        /// </param>
        /// <param name="offset">
        ///     The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the
        ///     current stream.
        /// </param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StreamNotWritableErrorMessage));
        }

        /// <summary>
        ///     Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed
        ///     resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_fileStreams != null)
                    {
                        foreach (var fileStream in _fileStreams)
                        {
                            fileStream?.Close();
                        }

                        _fileStreams.Clear();
                        _fileStreams = null;
                    }
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
