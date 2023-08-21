namespace Aristocrat.Monaco.Application.UI.LampTest
{
    using Contracts.LampTest;
    using Hardware.Contracts.ButtonDeck;

    public static class LampTestUtilities
    {
        public static ILampTest GetLampTest()
        {
            switch (ButtonDeckUtilities.GetButtonDeckType())
            {
                case ButtonDeckUtilities.ButtonDeckType.LCD:
                    return new LCDLampTest();
                case ButtonDeckUtilities.ButtonDeckType.Virtual:
                    return new LampTest();
                case ButtonDeckUtilities.ButtonDeckType.PhysicalButtonDeck:
                    return new LampTest();
            }
            return null;
        }
    }
}
