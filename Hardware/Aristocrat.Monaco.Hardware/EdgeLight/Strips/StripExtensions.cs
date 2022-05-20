namespace Aristocrat.Monaco.Hardware.EdgeLight.Strips
{
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;

    /// <summary>
    ///     Utility Class to store strip id to role association.
    /// </summary>
    public static class StripExtensions
    {
        //0x1300XX is for behemoth, arc, flame
        //0x1310XX is for HelixXT, Edge-X and Bartop
        //0x1900XX 55” Signage Topper
        //0x1200XX All rectangular toppers
        //0x1600XX GS/Edge-X Topper
        //0x1700XX Button halo
        private static readonly List<int> MainBoardIds = new List<int>() { 0x110000, 0x131000, 0x130000 };
        private static readonly List<int> TopperBoardIds = new List<int>() { 0x120000, 0x160000, 0x190000 };
        private static readonly List<int> VbdBoardIds = new List<int>() { 0x170000 };

        private static readonly IReadOnlyDictionary<ElDeviceType, IReadOnlyCollection<int>> ElTypeToBoardIDictionary =
            new Dictionary<ElDeviceType, IReadOnlyCollection<int>>
            {
                { ElDeviceType.Cabinet, MainBoardIds },
                { ElDeviceType.Topper, TopperBoardIds },
                { ElDeviceType.VBD, VbdBoardIds }
            };

        private static readonly IReadOnlyDictionary<ElDeviceType, IReadOnlyCollection<StripIDs>> StripRoleDictionary =
            new Dictionary<ElDeviceType, IReadOnlyCollection<StripIDs>>
            {
                {
                    ElDeviceType.Cabinet,
                    new List<StripIDs>
                    {
                        StripIDs.MainCabinetLeft,
                        StripIDs.MainCabinetRight,
                        StripIDs.MainCabinetBottom,
                        StripIDs.MainCabinetTop,
                        StripIDs.SaddleStrip,
                        StripIDs.BarkeeperStrip4Led,
                        StripIDs.BarkeeperStrip1Led,
                        StripIDs.EdgeXLeftSoundLed,
                        StripIDs.EdgeXRightSoundLed
                    }
                },
                {
                    ElDeviceType.Topper,
                    new List<StripIDs>
                    {
                        StripIDs.NormalTopperLeft,
                        StripIDs.NormalTopperRight,
                        StripIDs.NormalTopperBottom,
                        StripIDs.NormalTopperTop,
                        StripIDs.StaticTopperLeft,
                        StripIDs.StaticTopperRight,
                        StripIDs.StaticTopperBottom,
                        StripIDs.StaticTopperTop
                    }
                },
                {
                    ElDeviceType.VBD,
                    new List<StripIDs>
                    {
                        StripIDs.VbdLeftStrip,
                        StripIDs.VbdRightStrip,
                        StripIDs.VbdBottomStrip,
                        StripIDs.RightHaloStrip,
                        StripIDs.LeftHaloStrip,
                        StripIDs.LandingStripLeft,
                        StripIDs.LandingStripRight
                    }
                }
            };

        public static ElDeviceType GetDeviceType(this IStrip strip, bool gen9Board)
        {
            return gen9Board ? 
                StripRoleDictionary.Where(x => x.Value.Any(y => (int)y == strip.FirmwareId()))
                    .Select(x => x.Key)
                    .Distinct().FirstOrDefault():
                ElTypeToBoardIDictionary.Where(x => x.Value.Any(y => y == strip.GetBoardId()))
                .Select(x => x.Key)
                .Distinct().FirstOrDefault();
        }

        public static int FirmwareId(this IStrip strip)
        {
            const int firmwareMask = 0xFF;
            return strip.StripId & firmwareMask;
        }

        public static bool IsBarKeeper(this IStrip strip)
        {
            return strip.FirmwareId() == (int)StripIDs.BarkeeperStrip1Led ||
                   strip.FirmwareId() == (int)StripIDs.BarkeeperStrip4Led;
        }

        private static int GetBoardId(this IStrip strip)
        {
            const int boardMask = 0xFFFF00;
            return (strip.StripId & boardMask);
        }
    }
}