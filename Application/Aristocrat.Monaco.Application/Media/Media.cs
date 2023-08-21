namespace Aristocrat.Monaco.Application.Media
{
    using Contracts.Media;
    using System;
    using System.Collections.Generic;

    public partial class MediaProvider
    {
        /// <summary>
        ///     Local implementation of <see cref="IMedia"/>
        /// </summary>
        private class Media : IMedia
        {
            /// <inheritdoc />
            public long LogSequence { get; set; }

            /// <inheritdoc />
            public long TransactionId { get; set; }

            /// <inheritdoc />
            public long Id { get; set; }

            /// <inheritdoc />
            public Uri Address { get; set; }

            /// <inheritdoc />
            public long AccessToken { get; set; }

            /// <inheritdoc />
            public MediaState State { get; set; }

            /// <inheritdoc />
            public MediaContentError ExceptionCode { get; set; }

            /// <inheritdoc />
            public IEnumerable<string> AuthorizedEvents { get; set; }

            /// <inheritdoc />
            public IEnumerable<string> AuthorizedCommands { get; set; }

            /// <inheritdoc />
            public long MdContentToken { get; set; }

            /// <inheritdoc />
            public bool EmdiConnectionRequired { get; set; }

            /// <inheritdoc />
            public int EmdiReconnectTimer { get; set; }

            /// <inheritdoc />
            public DateTime? LoadTime { get; set; }

            /// <inheritdoc />
            public DateTime? ReleaseTime { get; set; }

            /// <inheritdoc />
            public bool NativeResolution { get; set; }

            /// <inheritdoc />
            public int PlayerId { get; set; }

            /// <inheritdoc />
            public bool IsFinalized => State == MediaState.Released || State == MediaState.Error;
        }
    }
}
