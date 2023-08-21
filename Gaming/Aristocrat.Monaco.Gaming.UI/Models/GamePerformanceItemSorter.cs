namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using Contracts.Models;

    /// <summary>
    ///     IComparer class to sort GameCombos
    /// </summary>
    public class GamePerformanceItemSorter : IComparer
    {
        private readonly ListSortDirection _direction;
        private readonly string _sortMemberPath;
        private readonly char _characterToSplitOn;

        public GamePerformanceItemSorter(string sortMemberPath, ListSortDirection direction)
        {
            _direction = direction;
            _sortMemberPath = sortMemberPath;
            _characterToSplitOn = '.';
        }

        public int Compare(object x, object y)
        {
            if (x == null)
            {
                return 0;
            }

            var retVal = 0;
            var t = x.GetType();
            PropertyInfo sortProperty;
            object context1 = x;
            object context2 = y;

            if (_sortMemberPath.Contains(nameof(GamePerformanceData.TheoreticalRtp)))
            {
                var tupleItemIndex = _sortMemberPath.Split(_characterToSplitOn)[1];
                var theoreticalRtpProperty = t.GetProperty(nameof(GamePerformanceData.TheoreticalRtp));

                sortProperty = theoreticalRtpProperty?.GetValue(x).GetType().GetProperty(tupleItemIndex);
                context2 = theoreticalRtpProperty?.GetValue(x);
                context1 = theoreticalRtpProperty?.GetValue(y);
            }
            else
            {
                sortProperty = t.GetProperty(_sortMemberPath);
            }

            if (sortProperty != null && x is GamePerformanceData xData && y is GamePerformanceData yData)
            {
                var activeCompare = xData.ActiveState.CompareTo(yData.ActiveState);
                var xValue = sortProperty.GetValue(context1);
                var yValue = sortProperty.GetValue(context2);

                if (activeCompare == 0 && xValue is IComparable xComp && yValue is IComparable yComp)
                {
                    if (double.TryParse(xValue.ToString(), out var xNumber) && double.TryParse(yValue.ToString(), out var yNumber))
                    {
                        retVal = _direction == ListSortDirection.Ascending
                            ? xNumber.CompareTo(yNumber)
                            : yNumber.CompareTo(xNumber);
                    }
                    else
                    {
                        retVal = _direction == ListSortDirection.Ascending
                            ? xComp.CompareTo(yComp)
                            : yComp.CompareTo(xComp);
                    }
                }
                else
                {
                    retVal = activeCompare;
                }

                if (retVal == 0)
                {
                    retVal = string.Compare(xData.GameName, yData.GameName, StringComparison.Ordinal);
                    if (retVal == 0)
                    {
                        retVal = xData.GameNumber.CompareTo(yData.GameNumber);
                    }
                }
            }

            return retVal;
        }
    }
}