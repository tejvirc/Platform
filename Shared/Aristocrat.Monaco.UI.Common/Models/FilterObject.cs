namespace Aristocrat.Monaco.UI.Common.Models
{
    using System;
    using MVVM.Model;

    /// <summary>
    ///     An string object used to filter a DataGrid column
    /// </summary>
    [CLSCompliant(false)]
    public class FilterObject : BaseNotify
    {
        private bool _filterIsChecked;

        /// <summary>
        ///     FilterObject
        /// </summary>
        public FilterObject(string filterName)
        {
            Name = filterName;
            _filterIsChecked = true;
        }

        /// <summary>
        ///     The string representation of the filter
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Whether the filter is checked
        /// </summary>
        public bool IsChecked
        {
            get => _filterIsChecked;
            set => SetProperty(ref _filterIsChecked, value);
        }
    }
}
