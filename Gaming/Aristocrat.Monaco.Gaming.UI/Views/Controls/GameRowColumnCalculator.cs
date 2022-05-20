namespace Aristocrat.Monaco.Gaming.UI.Views.Controls
{
    public static class GameRowColumnCalculator
    {
        public static (int Rows, int Cols) ExtraLargeIconRowColCount { get; } = (1, 2);

        public static (int Rows, int Cols) CalculateRowColCount(int childCount)
        {
            var rows = childCount > 21 ? 4 : childCount > 8 ? 3 : childCount > 4 ? 2 : 1;
            var cols = childCount > 0 ? childCount > 8 ? (childCount + (rows - 1)) / rows : 4 : 0;

            return (rows, cols);
        }
    }
}