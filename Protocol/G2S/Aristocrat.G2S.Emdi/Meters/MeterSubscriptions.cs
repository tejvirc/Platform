namespace Aristocrat.G2S.Emdi.Meters
{
    using System;
    using System.Collections.Concurrent;
    using Protocol.v21ext1b1;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    ///     Implements <see cref="IMeterSubscriptions"/> interface
    /// </summary>
    public class MeterSubscriptions : IMeterSubscriptions
    {
        private readonly ConcurrentDictionary<int, List<MeterSubscription>> _subscriptions = new ConcurrentDictionary<int, List<MeterSubscription>>();

        /// <inheritdoc />
        public async Task AddAsync(int port, string meterName, t_meterTypes meterType)
        {
            if (!_subscriptions.ContainsKey(port) && !_subscriptions.TryAdd(port, new List<MeterSubscription>()))
            {
                throw new InvalidOperationException($"EMDI: Error allocating list for subscriptions on port {port}");
            }

            if (!_subscriptions.TryGetValue(port, out List<MeterSubscription> subs))
            {
                throw new InvalidOperationException($"EMDI: Error getting subscriptions on port {port}");
            }

            if (subs.Any(sub => sub.Meter.Name == meterName))
            {
                return;
            }

            subs.Add(new MeterSubscription(port, (meterName, meterType)));

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task RemoveAsync(int port, string meterName)
        {
            if (!_subscriptions.ContainsKey(port))
            {
                return;
            }

            if (!_subscriptions.TryGetValue(port, out List<MeterSubscription> subs))
            {
                throw new InvalidOperationException($"EMDI: Error getting subscriptions on port {port}");
            }

            var item = subs.FirstOrDefault(sub => sub.Meter.Name == meterName);

            if (item == null)
            {
                return;
            }

            subs.Remove(item);

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task RemoveAllAsync(int port)
        {
            if (!_subscriptions.ContainsKey(port))
            {
                return;
            }

            if (!_subscriptions.TryGetValue(port, out List<MeterSubscription> subs))
            {
                throw new InvalidOperationException($"EMDI: Error retrieving subscriptions on port {port}");
            }

            subs.Clear();

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<IList<MeterSubscription>> GetSubscriptionsAsync(int port)
        {
            if (!_subscriptions.ContainsKey(port))
            {
                return Enumerable.Empty<MeterSubscription>().ToList();
            }

            if (!_subscriptions.TryGetValue(port, out List<MeterSubscription> subs))
            {
                throw new InvalidOperationException($"EMDI: Error retrieving subscriptions on port {port}");
            }

            return await Task.FromResult(subs);
        }

        /// <inheritdoc />
        public async Task<IList<MeterSubscriber>> GetSubscribersAsync(params string[] meterNames)
        {
            var subscribers = _subscriptions
                .SelectMany(sub => sub.Value)
                .Where(sub => meterNames.Contains(sub.Meter.Name))
                .Select(sub => new MeterSubscriber(sub.Port))
                .ToList();

            return await Task.FromResult(subscribers);
        }
    }
}
