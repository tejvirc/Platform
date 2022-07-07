namespace Aristocrat.Monaco.RobotController
{
    public static class Constants
    {
        public const long CashOutTimeout = 5000;
        public const long InsertCreditsDelay = 1000;
        public const long InsertCreditsTimeout = 1 * 60 * 1000;
        public const long IdleTimeout = 20 * 60 * 1000;
        public const long LockupDuration = 10 * 1000;
        public const long AuditMenuDuration = 10 * 1000;
        public const long DuplicateVoucherWindow = 2;
        public const string ConfigurationFileName = @"Aristocrat.Monaco.RobotController.xml";
        public const string GdkRuntimeHostName = "GDKRuntimeHost";
        public const string HandleExitToLobby = "Automation.HandleExitToLobby";
        public const int ToggleJackpotKeyDuration = 1000;
        public const int BalanceCheckDelayDuration = 2000;
        public const int ToggleJackpotKeyLongerDuration = 10000;
        public const int loadGameDelayDuration = 2000;
        public const int CashOutDelayDuration = 5000;
        public const int ServiceRequestDelayDuration = 5000;
    }
}
