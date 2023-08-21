namespace Aristocrat.Monaco.Hardware.EdgeLight.Contracts
{
    /// <summary>
    ///     Different BoardIDs that are supported by the firmware
    /// </summary>
    public enum BoardIds : uint
    {
        MainBoardId = 0x1100,
        TopperBoardId = 0x1200, //Helix topper.
        BehemothBoardId = 0x1300,
        UsHelixXtBoardId = 0x1310, //US Helix XT
        StepperBoardId = 0x1400,
        RectangularTopperBoardId = 0x1600, //Rectangular, non-Helix toppers. Exposes 4 strips, one on each side.
        HaloBoardId = 0X1700, // Halo light of bash button on Flame's VBD
        FasciaTopperBoardId = 0x1800,
        InvalidBoardId = 0xFFFF
    }
}