﻿namespace Aristocrat.G2S.Client.Communications
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Protocol.v21;
    using Devices;
    using Devices.v21;
    using Monaco.Protocol.Common.Communication;

    /// <summary>
    ///     Methods to control an MTP client <see cref="UdpConnection"/>
    /// </summary>
    public interface IMtpClient : IMtpConnectionControl
    {
        /// <summary>
        ///     Joins all registered multicast groups.
        /// </summary>
        void Open();

        /// <summary>
        ///     Joins the specified multicast group.
        /// </summary>
        /// <param name="multicastId">Multicast identifier.</param>
        void Open(string multicastId);

        /// <summary>
        ///     Leaves all known multicast groups.
        /// </summary>
        void Close();

        /// <summary>
        ///     Closes an existing MTP connection.
        /// </summary>
        /// <param name="multicastId">Multicast identifier.</param>
        void Close(string multicastId);

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        public void Dispose();
    }
}
