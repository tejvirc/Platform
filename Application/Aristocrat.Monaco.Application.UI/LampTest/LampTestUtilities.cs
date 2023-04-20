namespace Aristocrat.Monaco.Application.UI.LampTest
{
    using Contracts.LampTest;
    using Hardware.Contracts.ButtonDeck;
    using Hardware.Contracts.Cabinet;
    using Kernel;

    public static class LampTestUtilities
    {
        public static ILampTest GetLampTest()
        {
            var serviceManager = ServiceManager.GetInstance();
            var properties = serviceManager.GetService<IPropertiesManager>();
            var cabinet = serviceManager.GetService<ICabinetDetectionService>();
            return cabinet.GetButtonDeckType(properties) switch
            {
                ButtonDeckType.LCD => new LCDLampTest(),
                ButtonDeckType.Virtual => new LampTest(),
                ButtonDeckType.PhysicalButtonDeck => new LampTest(),
                _ => null
            };
        }
    }
}
