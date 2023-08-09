namespace Aristocrat.Monaco.UI.Common.Models
{
    using System;
    using CommunityToolkit.Mvvm.ComponentModel;

    /// <summary>
    ///     An string object used to filter a DataGrid column
    /// </summary>
    [CLSCompliant(false)]
    public class FilterObject : ObservableObject
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
