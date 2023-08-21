namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Kernel;
    using Mono.Addins;

    /// <summary>
    ///     An IPersistenceClearArbiter implementation that is a public service and uses addin rules to
    ///     determine if persistence clearing should be allowed.
    /// </summary>
    public class AddinRuleBasedPersistenceClearArbiter : IPersistenceClearArbiter, IService, IDisposable
    {
        private bool _disposed;
        private List<string> _fullClearDenyReasons = new List<string>();
        private readonly object _lock = new object();
        private List<string> _partialClearDenyReasons = new List<string>();

        private readonly List<IPersistenceClearRule> _rules = new List<IPersistenceClearRule>();

        /// <summary>Disposes the object</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public bool PartialClearAllowed { get; private set; }

        /// <inheritdoc />
        public bool FullClearAllowed { get; private set; }

        /// <inheritdoc />
        public string[] PartialClearDeniedReasons => _partialClearDenyReasons.ToArray();

        /// <inheritdoc />
        public string[] FullClearDeniedReasons => _fullClearDenyReasons.ToArray();

        /// <inheritdoc />
        public string Name => "Persistence Clear Arbiter";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IPersistenceClearArbiter) };

        /// <inheritdoc />
        public void Initialize()
        {
            lock (_lock)
            {
                var addinHelper = ServiceManager.GetInstance().GetService<IAddinHelper>();
                foreach (var node in addinHelper.GetSelectedNodes<TypeExtensionNode>(
                    "/Application/PersistenceClearRules"))
                {
                    var rule = (IPersistenceClearRule)node.CreateInstance();
                    rule.RuleChangedEvent += OnRuleChanged;
                    _rules.Add(rule);
                }

                UpdateAllowances();
            }
        }

        /// <summary>
        ///     Disposes the object
        /// </summary>
        /// <param name="disposing">Indicates whether or not to clean up managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            lock (_lock)
            {
                _disposed = true;
                if (disposing)
                {
                    foreach (var rule in _rules)
                    {
                        rule.RuleChangedEvent -= OnRuleChanged;
                        (rule as IDisposable)?.Dispose();
                    }

                    _rules.Clear();
                }
            }
        }

        private void UpdateAllowances()
        {
            PartialClearAllowed = true;
            FullClearAllowed = true;
            var newPartialReasons = new List<string>();
            var newFullReasons = new List<string>();

            foreach (var rule in _rules)
            {
                if (!rule.PartialClearAllowed)
                {
                    PartialClearAllowed = false;
                    newPartialReasons.Add(rule.ClearDeniedReason);
                }

                if (!rule.FullClearAllowed)
                {
                    FullClearAllowed = false;
                    newFullReasons.Add(rule.ClearDeniedReason);
                }
            }

            _partialClearDenyReasons = newPartialReasons;
            _fullClearDenyReasons = newFullReasons;

            ServiceManager.GetInstance().GetService<IEventBus>().Publish(
                new PersistenceClearAuthorizationChangedEvent(
                    PartialClearAllowed,
                    FullClearAllowed,
                    PartialClearDeniedReasons,
                    FullClearDeniedReasons));
        }

        private void OnRuleChanged(object sender, EventArgs args)
        {
            lock (_lock)
            {
                UpdateAllowances();
            }
        }
    }
}