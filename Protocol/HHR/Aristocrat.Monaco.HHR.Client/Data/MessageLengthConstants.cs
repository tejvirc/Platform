namespace Aristocrat.Monaco.Hhr.Client.Data
{
    /// <summary>
    ///     Message struct array length constants
    /// </summary>
    public static class MessageLengthConstants
    {
        /// <summary />
        public const int TypeLength = 22;

        /// <summary />
        public const int SerialNumberLength = 22;

        /// <summary />
        public const int LocationLength = 22;

        /// <summary />
        public const int KeyLength = 8;

        /// <summary />
        public const int ProgressiveLength = 21;

        /// <summary />
        public const int VersionLength = 13;

        /// <summary />
        public const int PhoneLength = 22;

        /// <summary />
        public const int DeviceLength = 22;

        /// <summary />
        public const int SiteLength = 32;

        /// <summary />
        public const int NameLength = 22;

        /// <summary />
        public const int MessageLength = 256;

        /// <summary />
        public const int TicketTitleLength = 18;

        /// <summary />
        public const int TicketTypeLength = 22;

        /// <summary />
        public const int FileNameLength = 22;

        /// <summary />
        public const int SignatureKeyLength = 148;

        /// <summary />
        public const int AddressLength = 32;

        /// <summary />
        public const int MaxGtPromos = 5;

        /// <summary />
        public const int MaxGtTournaments = 5;

        /// <summary />
        public const int StateLength = 4;

        /// <summary />
        public const int CityLength = 32;

        /// <summary />
        public const int MaxGtGames = 12;

        /// <summary />
        public const int ReservedLength = 21;

        /// <summary />
        public const int ProgramLength = 21;

        /// <summary />
        public const int UdpIpLength = 16;

        /// <summary />
        public const int DllLength = 22;

        /// <summary />
        public const int MaxGameIndex = 120;

        /// <summary />
        public const int NumBetLevels = 2;

        /// <summary />
        public const int MaxNumProgs = 50;

        /// <summary />
        public const int GameTriggerLength = 560;

        /// <summary />
        public const int WagerLength = 20;

        /// <summary />
        public const int EncMessagesLength = 256;

        /// <summary />
        public const int EncTicketsLength = 256;

        /// <summary />
        public const int RaceInformationLength = 50;

        /// <summary />
        public const int PrizeLength = 100;

        /// <summary />
        public const int MaxNumPatterns = 3000;

        /// <summary />
        public const int MaxNumTickets = 25;

        /// <summary />
        public const int PlayerIdLength = 13;

        /// <summary />
        public const int PinLength = 10;

        /// <summary />
        public const int ReasonLength = 256;

        /// <summary />
        public const int NumRaceData = 10;

        /// <summary />
        public const int NumRaceStat = 3;

        /// <summary />
        public const int DescriptionLength = 50;

        /// <summary />
        public const int RaceDateLength = 50;

        /// <summary />
        public const int HorseStringLength = 13;

        /// <summary />
        public const int NumStats = 13;

        /// <summary />
        public const int PlayerClubLength = 81;

        /// <summary />
        public const int NumTemplatePool = 25;

        /// <summary />
        public const int NumPrizeDataRace = 3000;

        /// <summary />
        public const int PatternSetNormLength = (MaxGameIndex + 1) * NumBetLevels;

        /// <summary />
        public const int TournamentDescLength = MaxGtTournaments * NameLength;
    }
}