namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    public class RtpValidationStatus
    {
        public RtpValidationStatus(bool isRtpValid, string rtpValidationStatusMessage)
        {
            IsRtpValid = isRtpValid;
            RtpValidationStatusMessage = rtpValidationStatusMessage;
        }

        public bool IsRtpValid  { get; }

        public string RtpValidationStatusMessage { get; }
    }
}