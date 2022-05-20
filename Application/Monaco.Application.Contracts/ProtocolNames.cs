// ReSharper disable InconsistentNaming
namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Common;

    /// <summary>
    /// A list of protocols
    /// </summary>
    public static class ProtocolNames
    {
        /// <summary>
        /// List of all available protocols
        /// </summary>
        public static string[] All = Enum.GetNames(typeof(CommsProtocol));

        /// <summary>
        /// Represents no protocol
        /// </summary>
        public static string None = EnumParser.ToName(CommsProtocol.None);

        /// <summary>
        /// Represents the ASP1000 protocol
        /// </summary>
        public static string Asp1000 = EnumParser.ToName(CommsProtocol.ASP1000);

        /// <summary>
        /// Represents the ASP2000 protocol
        /// </summary>
        public static string Asp2000 = EnumParser.ToName(CommsProtocol.ASP2000);

        /// <summary>
        /// Represents the Bingio protocol
        /// </summary>
        public static string Bingo = EnumParser.ToName(CommsProtocol.Bingo);

        /// <summary>
        /// Represents the DACOM protocol
        /// </summary>
        public static string DACOM = EnumParser.ToName(CommsProtocol.DACOM);

        /// <summary>
        /// Represents the DemonstrationMode protocol
        /// </summary>
        public static string DemonstrationMode = EnumParser.ToName(CommsProtocol.DemonstrationMode);

        /// <summary>
        /// Represents the G2S protocol
        /// </summary>
        public static string G2S = EnumParser.ToName(CommsProtocol.G2S);

        /// <summary>
        /// Represents the HHR protocol
        /// </summary>
        public static string HHR = EnumParser.ToName(CommsProtocol.HHR);

        /// <summary>
        /// Represents the MGAM/NYL Host protocol
        /// </summary>
        public static string MGAM = EnumParser.ToName(CommsProtocol.MGAM);

        /// <summary>
        /// Represents the SAS protocol
        /// </summary>
        public static string SAS = EnumParser.ToName(CommsProtocol.SAS);

        /// <summary>
        /// Represents the Test protocol
        /// </summary>
        public static string Test = EnumParser.ToName(CommsProtocol.Test);
    }
}