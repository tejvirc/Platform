namespace Aristocrat.Monaco.Hardware.Tests.EdgeLight.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.EdgeLighting;

    internal class TestPriorityComparer : IComparer<StripPriority>
    {
        private readonly List<StripPriority> _priorityOrder;

        public TestPriorityComparer(IReadOnlyCollection<StripPriority> desiredOrder)
        {
            var priorityOrder = ((StripPriority[])Enum.GetValues(typeof(StripPriority))).ToList();
            var firstIndex = priorityOrder.IndexOf(desiredOrder.First());
            foreach (var stripPriority in desiredOrder.Skip(1))
            {
                var currentIndex = priorityOrder.IndexOf(stripPriority);
                if (currentIndex > firstIndex)
                {
                    priorityOrder.RemoveAt(currentIndex);
                    priorityOrder.Insert(firstIndex, stripPriority);
                }

                firstIndex = currentIndex;
            }

            _priorityOrder = priorityOrder;
        }

        public int Compare(StripPriority x, StripPriority y)
        {
            var indexX = _priorityOrder.IndexOf(x);
            var indexY = _priorityOrder.IndexOf(y);
            if (indexX >= 0 && indexY >= 0)
            {
                return indexX.CompareTo(indexY);
            }

            return x.CompareTo(y);
        }
    }
}