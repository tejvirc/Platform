namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Data;
    using log4net;
    using Protocol.Common.Logging;

    /// <summary>
    ///     A collection of utility functions that are used for serialization of messages.
    /// </summary>
    public static class MessageUtility
    {
        private static readonly ILog ProtoLog = LogManager.GetLogger("Protocol");

        private static readonly int MsgEncHdrSize = Marshal.SizeOf<MessageEncryptHeader>();
        private static readonly int MsgHdrSize = Marshal.SizeOf<MessageHeader>();

        /// <summary>
        ///     Get message of a given type from byte[]
        /// </summary>
        /// <param name="byteData">Raw data to Marshal into a given type</param>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <returns>Marshaled Type from byte[]</returns>
        public static TMessage GetMessage<TMessage>(byte[] byteData) where TMessage : struct
        {
            var startPos = GetMessageStartIndex(new TMessage());
            var size = Marshal.SizeOf(typeof(TMessage));
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(byteData, startPos, ptr, size);
            var encryptedHeader = Marshal.PtrToStructure<TMessage>(ptr);
            Marshal.FreeHGlobal(ptr);
            return encryptedHeader;
        }

        /// <summary>
        ///     Sets a given MessageType into given buffer
        /// </summary>
        /// <param name="bytes">Buffer where a given message needs to set.</param>
        /// <param name="message">Message which needs to setup into buffer</param>
        /// <typeparam name="TMessage">Type of Message</typeparam>
        public static void SetMessage<TMessage>(byte[] bytes, TMessage message) where TMessage : struct
        {
            var size = Marshal.SizeOf<TMessage>();
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(message, ptr, true);
            Marshal.Copy(ptr, bytes, GetMessageStartIndex(new TMessage()), size);
            Marshal.FreeHGlobal(ptr);
        }

        /// <summary>
        ///     Wraps message bytes with another message and returns the resulting bytes.
        /// </summary>
        /// <param name="bytes">Buffer which needs to be wrapped by the message.</param>
        /// <param name="message">Message which needs to be prepended.</param>
        /// <typeparam name="TMessage">Type of message with which to wrap the bytes.</typeparam>
        /// <returns>New buffer which includes message as a header and the bytes appended after.</returns>
        public static byte[] WrapBytesWithMessage<TMessage>(byte[] bytes, TMessage message) where TMessage : struct
        {
            var size = Marshal.SizeOf<TMessage>();
            var newMessage = new byte[size + bytes.Length];

            var messageBytes = ConvertMessageToByteArray(message);
            Array.Copy(messageBytes, 0, newMessage, 0, messageBytes.Length);
            Array.Copy(bytes, 0, newMessage, messageBytes.Length, bytes.Length);

            return newMessage;
        }

        /// <summary>
        ///     Removes Encrypted header from given Buffer and returns buffer next to Header.
        /// </summary>
        /// <param name="data">Input buffer</param>
        /// <returns>Buffer - EncryptedHeader</returns>
        public static byte[] ExtractEncryptedHeader(byte[] data)
        {
            var bytes = new byte[data.Length - MsgEncHdrSize];
            Array.Copy(data, MsgEncHdrSize, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        ///     Removes message header from given Buffer and returns buffer next to Header.
        /// </summary>
        /// <param name="data">Input buffer</param>
        /// <returns>Buffer - EncryptedHeader</returns>
        public static byte[] ExtractMessageHeader(byte[] data)
        {
            var bytes = new byte[data.Length - MsgHdrSize];
            Array.Copy(data, MsgHdrSize, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        ///     Populates MessageHeader and appends it into command data.
        /// </summary>
        /// <param name="request">Request type that is being populated</param>
        /// <param name="commandData">Command data buffer.</param>
        /// <returns>Data buffer with Header + Command</returns>
        public static byte[] PopulateMessageHeader(Request request, byte[] commandData)
        {
            var header = new MessageHeader
            {
                DeviceId = ClientProperties.ParameterDeviceId,
                Command = (uint)request.Command,
                Version = ClientProperties.CommandVersion,
                Retries = HhrConstants.RetryCount,
                ReplyId = 0,
                Sequence = request.SequenceId,
                MessageId = request.SequenceId,
                Length = MsgHdrSize + commandData.Length,
                Time = (int)GetTimeUnix()
            };

            ProtoLog.Debug($"[SEND] Add Header: {header.ToJson2()}");

            return WrapBytesWithMessage(commandData, header);
        }

        /// <summary>
        ///     Convert message object to byte representation
        /// </summary>
        /// <param name="message">Message which needs to be appended.</param>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <returns>Data buffer representing the message object content</returns>
        public static byte[] ConvertMessageToByteArray<TMessage>(TMessage message) where TMessage : struct
        {
            var size = Marshal.SizeOf<TMessage>();
            var newMessage = new byte[size];

            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(message, ptr, true);
            Marshal.Copy(ptr, newMessage, 0, newMessage.Length);
            Marshal.FreeHGlobal(ptr);

            return newMessage;
        }

        /// <summary>
        ///     Convert byte representation to message object
        /// </summary>
        /// <param name="byteData"> Data buffer representing the message object content </param>
        /// <typeparam name="TMessage">Message object corresponding to data buffer </typeparam>
        /// <returns> Marshaled Type from byte[] </returns>
        public static TMessage ConvertByteArrayToMessage<TMessage>(byte[] byteData) where TMessage : struct
        {
            var size = Marshal.SizeOf(typeof(TMessage));
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(byteData, 0, ptr, size);
            var message = Marshal.PtrToStructure<TMessage>(ptr);
            Marshal.FreeHGlobal(ptr);
            return message;
        }

        private static int GetMessageStartIndex(object messageType)
        {
            switch (messageType)
            {
                case MessageHeader _:
                    return MsgEncHdrSize;
                case MessageEncryptHeader _:
                    return 0;
                default:
                    return MsgEncHdrSize + MsgHdrSize;
            }
        }

        private static long GetTimeUnix()
        {
            var dto = new DateTimeOffset(DateTime.Now);
            return dto.ToUnixTimeSeconds();
        }
    }
}