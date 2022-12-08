namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    using System.Collections.Generic;

    /// <summary>
    ///     Response to a request progressive update call to the server
    /// </summary>
    public class ProgressiveUpdateResults : IResponse
    {
        public ProgressiveUpdateResults()
        {
            ResponseCode = ResponseCode.Ok;
            Accepted = false;
            UpdateInfo = new List<ProgressiveUpdateInfo>();
        }

        public ProgressiveUpdateResults(
            ResponseCode code,
            bool accepted,
            IList<ProgressiveUpdateInfo> progressiveUpdateInfo)
        {
            ResponseCode = code;
            Accepted = accepted;
            UpdateInfo = progressiveUpdateInfo;
        }

        public ResponseCode ResponseCode { get; }

        public bool Accepted { get; }

        public IList<ProgressiveUpdateInfo> UpdateInfo { get; }
    }
}
