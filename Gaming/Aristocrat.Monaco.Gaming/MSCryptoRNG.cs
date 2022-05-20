namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.ObjectModel;
    using System.Security.Cryptography;
    using PRNGLib;

    /// <summary>
    ///     An <see cref="IPRNG" /> to be used only for non-gaming related RNGs
    /// </summary>
    [CLSCompliant(false)]
    public class MSCryptoRNG : IPRNG, IDisposable
    {
        private readonly byte[] _buffer = new byte[8];

        private RNGCryptoServiceProvider _cryptoService = new RNGCryptoServiceProvider();

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public ulong GetValue(ulong range)
        {
            ulong value;
            var limit = ulong.MaxValue / range * range;
            var width = limit / range;

            for (;;)
            {
                _cryptoService.GetBytes(_buffer);
                var result = BitConverter.ToUInt64(_buffer, 0);
                if (result < limit)
                {
                    value = result / width;
                    break;
                }
            }

            return value;
        }

        /// <inheritdoc />
        public void GetValues(Collection<ulong> ranges, Collection<ulong> values)
        {
            values.Clear();
            foreach (var range in ranges)
            {
                values.Add(GetValue(range));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _cryptoService.Dispose();
            }

            _cryptoService = null;

            _disposed = true;
        }
    }
}