﻿namespace Aristocrat.Mgam.Client.Messaging
{
    using System;

    /// <summary>
    ///     This message is received as a response to the RequestGUID message on the specified UDP port,
    ///     and contains the generated GUID.
    /// </summary>
    public class RequestGuidResponse : Response
    {
        /// <summary>
        ///     Gets or sets a <see cref="Guid"/> generated by the host.
        /// </summary>
        public Guid Guid { get; set; }
    }
}
