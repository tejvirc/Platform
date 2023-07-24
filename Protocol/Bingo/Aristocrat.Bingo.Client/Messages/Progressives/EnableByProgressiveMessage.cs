namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    /// <summary>
    ///     Enable by progressive message when a progressive update comes in from the server
    ///     that indicates the system should be enabled.
    /// </summary>
    /// 
    public class EnableByProgressiveMessage : IResponse
    {
        public EnableByProgressiveMessage(
            ResponseCode code)
        {
            ResponseCode = code;
        }

        public ResponseCode ResponseCode { get; set; }
    }
}
