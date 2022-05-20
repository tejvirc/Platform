namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Binary structure for HHR message of type CMD_HANDPAY_CREATE
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GMessageCreateHandPayItem
    {
        /// <summary />
        public uint TransactionId;

        /// <summary />
        public uint HandpayType;

        /// <summary />
        public uint Amount;

        /// <summary />
        public uint Denomination;

        /// <summary />
        public uint GameWin;

        /// <summary />
        public uint BonusWin;

        /// <summary />
        public uint ProgWin;

        /// <summary />
        public uint MachinePaid;

        /// <summary />
        public uint LastWager;

        /// <summary />
        public uint MaxBetValue;

        /// <summary />
        public byte CreditMeterKeyOff;

        /// <summary />
        public byte VoucherKeyOff;

        /// <summary />
        public byte RemoteKeyOff;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.PlayerIdLength)]
        public string PlayerId;

        /// <summary />
        public uint GameMapId;

        /// <summary />
        public uint LastGamePlayTime;
    }
}