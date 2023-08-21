namespace Aristocrat.Monaco.G2S.UI.Views
{
    using System.Windows.Controls;

    /// <summary>
    ///     Represents a view mode that displays data items in columns for a System.Windows.Controls.ListView control
    ///     with auto sized columns based on the column content
    /// </summary>
    public class AutoSizedGridView : GridView
    {
        /// <inheritdoc cref="GridView" />
        protected override void PrepareItem(ListViewItem item)
        {
            foreach (var column in Columns)
            {
                // Setting NaN for the column width automatically determines the required
                // width to hold the content completely.

                // If the width is NaN, first set it to ActualWidth temporarily.
                if (double.IsNaN(column.Width))
                {
                    column.Width = column.ActualWidth;
                }

                // Finally, set the column width to NaN. This raises the property change
                // event and re computes the width.
                column.Width = double.NaN;
            }

            base.PrepareItem(item);
        }
    }
}