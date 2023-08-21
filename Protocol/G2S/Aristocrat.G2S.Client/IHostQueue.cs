namespace Aristocrat.G2S.Client
{
    using System;
    using Communications;

    /// <summary>
    ///     Provides a mechanism to control a host queue.
    /// </summary>
    public interface IHostQueue : IMessageReceiver, ICommandQueue
    {
        /// <summary>
        ///     Invoked when a message is sent to the associated host.
        /// </summary>
        event EventHandler<MessageHandledEventArgs> MessageSent;

        /// <summary>
        ///     Invoked when a message is received from the associated host.
        /// </summary>
        event EventHandler<MessageHandledEventArgs> MessageReceived;

        /// <summary>
        ///     Removes all  <see cref="T:Aristocrat.G2S.ClassCommand" />s from the queue for the specified host.
        /// </summary>
        void Clear();

        /// <summary>
        ///     Removes all  <see cref="T:Aristocrat.G2S.ClassCommand" />s from the queue for the specified host.
        /// </summary>
        /// <param name="clearInbound">true to clear inbound messages.</param>
        void Clear(bool clearInbound);

        /// <summary>
        ///     Removes and returns the <see cref="T:Aristocrat.G2S.ClassCommand" /> at the beginning of the send queue for the
        ///     specified host.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:Aristocrat.G2S.ClassCommand" /> that is removed from the beginning of the send queue.
        /// </returns>
        ClassCommand Dequeue();

        /// <summary>
        ///     Disable sending for a host.
        /// </summary>
        void DisableSend();

        /// <summary>
        ///     Enable sending for a host.
        /// </summary>
        /// <param name="startPump">Flag indicating if the command pump should be started.</param>
        void EnableSend(bool startPump);

        /// <summary>
        ///     Adds a <see cref="T:Aristocrat.G2S.ClassCommand" /> to the end of the send queue.
        /// </summary>
        /// <param name="command">
        ///     The <see cref="T:Aristocrat.G2S.ClassCommand" /> to add to the send queue.
        /// </param>
        /// <returns><b>true</b> if successful; otherwise <b>false</b>.</returns>
        bool Enqueue(ClassCommand command);

        /// <summary>
        ///     Set a flag indicating a host is online.
        /// </summary>
        void SetOnline();

        /// <summary>
        ///     Returns th <see cref="T:Aristocrat.G2S.ClassCommand" /> at the beginning of the send
        ///     queue without removing it.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:Aristocrat.G2S.ClassCommand" /> at the beginning of the send queue.
        /// </returns>
        ClassCommand Peek();

        /// <summary>
        ///     Get the next <see cref="T:Aristocrat.G2S.ClassCommand" /> in the to process in the queue.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:Aristocrat.G2S.ClassCommand" /> at the beginning of the receive queue.
        /// </returns>
        ClassCommand Process();

        /// <summary>
        ///     Returns the <see cref="T:Aristocrat.G2S.ClassCommand" /> at the beginning of the received queue without removing
        ///     it.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:Aristocrat.G2S.ClassCommand" /> at the beginning of the received queue.
        /// </returns>
        ClassCommand PeekProcess();

        /// <summary>
        ///     Add a <see cref="T:Aristocrat.G2S.ClassCommand" /> to the received queue.
        /// </summary>
        /// <param name="command"><see cref="T:Aristocrat.G2S.ClassCommand" /> that needs to be processed.</param>
        void Received(ClassCommand command);
    }
}