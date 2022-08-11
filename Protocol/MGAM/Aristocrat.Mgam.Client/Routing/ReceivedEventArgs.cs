namespace Aristocrat.Mgam.Client.Routing
{
    using System;
    using System.Net;

    /// <summary>
    ///     Received event arguments.
    /// </summary>
    internal class ReceivedEventArgs
        : EventArgs
    {
        /// <summary>
        ///     Initializes and instance of the <see cref="ReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="endPoint">The end-point of the client.</param>
        public ReceivedEventArgs(IPEndPoint endPoint, byte[] buffer, long offset, long size)
        {
            EndPoint = endPoint;
            Buffer = buffer;
            Offset = offset;
            Size = size;
        }

        /// <summary>
        ///     Gets the end-point of the client.
        /// </summary>
        public IPEndPoint EndPoint { get; }

        /// <summary>
        ///     Gets or sets the bytes received from the host.
        /// </summary>
        public byte[] Buffer { get; }

        /// <summary>
        ///     Gets the offset of the bytes received.
        /// </summary>
        public long Offset { get; }

        /// <summary>
        ///     Gets the size of the bytes received.
        /// </summary>
        public long Size { get; }
    }
}
