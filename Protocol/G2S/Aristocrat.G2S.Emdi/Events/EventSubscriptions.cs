namespace Aristocrat.G2S.Emdi.Events
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    ///     Manages media content subscriptions to events
    /// </summary>
    public class EventSubscriptions : IEventSubscriptions
    {
        private readonly ConcurrentDictionary<int, List<EventSubscription>> _subscriptions = new ConcurrentDictionary<int, List<EventSubscription>>();

        /// <inheritdoc />
        public async Task AddAsync(int port, string eventCode)
        {
            if (!_subscriptions.ContainsKey(port) && !_subscriptions.TryAdd(port, new List<EventSubscription>()))
            {
                throw new InvalidOperationException($"EMDI: Error allocating list for subscriptions on port {port}");
            }

            if (!_subscriptions.TryGetValue(port, out List<EventSubscription> subs))
            {
                throw new InvalidOperationException($"EMDI: Error getting subscriptions on port {port}");
            }

            if (subs.Any(sub => sub.EventCode == eventCode))
            {
                return;
            }

            subs.Add(new EventSubscription(port, eventCode));

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task RemoveAsync(int port, string eventCode)
        {
            if (!_subscriptions.ContainsKey(port))
            {
                return;
            }

            if (!_subscriptions.TryGetValue(port, out List<EventSubscription> subs))
            {
                throw new InvalidOperationException($"EMDI: Error getting subscriptions on port {port}");
            }

            var item = subs.FirstOrDefault(sub => sub.EventCode == eventCode);

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

            if (!_subscriptions.TryGetValue(port, out List<EventSubscription> subs))
            {
                throw new InvalidOperationException($"EMDI: Error getting subscriptions on port {port}");
            }

            subs.Clear();

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<IList<EventSubscription>> GetSubscriptionsAsync(int port)
        {
            if (!_subscriptions.ContainsKey(port))
            {
                return Enumerable.Empty<EventSubscription>().ToList();
            }

            if (!_subscriptions.TryGetValue(port, out List<EventSubscription> subs))
            {
                throw new InvalidOperationException($"EMDI: Error getting subscriptions on port {port}");
            }

            return await Task.FromResult(subs);
        }

        /// <inheritdoc />
        public async Task<IList<EventSubscriber>> GetSubscribersAsync(params string[] eventCodes)
        {
            var subscribers = _subscriptions
                .SelectMany(sub => sub.Value)
                .Where(sub => eventCodes.Contains(sub.EventCode))
                .Select(sub => new EventSubscriber(sub.Port))
                .ToList();

            return await Task.FromResult(subscribers);
        }
    }
}
