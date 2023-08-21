namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey
{
    using System.Drawing;
    using Contracts.Gds.Reel;

    public static class HarkeyMessageExtensions
    {
        public static bool IsResponseError(this int responseCode)
        {
            return responseCode is (int)HarkeyResponseErrorCodes.NotAvailable or
                (int)HarkeyResponseErrorCodes.NoSync or
                (int)HarkeyResponseErrorCodes.Skew or
                (int)HarkeyResponseErrorCodes.Stall;
        }

        public static ushort ToWord(this Color color)
        {
            const int redShift = 11; // red is top 5 bits of the word
            const int greenShift = 5; // green is middle 6 bits of word

            const int maxRed = 31; // Red is 5 bits
            const int maxGreen = 31; // Green is 6 bits
            const int maxBlue = 31; // Blue is 5 bits
            const int maxByte = 255;

            var red = (double)color.R / maxByte * maxRed;
            var green = (double)color.G / maxByte * maxGreen;
            var blue = (double)color.B / maxByte * maxBlue;

            return (ushort)(((int)red << redShift) | ((int)green << greenShift) | (int)blue);
        }

        public static FailureStatus ToReelFailureStatus(this int errorCode, int reelId)
        {
            return (HarkeyRequestErrorCodes)errorCode switch
            {
                HarkeyRequestErrorCodes.InvalidValue => new FailureStatus
                {
                    ReelId = reelId + 1, ComponentError = true, ErrorCode = (byte)errorCode
                },
                HarkeyRequestErrorCodes.ReelInError => new FailureStatus
                {
                    ReelId = reelId + 1, ComponentError = true, ErrorCode = (byte)errorCode
                },
                HarkeyRequestErrorCodes.ReelAlreadySpinning => new FailureStatus
                {
                    ReelId = reelId + 1, ComponentError = true, ErrorCode = (byte)errorCode
                },
                HarkeyRequestErrorCodes.BadState => new FailureStatus
                {
                    ReelId = reelId + 1, ComponentError = true, ErrorCode = (byte)errorCode
                },
                HarkeyRequestErrorCodes.ReelNotAvailable => new FailureStatus
                {
                    ReelId = reelId + 1, ComponentError = true, ErrorCode = (byte)errorCode
                },
                HarkeyRequestErrorCodes.OutOfSync => new FailureStatus
                {
                    ReelId = reelId + 1, TamperDetected = true, ErrorCode = (byte)errorCode
                },
                HarkeyRequestErrorCodes.LowVoltage => new FailureStatus
                {
                    ReelId = reelId + 1, LowVoltageDetected = true, ErrorCode = (byte)errorCode
                },
                HarkeyRequestErrorCodes.GameChecksumError => new FailureStatus
                {
                    ReelId = reelId + 1, FirmwareError = true, ErrorCode = (byte)errorCode
                },
                HarkeyRequestErrorCodes.FaultChecksumError => new FailureStatus
                {
                    ReelId = reelId + 1, FirmwareError = true, ErrorCode = (byte)errorCode
                },
                _ => null
            };
        }
    }
}