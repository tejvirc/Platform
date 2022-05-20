namespace Aristocrat.Monaco.Hhr.UI.Converters
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    /// <summary>
    /// This will convert byte to array of bool for example
    /// 0-> False False False False False False False False;
    /// 255-> True True True True True True True True
    /// </summary>

    public class ByteToBitsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var patternBits = new ObservableCollection<bool>();

            foreach (var pattern in (byte[])value)
            {
                new BitArray(new[] { pattern }).Cast<bool>().Select(x => x).ToList().ForEach(delegate (bool patternBit)
                {
                    patternBits.Add(patternBit);
                });
            }
            return patternBits;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}