namespace Aristocrat.Monaco.Hardware.EdgeLight.Manager
{
    using System;
    using System.Collections.Generic;
    using Hardware.Contracts.EdgeLighting;

    internal class PriorityComparer : IComparer<StripPriority>
    {
        private IComparer<StripPriority> _overridenComparer;

        public IComparer<StripPriority> OverridenComparer
        {
            get => _overridenComparer;
            set
            {
                if (ReferenceEquals(value, _overridenComparer))
                {
                    return;
                }

                _overridenComparer = value;
                ComparerChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int Compare(StripPriority x, StripPriority y)
        {
            return OverridenComparer?.Compare(x, y) ?? x.CompareTo(y);
        }

        public event EventHandler<EventArgs> ComparerChanged;
    }
}