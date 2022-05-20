namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using Monaco.UI.Common;

    /// <summary>
    ///     Takes multiple parameters to determine if a bright border should
    ///     encircle the selected game
    /// </summary>
    internal class CharacterToResourceImageConverter : IValueConverter
    {
        private static readonly ResourceDictionary CommonDictionary = SkinLoader.Load("CommonUI.xaml");
        private readonly Image _blankImage = new Image();
        private const string EuroSymbol = "€";
        private const string EuroText = "Euro";
        private const string SpaceSymbol = " ";
        private const string SpaceText = "Space";

        private string CharacterResourceId(string prefix, string character) => prefix + "Char" + character;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return _blankImage;
            }

            var prefix = (string)parameter;
            var character = ((char)value).ToString();
            string resourceId;

            switch (character)
            {
                case EuroSymbol:
                    resourceId = CharacterResourceId(prefix, EuroText);
                    break;
                case SpaceSymbol:
                    resourceId = CharacterResourceId(prefix, SpaceText);
                    break;
                default:
                    resourceId = CharacterResourceId(prefix, character);
                    break;
            }

            return CommonDictionary[resourceId];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}