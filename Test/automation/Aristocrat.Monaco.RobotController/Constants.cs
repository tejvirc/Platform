using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
