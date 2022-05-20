namespace Generator.Utils
{
    using Common.Utils;
    using Common.Models;
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows.Data;

    public class BoolToStringConverter : BoolToValueConverter<String> { };
    public class BoolToColorConverter : BoolToValueConverter<String> { };
    public class BoolToVisibilityConverter : BoolToValueConverter<String> { };
    public class BoolToBoolConverter : BoolToValueConverter<Boolean> { };
    public class BoolToValueConverter<T> : IValueConverter
    {
        public T FalseValue { get; set; }
        public T TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return FalseValue;
            else
                return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? value.Equals(TrueValue) : false;
        }
    }

    public class DoubleToColorConverter : DoubleToValueConverter<String> { };
    public class DoubleToValueConverter<T> : IValueConverter
    {
        public T BadValue { get; set; }
        public T GoodValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val = (double)value;

            if (val < Validator.MinSizeGB)
                return BadValue;
            else
                return GoodValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CollToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<USBKey> keys = (ObservableCollection<USBKey>)value;
            if (keys.Count == 0)
                return "Visible";
            return "Hidden";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
