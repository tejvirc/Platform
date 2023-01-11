namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System.Windows.Forms;
    using Aristocrat.Cabinet.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Cabinet;
    using Kernel;

    internal static class ScaleUtility
    {
        internal const double BaseScreenWidth = 1920;
        internal const double BaseScreenHeight = 1080;

        // If the game icon passes this height, we need to shift the denom panel up a bit
        internal const double GameIconHeightThreshold = 376;

        internal static double GetScale()
        {
            var cabinetDetectionService = ServiceManager.GetInstance().GetService<ICabinetDetectionService>();
            if (cabinetDetectionService is null || cabinetDetectionService.Type == CabinetType.Unknown)
            {
                return 1;
            }

            return Screen.PrimaryScreen.Bounds.Width / BaseScreenWidth;
        }
    }
}
