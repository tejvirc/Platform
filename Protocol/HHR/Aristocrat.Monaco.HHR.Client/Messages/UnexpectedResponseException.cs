namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Exception when response is not as expected from Request.
    /// </summary>
    [Serializable]
    public class UnexpectedResponseException : Exception
    {
        /// <summary>
        ///     Construction of UnexpectedResponseException
        /// </summary>
        /// <param name="response">Response received from CentralServer</param>
        public UnexpectedResponseException(Response response)
        {
            Response = response;
        }

        /// <inheritdoc />
        public override string Message =>
            $"Unexpected response for SequenceId [{Response.ReplyId}] received with Status - {Response.MessageStatus}";

        /// <summary>
        ///     The response received from Central Server
        /// </summary>
        public Response Response { get; }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Response", Response);
        }
    }
}