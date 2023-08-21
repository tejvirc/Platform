namespace Aristocrat.Monaco.G2S.Tests
{
    using Aristocrat.G2S;

    internal static class TestConstants
    {
        public const int HostId = 1;

        public const string EgmId = "ATI_TEST123";

        public const int TimeToLive = 30 * 1000; // It's in milliseconds

        public const string InvalidDeviceErrorCode = ErrorCode.G2S_APX003;

        public const string TimeToLiveExpiredErrorCode = ErrorCode.G2S_APX011;

        public const string RestrictedToOwnerErrorCode = ErrorCode.G2S_APX010;

        public const string RestrictedToOwnerAndGuestsErrorCode = ErrorCode.G2S_APX012;
    }
}