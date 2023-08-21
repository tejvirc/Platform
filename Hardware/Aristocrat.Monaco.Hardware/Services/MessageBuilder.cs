namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Drawing;
    using System.Reflection;
    using log4net;

    /// <summary>
    ///     The responsibility of this class is to generate messages that conform with the
    ///     lighting controller protocol.
    /// </summary>
    public class MessageBuilder
    {
        /// <summary>
        ///     The version of the API that is being used to communicate;
        /// </summary>
        private const short MessageVersion = 1;

        /// <summary>The length of the message category field</summary>
        private const int MessageCategoryFieldLength = 2;

        /// <summary>The length of the message version field</summary>
        private const int MessageVersionFieldLength = 2;

        /// <summary>The length of the message length (data size) field</summary>
        private const int MessageLengthFieldLength = 4;

        /// <summary>
        ///     Create a logger for use in this class.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     The sync pattern. It is included at the start of the message header
        /// </summary>
        private static readonly byte[] SyncPattern = { 0x7A, 0x8B, 0x05, 0x74 };

        /// <summary>
        ///     Builds a PlayShow message using a showID
        /// </summary>
        /// <param name="showId">The show id to be played as a string</param>
        /// <returns>Returns a built message as a byte array</returns>
        public virtual byte[] BuildPlayShowMessage(string showId)
        {
            var message = new byte[sizeof(short) + showId.Length];

            // Add the length of the showID string to the message
            var showIdLength = BitConverter.GetBytes((short)showId.Length);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(showIdLength);
            }

            showIdLength.CopyTo(message, 0);

            for (var i = 0; i < showId.Length; i++)
            {
                message[i + showIdLength.Length] = Convert.ToByte(showId[i]);
            }

            return BuildMessageWithHeader(message, MessageType.PlayShow);
        }

        /// <summary>
        ///     Builds a message to change the light strip's color
        /// </summary>
        /// <param name="stripColor">The color that the light strip is to be changed to.</param>
        /// <param name="logoColor">Indicates if the logo is participating.</param>
        /// <returns>Return a built message as a byte array</returns>
        public virtual byte[] BuildChangeStripColorMessage(Color stripColor, Color logoColor)
        {
            // Setup the message
            // We don't have to convert to little endian because the type is a byte
            // The first three bytes are for the strip color
            // The last three bytes are for the VGT logo.
            byte[] message = { stripColor.R, stripColor.G, stripColor.B, logoColor.R, logoColor.G, logoColor.B };

            return BuildMessageWithHeader(message, MessageType.SetStripColor);
        }

        /// <summary>
        ///     Build a message to play a javascript
        /// </summary>
        /// <param name="script">Javascript file as a byte array</param>
        /// <returns>Return a built message as a byte array</returns>
        public virtual byte[] BuildPlayScriptMessage(byte[] script)
        {
            // Add the length of the javascript byte array to the message
            var scriptLength = BitConverter.GetBytes(script.Length);

            // We don't have to convert to little endian because the type is a byte
            var message = new byte[scriptLength.Length + script.Length];

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(scriptLength);
            }

            // Copy the length of the javascript file to the message
            scriptLength.CopyTo(message, 0);

            // Copy the javascript file to the message
            script.CopyTo(message, scriptLength.Length);

            return BuildMessageWithHeader(message, MessageType.PlayScript);
        }

        /// <summary>
        ///     This is a helper method that combines a message with
        ///     the appropriate header data.
        /// </summary>
        /// <param name="message">The message data.</param>
        /// <param name="typeOfMessage">The type of message to be built.</param>
        /// <returns>A byte array containing a message and appropriate header.</returns>
        private byte[] BuildMessageWithHeader(byte[] message, MessageType typeOfMessage)
        {
            var totalMessageSize =
                SyncPattern.Length +
                MessageCategoryFieldLength +
                MessageVersionFieldLength +
                MessageLengthFieldLength +
                message.Length;

            var messageWithHeader = new byte[totalMessageSize];
            var messageTypeByteArray = BitConverter.GetBytes((short)typeOfMessage);
            var messageVersionByteArray = BitConverter.GetBytes(MessageVersion);
            Logger.DebugFormat("Building a message with a length of: {0}", message.Length);
            var messageLengthByteArray = BitConverter.GetBytes(message.Length);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(messageTypeByteArray);
                Array.Reverse(messageLengthByteArray);
            }

            SyncPattern.CopyTo(messageWithHeader, 0);

            var index = SyncPattern.Length;

            messageVersionByteArray.CopyTo(messageWithHeader, index);
            index += MessageVersionFieldLength;

            messageTypeByteArray.CopyTo(messageWithHeader, index);
            index += MessageCategoryFieldLength;

            messageLengthByteArray.CopyTo(messageWithHeader, index);
            index += MessageLengthFieldLength;

            message.CopyTo(messageWithHeader, index);

            return messageWithHeader;
        }

        /// <summary>
        ///     These types define the different types of messages that can be sent to
        ///     the lighting controller.
        /// </summary>
        protected enum MessageType
        {
            /// <summary>
            ///     The default message type.
            /// </summary>
            None,

            /// <summary>
            ///     Indicates that the message contains data relevant to
            ///     playing a light show
            /// </summary>
            PlayShow,

            /// <summary>
            ///     Indicates that the message contains data relevant to
            ///     setting the color of the light strip
            /// </summary>
            SetStripColor,

            /// <summary>
            ///     Indicates that the message contains data relevant to
            ///     playing a scripted light show.
            /// </summary>
            PlayScript
        }
    }
}