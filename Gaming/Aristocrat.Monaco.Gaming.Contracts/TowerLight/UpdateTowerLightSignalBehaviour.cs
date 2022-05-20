namespace Aristocrat.Monaco.Gaming.Contracts.TowerLight
{
    /// <summary>
    ///     Indicate what strategy to select signal configuration behaviour
    /// </summary>
    public enum UpdateTowerLightSignalBehaviour
    {
        /// <summary>
        ///    First matched Operational Condition and Door Condition
        ///     Example: Tilt | Handpay, DoorOpen
        ///             OperationalCondition condition="Tilt"
        ///                    DoorCondition condition="DoorOpen"
        ///                         Set lightTier="Tier1" flashState="Off"
        ///                         Set lightTier="Tier4" flashState="SlowFlash"
        ///                         Set lightTier="Tier3" flashState="Off"
        ///                         Set lightTier="Tier2" flashState="Off"
        ///
        ///             OperationalCondition condition="Handpay"
        ///                    DoorCondition condition="DoorOpen"
        ///                         Set lightTier="Tier1" flashState="SlowFlash"
        ///                         Set lightTier="Tier4" flashState="Off"
        ///                         Set lightTier="Tier3" flashState="Off"
        ///                         Set lightTier="Tier2" flashState="Off"
        ///     Result:  Tier4 = SlowFlash and Tier1 = Off
        ///
        ///     Example: Tilt | Handpay, DoorOpen
        ///             OperationalCondition condition="Tilt"
        ///                    DoorCondition condition="DoorOpen"
        ///                         Set lightTier="Tier4" flashState="SlowFlash"
        ///
        ///             OperationalCondition condition="Handpay"
        ///                    DoorCondition condition="DoorOpen"
        ///                         Set lightTier="Tier1" flashState="SlowFlash"
        ///     Result:  Tier4 = SlowFlash and Tier1 = ?????? - unknown
        ///
        ///     Example: Tilt | Handpay, DoorOpen
        ///             OperationalCondition condition="Tilt"
        ///                    DoorCondition condition="DoorOpen"
        ///                         Set lightTier="Tier4" flashState="SlowFlash"
        ///
        ///             OperationalCondition condition="Handpay"
        ///                    DoorCondition condition="DoorOpen"
        ///                         Set lightTier="Tier4" flashState="On"
        ///     Result:  Tier4 = SlowFlash
        /// </summary>
        Default,

        /// <summary>
        ///     First matched Operational Condition and Door Condition has higher priority but lower could override if higher is not set of off
        ///     Example: Tilt | Handpay, DoorOpen
        ///             OperationalCondition condition="Tilt"
        ///                    DoorCondition condition="DoorOpen"
        ///                         Set lightTier="Tier1" flashState="Off"
        ///                         Set lightTier="Tier4" flashState="SlowFlash"
        ///                         Set lightTier="Tier3" flashState="Off"
        ///                         Set lightTier="Tier2" flashState="Off"
        ///
        ///             OperationalCondition condition="Handpay"
        ///                    DoorCondition condition="DoorOpen"
        ///                         Set lightTier="Tier1" flashState="SlowFlash"
        ///                         Set lightTier="Tier4" flashState="Off"
        ///                         Set lightTier="Tier3" flashState="Off"
        ///                         Set lightTier="Tier2" flashState="Off"
        ///     Result:  Tier4 = SlowFlash and Tier1 = SlowFlash
        ///
        ///     Example: Tilt | Handpay, DoorOpen
        ///             OperationalCondition condition="Tilt"
        ///                    DoorCondition condition="DoorOpen"
        ///                         Set lightTier="Tier4" flashState="SlowFlash"
        ///
        ///             OperationalCondition condition="Handpay"
        ///                    DoorCondition condition="DoorOpen"
        ///                         Set lightTier="Tier1" flashState="SlowFlash"
        ///     Result:  Tier4 = SlowFlash and Tier1 = SlowFlash
        ///
        ///     Example: Tilt | Handpay, DoorOpen
        ///             OperationalCondition condition="Tilt"
        ///                    DoorCondition condition="DoorOpen"
        ///                         Set lightTier="Tier4" flashState="SlowFlash"
        ///
        ///             OperationalCondition condition="Handpay"
        ///                    DoorCondition condition="DoorOpen"
        ///                         Set lightTier="Tier4" flashState="On"
        ///     Result:  Tier4 = SlowFlash
        /// </summary>
        PriorityHigherCanBeOverriddenIfNoneOrOff,
    }
}