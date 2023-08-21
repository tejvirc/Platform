namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Binary structure for HHR message of type CMD_RACE_PARI_REQ
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GMessageRacePariRequest
    {
        /// <summary />
        public uint GameId;

        /// <summary />
        public uint CreditsPlayed;

        /// <summary />
        public uint LinesPlayed;
    }
}