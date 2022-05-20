namespace Aristocrat.G2S.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     A set of <see cref="IStartupContext" /> extension methods
    /// </summary>
    public static class StartupContextExtensions
    {
        /// <summary>
        ///     Creates a context for each host provided using the default context provided
        /// </summary>
        /// <param name="this">The context to use for all hosts</param>
        /// <param name="hosts">The list of hosts</param>
        /// <returns>A startup context for each host</returns>
        public static IEnumerable<IStartupContext> ContextPerHost(this IStartupContext @this, IEnumerable<IHost> hosts)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (hosts == null)
            {
                throw new ArgumentNullException(nameof(hosts));
            }

            return hosts.Select(
                h => new StartupContext
                {
                    HostId = h.Id,
                    DeviceChanged = @this.DeviceChanged,
                    SubscriptionLost = @this.SubscriptionLost,
                    DeviceStateChanged = @this.DeviceStateChanged,
                    MetersReset = @this.MetersReset,
                    DeviceReset = @this.DeviceReset,
                    DeviceAccessChanged = @this.DeviceAccessChanged
                });
        }
    }
}