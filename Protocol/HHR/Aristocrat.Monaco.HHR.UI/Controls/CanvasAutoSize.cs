namespace Aristocrat.Monaco.Hhr.UI.Controls
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    public class CanvasAutoSize : Canvas
    {
        protected override Size MeasureOverride(Size constraint)
        {
            base.MeasureOverride(constraint);
            double width = 
                InternalChildren
                .OfType<UIElement>()
                .Max(i => i.DesiredSize.Width + (double)i.GetValue(Canvas.LeftProperty));

            double height =
                InternalChildren
                .OfType<UIElement>()
                .Max(i => i.DesiredSize.Height + (double)i.GetValue(Canvas.TopProperty));

            return new Size(width, height);
        }
    }
}
