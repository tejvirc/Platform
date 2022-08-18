namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows;
    using System.Windows.Forms;

    public class GameIconImageMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GameGridMarginInputs inputs && !inputs.ExtraLargeIconLayout)
            {
                if (Screen.PrimaryScreen.Bounds.Height > GameIconImageHeightConverter.BaseScreenHeight)
                {
                    // Unfortunately the game icon is not created properly, some icons have some blank spaces above and below
                    // the images, so we have to move the image up a bit to display it in correct place.
                    double topMarginOffset = -24;
                    if (inputs.GameCount > 8)
                    {
                        return new Thickness(0, topMarginOffset, 0, 0);
                    }
                }
            }
            

            return new Thickness();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
