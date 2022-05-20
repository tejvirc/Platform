using System;
using System.Globalization;
using System.Windows.Data;
using System.Linq;
using System.Text;
using Aristocrat.Monaco.Application.Contracts.Localization;

namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using Localization.Properties;

    /// <summary>
    ///     Converter used to format Hmac Hash strings.
    /// </summary>
    public class HmacDisplayBlockSizeConverter : IValueConverter
    {
        private readonly char _characterToSplitOn = ' ';
        private int _blockSizeFormat = 10;
        private readonly string _seed = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Seed).ToUpper();
        private readonly string _result = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Result).ToUpper();
        private readonly string _notApplicable = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable).ToUpper();

        /// <summary>
        ///     Convert the Hmac String into a format that fits in the LogDetailsViewModel popup box and looks visually appealing.
        /// </summary>
        /// <param name="value">Tuple, of information to populate view (Item1, Item2)</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">not used</param>
        /// <param name="culture">not used</param>
        /// <returns>A formatted Hmac Hash value that is split into lines that contain _blockSizeFormat blocks</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null)
            {
                return "";
            }

            var inputValue = (Tuple<string, string>)value;
            if (!inputValue.Item1.Equals(_seed) && !inputValue.Item1.Equals(_result) || inputValue.Item2.Equals(_notApplicable))
            {
                return inputValue.Item2;
            }

            var hash = inputValue.Item2.Split(_characterToSplitOn);
            StringBuilder formattedHashValue = new StringBuilder();

            // Split the string into 10 block lines
            for (var i = 0; i < hash.Length / _blockSizeFormat; ++i)
            {
                formattedHashValue.Append(string.Join(_characterToSplitOn.ToString(), hash.Skip(i * _blockSizeFormat).Take(_blockSizeFormat)) + "\n");
            }

            // Get the remainder
            var charactersPerBlockIncludingSpace = 5; 
            var startingDifference = inputValue.Item2.Length - (hash.Length % _blockSizeFormat - 1) * charactersPerBlockIncludingSpace;
            for (var i = startingDifference; i < inputValue.Item2.Length; ++i)
            {
                formattedHashValue.Append(inputValue.Item2[i]);
            }

            return formattedHashValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
