namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Contracts;

    public class TimeLimitDialogVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var retVal = Visibility.Collapsed;

            if (value is ResponsibleGamingDialogState state && parameter is string param)
            {
                var command = param.ToUpperInvariant();

                if (command == "AlcTwoButton".ToUpperInvariant())
                {
                    if (state == ResponsibleGamingDialogState.TimeInfo)
                    {
                        retVal = Visibility.Visible;
                    }
                }
                else if (command == "AlcOneButton".ToUpperInvariant())
                {
                    if (state == ResponsibleGamingDialogState.TimeInfoLastWarning ||
                        state == ResponsibleGamingDialogState.SeeionEndForceCashOut)
                    {
                        retVal = Visibility.Visible;
                    }
                }
                else if (command == "TimeoutButtons".ToUpperInvariant())
                {
                    if (state == ResponsibleGamingDialogState.Initial ||
                        state == ResponsibleGamingDialogState.ChooseTime)
                    {
                        retVal = Visibility.Visible;
                    }
                }
                else if (command == "ManitobaPlayBreak".ToUpperInvariant())
                {
                    if (state == ResponsibleGamingDialogState.PlayBreak1 ||
                        state == ResponsibleGamingDialogState.PlayBreak2)
                    {
                        retVal = Visibility.Visible;
                    }
                }
                else if (command == "StandardDialog".ToUpperInvariant())
                {
                    if (state == ResponsibleGamingDialogState.Initial ||
                        state == ResponsibleGamingDialogState.ChooseTime ||
                        state == ResponsibleGamingDialogState.ForceCashOut)
                    {
                        retVal = Visibility.Visible;
                    }
                }
                else if (command == "WelcomeCashOut".ToUpperInvariant())
                {
                    if (state == ResponsibleGamingDialogState.Initial ||
                        state == ResponsibleGamingDialogState.ForceCashOut)
                    {
                        retVal = Visibility.Visible;
                    }
                }
                else if (command == state.ToString().ToUpperInvariant())
                {
                    retVal = Visibility.Visible;
                }
            }

            return retVal;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
