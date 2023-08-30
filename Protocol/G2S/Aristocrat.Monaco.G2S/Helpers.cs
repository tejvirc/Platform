namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Threading;

    public class Helpers
    {
        public static T RetryForever<T>(Func<T> action, int delayBetweenRetriesInMilliseconds = 10000)
        {
            while (true)
            {
                try
                {
                    return action();
                }
                catch (Exception)
                {
                    Thread.Sleep(delayBetweenRetriesInMilliseconds);
                }
            }
        }
    }
}
