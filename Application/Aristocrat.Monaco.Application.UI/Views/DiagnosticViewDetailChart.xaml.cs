using OxyPlot;

namespace Aristocrat.Monaco.Application.UI.Views
{
    /// <summary>
    /// Interaction logic for DiagnosticViewDetailChart.xaml
    /// </summary>
    public partial class DiagnosticViewDetailChart
    {
        public DiagnosticViewDetailChart()
        {
            InitializeComponent();

            var myController = new PlotController();

            MonacoPlotView.Controller = myController;

            //Need to unbind the zooming and panning by touch before binding it back
            myController.UnbindTouchDown();

            myController.BindTouchDown(PlotCommands.PanZoomByTouch);
        }
    }
}
