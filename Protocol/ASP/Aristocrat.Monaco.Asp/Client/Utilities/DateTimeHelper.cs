namespace Aristocrat.Monaco.Asp.Client.Utilities
{
    using System;

    public class DateTimeHelper
    {
        public static uint GetNumberOfSecondsSince1990()
        {
            return (uint) (DateTime.UtcNow - new DateTime(1990, 1, 1).ToUniversalTime()).TotalSeconds;
        }
    }
}
