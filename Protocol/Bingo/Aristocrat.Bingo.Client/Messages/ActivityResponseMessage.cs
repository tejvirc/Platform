namespace Aristocrat.Bingo.Client.Messages
{
    using System;

    public class ActivityResponseMessage : IResponse
    {
        public ActivityResponseMessage(ResponseCode responseCode, DateTime responseTime)
        {
            ResponseCode = responseCode;
            ResponseTime = responseTime;
        }

        public ResponseCode ResponseCode { get; }

        public DateTime ResponseTime { get; }
    }
}