namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using System;
    using System.Collections.Generic;
    using Kernel;
    using SharedDevice;

    /// <summary>Logical IO state enumerations. Represents logical states of the IO.</summary>
    public enum IOLogicalState
    {
        /// <summary>Indicates IO state uninitialized.</summary>
        Uninitialized = 0,

        /// <summary>Indicates IO state idle.</summary>
        Idle,

        /// <summary>Indicates IO state error.</summary>
        Error,

        /// <summary>Indicates IO state disabled.</summary>
        Disabled
    }

    /// <summary>Represents firmware data location.</summary>
    public enum FirmwareData
    {
        /// <summary>Indicates IO state uninitialized.</summary>
        Bios = 0,

        /// <summary>Indicates IO state idle.</summary>
        Fpga = 1
    }

    /// <summary>
    ///     The IO Interfaces component defines the IO events and public interfaces of the IO components.
    /// </summary>
    [CLSCompliant(false)]
    public interface IIO
    {
        /// <summary>Gets the implemented device configuration such as make, model, firmware ID.</summary>
        /// <returns>Device configuration.</returns>
        Device DeviceConfiguration { get; }

        /// <summary>Gets the maximum number of available inputs.</summary>
        /// <returns>Maximum number of inputs.</returns>
        int GetMaxInputs { get; }

        /// <summary>Gets the maximum number of available outputs.</summary>
        /// <returns>Maximum number of outputs.</returns>
        int GetMaxOutputs { get; }

        /// <summary>Gets the current state of inputs.</summary>
        /// <returns>The inputs of the I/O card.</returns>
        ulong GetInputs { get; }

        /// <summary>Gets the current state of outputs.</summary>
        /// <returns>The outputs of the I/O card.</returns>
        ulong GetOutputs { get; }

        /// <summary>
        ///     Gets the InputEvents that have been queued since IO startup
        /// </summary>
        /// <returns>The queued events.</returns>
        ICollection<IEvent> GetQueuedEvents { get; }

        /// <summary>Get whether the carrier board was removed during power-down or changed.</summary>
        /// <returns>true if board was removed, else false</returns>
        bool WasCarrierBoardRemoved { get; }

        /// <summary>Gets or sets the last changed inputs.</summary>
        /// <returns>The last changed inputs.</returns>
        ulong LastChangedInputs { get; set; }

        /// <summary>Gets the logical IO service state.</summary>
        /// <returns>The logical IO service state.</returns>
        IOLogicalState LogicalState { get; }

        /// <summary>Sets the action for the given output physical ID.</summary>
        /// <param name="physicalId">The physical ID.</param>
        /// <param name="action">Flag indicating action to perform. For example light on/off, bell ringing/not ringing.</param>
        /// <param name="postActionEvent">Flag indicating whether to post an event when the action occurs.</param>
        void SetOutput(int physicalId, bool action, bool postActionEvent);

        /// <summary>Sets the action of 32 outputs at a time.</summary>
        /// <param name="physicalId">The physical ID.</param>
        /// <param name="action">Flag indicating action to perform. For example light on/off, bell ringing/not ringing.</param>
        /// <param name="postActionEvent">Flag indicating whether to post an event when the action occurs.</param>
        void SetOutput32(int physicalId, bool action, bool postActionEvent);

        /// <summary>set the state of meter light.</summary>
        /// <param name="lightEnabled">state of the meter light.</param>
        void SetMechanicalMeterLight(bool lightEnabled);

        /// <summary>Set the action of mechanical meters.</summary>
        /// <param name="meterMask">
        ///     The physical meter index  in bit mask form (least significant bit is meter 0, next bit is 1,
        ///     etc)
        /// </param>
        /// <returns>The successful meters updated (masked bits).</returns>
        int SetMechanicalMeter(int meterMask);

        /// <summary>Clear the action of mechanical meters.</summary>
        /// <param name="meterMask">
        ///     The physical meter index  in bit mask form (least significant bit is meter 0, next bit is 1,
        ///     etc)
        /// </param>
        /// <returns>The successful meters updated (masked bits).</returns>
        int ClearMechanicalMeter(int meterMask);

        /// <summary>Status of mechanical meter.</summary>
        /// <param name="meterMask">
        ///     The physical meter index  in bit mask form (least significant bit is meter 0, next bit is 1,
        ///     etc)
        /// </param>
        /// <returns>The successful meters updated (masked bits).</returns>
        int StatusMechanicalMeter(int meterMask);

        /// <summary>set the state of a single button light.</summary>
        /// <param name="lightIndex">light to be set (0-16 bits).</param>
        /// <param name="lightStatusOn">state of the light on (true) or off (false).</param>
        /// ///
        /// <returns>returns the light bit.</returns>
        [CLSCompliant(false)]
        uint SetButtonLamp(uint lightIndex, bool lightStatusOn);

        /// <summary>set the state of a range of button lights.</summary>
        /// <param name="lightBits">
        ///     light bits to be set in bit mask form (0x0000 - 0xFFFF)
        ///     least significant bit sets light index 0, most significant bit sets light 15.
        /// </param>
        /// <param name="lightStatusOn">state of the light on (true) or off (false).</param>
        /// <returns>returns the light bits.</returns>
        [CLSCompliant(false)]
        uint SetButtonLampByMask(uint lightBits, bool lightStatusOn);

        /// <summary>Sets the Door Seal value.</summary>
        void ResetDoorSeal();

        /// <summary>Sets the Door Seal value.</summary>
        /// <returns>set the seal latch value (0 = reset)</returns>
        byte GetDoorSealValue();

        /// <summary>Sets the LogicDoor Seal value.</summary>
        /// <param name="sealValue">set the seal latch value (dont use 0x80 or 0x255, see GetLogicDoorSealVlaue)</param>
        void SetLogicDoorSealValue(byte sealValue);

        /// <summary>Gets the Door Seal value.</summary>
        /// <returns>set the seal latch value (0x80 = door was open, 0x255 = door is open)</returns>
        byte GetLogicDoorSealValue();

        /// <summary>Resets the Door Seal value for the specified physical id.</summary>
        /// <param name="physicalId">The physical id of the door to reset</param>
        void ResetPhysicalDoorWasOpened(int physicalId);

        /// <summary>Gets a value indicating whether the physical door was opened.</summary>
        /// <param name="physicalId">The physical id of the door whose status to get.</param>
        /// <returns>True if the physical door was opened</returns>
        bool GetPhysicalDoorWasOpened(int physicalId);

        /// <summary>Sets the key indicator enabled or  disabled.</summary>
        /// <param name="keyMask">the masks of keys to set state</param>
        /// <param name="lightEnabled">set the state of light</param>
        void SetKeyIndicator(int keyMask, bool lightEnabled);

        /// <summary>Gets the electronics (service protocol)</summary>
        /// <returns>the electronics version</returns>
        string GetElectronics();

        /// <summary>Get Firmware Data.</summary>
        /// <param name="location">represent which firmware to retrieve</param>
        /// <returns>the firmware data of location</returns>
        byte[] GetFirmwareData(FirmwareData location);

        /// <summary>Get Firmware Size.</summary>
        /// <param name="location">represent which firmware to retrieve</param>
        /// <returns>the firmware data of location</returns>
        long GetFirmwareSize(FirmwareData location);

        /// <summary>Gets the firmware version</summary>
        /// <param name="location">represent which firmware to retrieve</param>
        /// <returns>the firmware version/revision</returns>
        string GetFirmwareVersion(FirmwareData location);

        /// <summary>
        /// Tests the specified battery
        /// </summary>
        /// <param name="batteryIndex"></param>
        /// <returns></returns>
        bool TestBattery(int batteryIndex);

        /// <summary>Sets the state of tower light device.</summary>
        /// <param name="lightIndex">Indicates which light to set. 0 for Tier#1, 1 for Tier#2, 2 for Tier#3, 3 for Tier#4.</param>
        /// <param name="lightOn">State of the light on (true) or off (false).</param>
        /// <returns>true if successful, otherwise false.</returns>
        bool SetTowerLight(int lightIndex, bool lightOn);

        /// <summary>Sets the ringing state for the bell</summary>
        /// <param name="ringBell">Whether or not the bell should be ringing</param>
        /// <returns>true if successful, otherwise false</returns>
        bool SetBellState(bool ringBell);

        /// <summary>Sets red screen free spin bank show state</summary>
        /// <param name="bankOn">State of the bank show on (true) or off (false).</param>
        /// <returns>true if successful, otherwise false</returns>
        bool SetRedScreenFreeSpinBankShow(bool bankOn);
    }
}