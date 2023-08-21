namespace Aristocrat.Monaco.Application.Contracts
{
    using Kernel;

    /// <summary>
    ///     An event to notify that the a LegitimacyLockUpEvent occured.
    /// </summary>
    /// <remarks>
    ///     >
    ///     <para>
    ///         The event should be taken care by the components which are interested
    ///         in the hard tilt conditions. For an example, the alarm will be sounded
    ///         when it is received by the <c>LightAndAlarmCoordinator</c> component.
    ///         Check the <c>LightAndAlarmCoordinatorMappings.addin.xml</c> configuration
    ///         file.
    ///     </para>
    /// </remarks>
    public class LegitimacyLockUpEvent : BaseEvent
    {
    }
}