namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Binary structure for generic HHR message header. This header wraps all messages
    ///     to and from the server, including broadcasts.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct MessageHeader
    {
        // NOTE: We do not include the EncMsgHdr as that is not on all messages. That has to be processed
        // and discarded before we marshal the remaining bytes into message structs.

        /// <summary />
        public uint Command;

        /// <summary />
        public uint DeviceId;

        /// <summary />
        public uint Version;

        /// <summary />
        public int Length;

        /// <summary />
        public ushort Retries;

        /// <summary />
        public uint Sequence;

        /// <summary />
        public uint MessageId;

        /// <summary />
        public uint ReplyId;

        /// <summary />
        public int Time;
    }
}