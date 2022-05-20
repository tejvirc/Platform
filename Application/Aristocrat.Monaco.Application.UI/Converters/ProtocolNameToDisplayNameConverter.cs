namespace Aristocrat.Monaco.Application.UI.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using Models;

    /// <summary>
    ///     Converts protocol name(s) to display name(s) and back.
    ///     Allows hiding protocol specifics and presenting a more friendly name.
    /// </summary>
    public class ProtocolNameToDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case null:
                    return null;
                case IList<CommsProtocol> list:
                    return list.Select(s => ProtocolNameToDisplayNameMapper.ToDisplayName(s.ToString())).ToList();
                default:
                    return ProtocolNameToDisplayNameMapper.ToDisplayName(value.ToString());
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case null:
                    return null;
                case IList<CommsProtocol> list:
                    return list.Select(s => ProtocolNameToDisplayNameMapper.ToProtocolName(s.ToString())).ToList();
                default:
                    return ProtocolNameToDisplayNameMapper.ToProtocolName(value.ToString());
            }
        }
    }
}
