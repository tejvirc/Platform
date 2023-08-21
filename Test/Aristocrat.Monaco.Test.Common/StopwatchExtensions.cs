namespace Aristocrat.Monaco.Test.Common
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using Monaco.Common.Storage;

    public static class StopwatchExtensions
    {
        public static IQueryable<T> LogTimeForGetQueryable<T>(
            this Stopwatch stopwatch,
            String message,
            Expression<Func<IQueryable<T>>> proc)
            where T : BaseEntity
        {
            stopwatch.Start();
            var result = proc.Compile().Invoke();
            stopwatch.Stop();
            Trace.WriteLine(message + " " + stopwatch.ElapsedMilliseconds + " ms.");
            return result;
        }
    }
}