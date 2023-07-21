namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    /// <summary>
    ///     Disable by progressive message when a progressive update comes in from the server
    ///     that indicates the system should be disabled.
    /// </summary>
    /// 
    public class DisableByProgressiveMessage : IResponse
    {
        public DisableByProgressiveMessage(
            ResponseCode code)
        {
            ResponseCode = code;
        }

        public ResponseCode ResponseCode { get; set; }
    }
}
