namespace Aristocrat.Monaco.Hardware.SerialTouch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class InvalidPacketException : Exception
    {
        private const string DefaultMessage = "Invalid byte for partial packet.";
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidPacketException" /> class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        public InvalidPacketException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidPacketException" /> class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="nextByte">The next byte that failed to be added to the current packet under construction.</param>
        /// <param name="packetUnderConstruction">The current packet under constructed.</param>
        public InvalidPacketException(byte nextByte, IEnumerable<byte> packetUnderConstruction)
            : this (DefaultMessage, nextByte, packetUnderConstruction)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidPacketException" /> class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="nextByte">The next byte that failed to be added to the current packet under construction.</param>
        /// <param name="packetUnderConstruction">The current packet under constructed.</param>
        public InvalidPacketException(string message, byte nextByte, IEnumerable<byte> packetUnderConstruction)
            : base (message)
        {
            NextByte = nextByte;
            PacketUnderConstruction = packetUnderConstruction.ToArray();
        }
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidPacketException" /> class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public InvalidPacketException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidPacketException" /> class and initializes
        ///     the contained message and inner exception reference.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public InvalidPacketException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidPacketException" /> class with serialized data.
        /// </summary>
        /// <param name="info">Information on how to serialize a TransactionException.</param>
        /// <param name="context">Information on the streaming context for a TransactionException.</param>
        protected InvalidPacketException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        /// <summary>
        ///     Gets the next byte that failed to be added to the current packet under construction.
        /// </summary>
        public byte NextByte { get; }
        
        /// <summary>
        ///     Gets current packet under constructed.
        /// </summary>
        public byte[] PacketUnderConstruction { get; }
    }
}
