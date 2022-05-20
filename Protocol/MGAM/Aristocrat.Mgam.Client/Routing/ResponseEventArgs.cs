namespace Aristocrat.Mgam.Client.Routing
{
    using System;
    using Messaging;

    /// <summary>
    ///     Response event args.
    /// </summary>
    public class ResponseEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponseEventArgs"/> class.
        /// </summary>
        /// <param name="response"></param>
        public ResponseEventArgs(IResponse response)
        {
            Response = response;
        }

        /// <summary>
        ///     Gets the response.
        /// </summary>
        public IResponse Response { get; }
    }
}
