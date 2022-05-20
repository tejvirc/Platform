namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Binary structure for HHR message of type CMD_TRANSACTION
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct MessageTransaction
    {
        /// <summary />
        public uint TransactionId;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.PlayerIdLength)]
        public string PlayerId;

        /// <summary />
        public byte TransType;

        /// <summary />
        public uint Credit;

        /// <summary />
        public uint Debit;

        /// <summary />
        public uint CashBalance;

        /// <summary />
        public uint NonCashBalance;

        /// <summary />
        public uint GameMapId;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.PlayerClubLength)]
        public string PTAccountId;

        /// <summary />
        public uint Flags;

        /// <summary />
        public uint LinesPlayed;

        /// <summary />
        public uint BetPerLine;

        /// <summary />
        public uint TotalCreditsBet;

        /// <summary />
        public uint Denomination;

        /// <summary />
        public uint PriorCashBalance;

        /// <summary />
        public uint PriorNonCashBalance;

        /// <summary />
        public uint HandPayType;

        /// <summary />
        public uint LastGamePlayTime;
    }
}