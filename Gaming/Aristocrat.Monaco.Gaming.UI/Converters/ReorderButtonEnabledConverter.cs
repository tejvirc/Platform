namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using Contracts;
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using ViewModels.OperatorMenu;

    public class ReorderButtonEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return true;
            }

            var gameDetail = values[0] as IGameDetail;
            var games = values[1] as ObservableCollection<GameLayoutViewModel.GameLayoutItem>;

            if (gameDetail == null || games == null)
            {
                return true;
            }

            var game = games.FirstOrDefault(g => g.GameDetail == gameDetail);
            if (game == null)
            {
                return true;
            }

            var gameCount = games.Count;
            var index = games.IndexOf(game);

            switch (parameter.ToString())
            {
                case "Left":
                    return index != 0;
                case "Right":
                    return index < gameCount - 1;
            }

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
