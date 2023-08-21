namespace Aristocrat.Monaco.Gaming.Runtime
{
    using System;
    using System.IO.MemoryMappedFiles;
    using System.Runtime.InteropServices;
    using System.Threading;

    /// <summary>
    ///     Provides a mechanism to read/write a struct to a memory mapped file (MMF) for use in inter process communications
    ///     (IPC)
    /// </summary>
    public class SharedObject : IDisposable
    {
        private int _bufferSize;

        private bool _disposed;
        private MemoryMappedFile _mmf;
        private string _name;
        private Mutex _sharedMutex;
        private MemoryMappedViewAccessor _view;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Opens a read/write shared object
        /// </summary>
        /// <typeparam name="T">A structure type</typeparam>
        public void Open<T>()
            where T : struct
        {
            _name = typeof(T).Name;

            _bufferSize = Marshal.SizeOf(typeof(T));

            InternalOpen();
        }

        /// <summary>
        ///     Reads an instance of <typeparamref name="T" /> from the buffer
        /// </summary>
        /// <typeparam name="T">A structure type</typeparam>
        /// <returns>The read structure</returns>
        public T Read<T>()
            where T : struct
        {
            return Read<T>(Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        ///     Reads an instance of <typeparamref name="T" /> from the buffer
        /// </summary>
        /// <typeparam name="T">A structure type</typeparam>
        /// <param name="timeout">
        ///     A System.TimeSpan that represents the number of milliseconds to wait, or a System.TimeSpan that
        ///     represents -1 milliseconds to wait indefinitely
        /// </param>
        /// <returns>The read structure</returns>
        public T Read<T>(TimeSpan timeout)
            where T : struct
        {
            var data = new byte[_bufferSize];

            var ptr = Marshal.AllocHGlobal(_bufferSize);

            try
            {
                if (!_sharedMutex.WaitOne(timeout))
                {
                    return default(T);
                }

                _view.ReadArray(0, data, 0, data.Length);

                Marshal.Copy(data, 0, ptr, _bufferSize);

                return ((T?)Marshal.PtrToStructure(ptr, typeof(T))).Value;
            }
            finally
            {
                _sharedMutex.ReleaseMutex();

                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        ///     Writes an instance of <typeparamref name="T" /> into the buffer
        /// </summary>
        /// <typeparam name="T">A structure type</typeparam>
        /// <param name="data">A reference to an instance of <typeparamref name="T" /> to be written</param>
        public void Write<T>(ref T data)
            where T : struct
        {
            Write(ref data, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        ///     Writes an instance of <typeparamref name="T" /> into the buffer
        /// </summary>
        /// <typeparam name="T">A structure type</typeparam>
        /// <param name="data">A reference to an instance of <typeparamref name="T" /> to be written</param>
        /// <param name="timeout">
        ///     A System.TimeSpan that represents the number of milliseconds to wait, or a System.TimeSpan that
        ///     represents -1 milliseconds to wait indefinitely
        /// </param>
        public void Write<T>(ref T data, TimeSpan timeout)
            where T : struct
        {
            var buffer = new byte[_bufferSize];

            // Initialize unmanaged memory.
            var ptr = Marshal.AllocHGlobal(_bufferSize);

            try
            {
                Marshal.StructureToPtr(data, ptr, false);

                Marshal.Copy(ptr, buffer, 0, _bufferSize);

                if (!_sharedMutex.WaitOne(timeout))
                {
                    return;
                }

                _view.WriteArray(0, buffer, 0, buffer.Length);
            }
            finally
            {
                _sharedMutex.ReleaseMutex();

                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        ///     Exchanges an instance of <typeparamref name="T" /> with the current value of <typeparamref name="T" /> if the current value is different
        /// </summary>
        /// <typeparam name="T">A structure type</typeparam>
        /// <param name="data">A reference to an instance of <typeparamref name="T" /> to be written</param>
        /// <returns>The original structure</returns>
        public T CompareExchange<T>(ref T data)
            where T : struct, IEquatable<T>
        {
            return CompareExchange(ref data, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        ///     Exchanges an instance of <typeparamref name="T" /> with the current value of <typeparamref name="T" /> if the current value is different
        /// </summary>
        /// <typeparam name="T">A structure type</typeparam>
        /// <param name="data">A reference to an instance of <typeparamref name="T" /> to be written</param>
        /// <param name="timeout">
        ///     A System.TimeSpan that represents the number of milliseconds to wait, or a System.TimeSpan that
        ///     represents -1 milliseconds to wait indefinitely
        /// </param>
        /// <returns>The original structure</returns>
        public T CompareExchange<T>(ref T data, TimeSpan timeout)
            where T : struct, IEquatable<T>
        {
            var buffer = new byte[_bufferSize];

            var readPtr = Marshal.AllocHGlobal(_bufferSize);
            var writePtr = Marshal.AllocHGlobal(_bufferSize);

            try
            {
                if (!_sharedMutex.WaitOne(timeout))
                {
                    return default(T);
                }

                _view.ReadArray(0, buffer, 0, buffer.Length);

                Marshal.Copy(buffer, 0, readPtr, _bufferSize);

                var current = (T?)Marshal.PtrToStructure(readPtr, typeof(T));
                if (current == null) throw new InvalidOperationException();
                if (!current.Value.Equals(data))
                {
                    var writeBuffer = new byte[_bufferSize];

                    Marshal.StructureToPtr(data, writePtr, false);

                    Marshal.Copy(writePtr, writeBuffer, 0, _bufferSize);

                    _view.WriteArray(0, writeBuffer, 0, writeBuffer.Length);
                }

                return (T)current;
            }
            finally
            {
                _sharedMutex.ReleaseMutex();

                Marshal.FreeHGlobal(readPtr);
                Marshal.FreeHGlobal(writePtr);
            }
        }

        /// <summary>
        ///     Closes a read/write shared object
        /// </summary>
        public void Close()
        {
            if (_view != null)
            {
                _view.Dispose();
            }

            if (_mmf != null)
            {
                _mmf.Dispose();
            }

            if (_sharedMutex != null)
            {
                _sharedMutex.Dispose();
            }

            _mmf = null;
            _view = null;
            _sharedMutex = null;
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Close();
            }

            _disposed = true;
        }

        private void InternalOpen()
        {
            Close();

            _mmf = MemoryMappedFile.CreateNew(_name, _bufferSize);

            _sharedMutex = new Mutex(true, $"{_name}Mutex", out _);
            _view = _mmf.CreateViewAccessor(0, _bufferSize, MemoryMappedFileAccess.ReadWrite);
            _sharedMutex.ReleaseMutex();
        }
    }
}
