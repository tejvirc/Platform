namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows.Data;
    using Models;

    /// <summary>
    ///     We need to determine in xaml whether a game in the lobby should have
    ///     the "attract highlight."
    /// </summary>
    public class AttractActiveToVisibilityConverter : IMultiValueConverter
    {
        /// <summary>
        ///     Convert to visible
        /// </summary>
        /// <param name="values">values to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>whether the attract feature is visible or not</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = values[0] as ObservableCollection<GameInfo>;
            var element = values[1] as GameInfo;
            var currentIndex = values[2] as int? ?? 0;
            var isTopAttractFeatureVisible = values[3] is bool && (bool)values[3];

            var elementIndex = collection != null && element != null ? collection.IndexOf(element) : 0;

            return elementIndex == currentIndex && isTopAttractFeatureVisible;
        }

        /// <summary>
        ///     Converts back to visible
        /// </summary>
        /// <param name="value">not used</param>
        /// <param name="targetTypes">also not used</param>
        /// <param name="parameter">still not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>nothing since it throws an exception</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}