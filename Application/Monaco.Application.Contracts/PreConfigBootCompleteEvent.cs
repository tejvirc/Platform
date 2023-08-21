////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="PreConfigBootCompleteEvent.cs" company="ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD">
// COPYRIGHT © 2017 ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD
// Absolutely no use, dissemination or copying in any matter whatsoever
// Of this material or any portion of it is to be made without the prior
// written authorisation of Aristocrat Technologies Australia Pty Ltd.
// All rights in and to this work are fully reserved
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event for when all application components needed for first-boot configuration have been loaded.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is posted on first boot after the operator has made configuration
    ///         selections that control addin loading (like jurisdiction, protocol) and then
    ///         remaining components needed to complete configuration have been loaded.
    ///     </para>
    ///     <para>
    ///         The event should be handled by the entity performing configuration (e.g. config wizard) to
    ///         know when it can proceed with configuration.
    ///     </para>
    /// </remarks>
    [Serializable]
    public class PreConfigBootCompleteEvent : BaseEvent
    {
    }
}