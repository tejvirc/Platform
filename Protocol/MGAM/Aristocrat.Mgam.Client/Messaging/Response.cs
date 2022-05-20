namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Base class for a message response.
    /// </summary>
    public abstract class Response : IResponse
    {
        /// <summary>
        ///     Initializes a new instance of instance of the <see cref="Response"/> class.
        /// </summary>
        protected Response()
        {
            ResponseCode = ServerResponseCode.Ok;
        }

        /// <summary>
        ///     Gets or sets the response status code.
        /// </summary>
        public ServerResponseCode ResponseCode { get; set; }
    }
}
