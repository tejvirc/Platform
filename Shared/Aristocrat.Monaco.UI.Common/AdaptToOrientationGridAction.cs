namespace Aristocrat.Monaco.UI.Common
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interactivity;
    using Application.Contracts.Media;

    /// <summary>
    ///     Resizes the first row of the Grid to 0 if the containing Window has a Landscape aspect-ratio.
    ///     Otherwise, the Row height is set to {FirstRowHeightWhenPortrait} when Window has a Portrait aspect-ratio.
    /// </summary>
    /// <seealso cref="TriggerAction{Grid}" />
    public class AdaptToOrientationGridAction : TriggerAction<Grid>
    {
        /// <summary>
        /// The DependencyProperty for FirstRowHeightWhenPortrait
        /// </summary>
        public static readonly DependencyProperty FirstRowHeightWhenPortraitProperty = DependencyProperty.Register(
            nameof(FirstRowHeightWhenPortrait),
            typeof(GridLength),
            typeof(AdaptToOrientationGridAction),
            new PropertyMetadata(default(GridLength)));

        /// <summary>
        ///     Gets or sets the Height of the Grid first Row, when in portrait mode.
        /// </summary>
        /// <value>The height for the first Row.</value>
        public GridLength FirstRowHeightWhenPortrait
        {
            get => (GridLength)GetValue(FirstRowHeightWhenPortraitProperty);
            set => SetValue(FirstRowHeightWhenPortraitProperty, value);
        }

        /// <inheritdoc />
        protected override void Invoke(object parameter)
        {
            if (AssociatedObject.RowDefinitions.Any())
            {
                AssociatedObject.RowDefinitions.First().Height = GetIsPortrait()
                    ? FirstRowHeightWhenPortrait
                    : new GridLength(0);
            }
        }

        private bool GetIsPortrait()
        {
            var window = TreeHelper.FindParent<Window>(AssociatedObject);
            return window?.IsPortrait() ?? false;
        }
    }
}