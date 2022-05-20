namespace Aristocrat.Monaco.Hhr.UI.Converters
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// This will take the odds of winning as integer and will return the
    /// observable collection of all the integers less than equal to odds
    /// for example if winning odds is n then this convertor will return collection of integers like
    /// 1,2,3,4,5,6...n so that we can use the related image in the Stat's Chart.
    /// </summary>

    public class WinningOddsToCollection : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<int> winningOddCollection = null;

            if (value != null && value is int winningOddNumber)
            {
                winningOddCollection = new ObservableCollection<int>();
                for (var index = 1; index <= winningOddNumber; index++)
                {
                    winningOddCollection.Add(index);
                }
            }

            return winningOddCollection;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}