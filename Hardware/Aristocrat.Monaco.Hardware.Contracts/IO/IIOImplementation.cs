namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using System;
    using SharedDevice;

    /// <summary>Valid IO state enumerations.</summary>
    public enum IOState
    {
        /// <summary>Indicates IO state idle.</summary>
        Idle = 0,

        /// <summary>Indicates IO state error.</summary>
        Error
    }

    /// <summary>Definition of the IIOImplementation interface.</summary>
    [CLSCompliant(false)]
    public interface IIOImplementation
    {
        /// <summary>Gets the implemented device configuration such as make, model, firmware ID.</summary>
        /// <returns>Device configuration.</returns>
        Device DeviceConfiguration { get; }

        /// <summary>Gets the maximum number of available inputs.</summary>
        /// <returns>Maximum number of inputs.</returns>
        int GetMaxInputs { get; }

        /// <summary>
        ///     Gets the maximum number of available general purpose inputs.
        /// </summary>
        /// <returns>Maximum number of general purpose inputs.</returns>
        int GetMaxGeneralPurposeInputs { get; }

        /// <summary>
        ///     Gets the maximum number of available general purpose outputs.
        /// </summary>
        /// <returns>Maximum number of general purpose outputs.</returns>
        int GetMaxGeneralPurposeOutputs { get; }

        /// <summary>Gets the maximum number of available outputs.</summary>
        /// <returns>Maximum number of outputs.</returns>
        int GetMaxOutputs { get; }

        /// <summary>Gets the current inputs.</summary>
        /// <returns>Current inputs.</returns>
        ulong GetInputs { get; }

        /// <summary>Gets an array of intrusion events (door open/close), including those that occurred while powered down</summary>
        /// <returns>An array of intrusion events from oldest to most recent</returns>
        InputEvent[] GetIntrusionEvents { get; }

        /// <summary>Gets the current outputs.</summary>
        /// <returns>Current outputs.</returns>
        ulong GetOutputs { get; }

        /// <summary>Gets a value indicating whether the watchdog is enabled or not.</summary>
        /// <returns>Number of seconds remaining before watchdog triggers a reboot.</returns>
        int GetWatchdogEnabled { get; }

        /// <summary>Get whether the carrier board was removed during power-down or changed.</summary>
        /// <returns>true if board was removed, else false</returns>
        bool WasCarrierBoardRemoved { get; }

        /// <summary>Cleans up the IO implementation.</summary>
        void Cleanup();

        /// <summary>Enables/disables the watchdog for the given seconds.</summary>
        /// <param name="seconds">
        ///     Number of seconds the watchdog should wait for a reset command before rebooting the system.
        ///     Valid range is 1 to 255, 0 will disable the watchdog.
        /// </param>
        /// <returns>0 if watchdog enabled, -1 if could not be enabled.</returns>
        int EnableWatchdog(int seconds);

        /// <summary>
        ///     We initialize the device here.
        /// </summary>
        void Initialize();

        /// <summary>Resets the watchdog to the second interval set when enabled.</summary>
        /// <returns>0 if watchdog reset, -1 if watchdog could not be reset.</returns>
        int ResetWatchdog();

        /// <summary>Turns the given outputs off.</summary>
        /// <param name="outputs">Outputs mask.</param>
        void TurnOutputsOff(ulong outputs);

        /// <summary>Turns the given outputs on.</summary>
        /// <param name="outputs">Outputs mask.</param>
        void TurnOutputsOn(ulong outputs);

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

        /// <summary>resets the Door Seal value (all doors).</summary>
        void ResetDoorSeal();

        /// <summary>Sets the Door Seal value.</summary>
        /// <returns>set the seal latch value (0 = reset)</returns>
        byte GetDoorSealValue();

        /// <summary>Sets the Logical Door Seal value.</summary>
        /// <param name="sealValue">set the seal latch value (don't use 0x80 or 0x255, see GetLogicalDoorSealValue)</param>
        void SetLogicDoorSealValue(byte sealValue);

        /// <summary>Gets the Door Seal value.</summary>
        /// <returns>Get the seal latch value (0x80 = door was open, 0x255 = door is open)</returns>
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

        /// <summary>Get Firmware Data.</summary>
        /// <param name="location">represent which firmware to retrieve</param>
        /// <returns>the firmware data of location</returns>
        byte[] GetFirmwareData(FirmwareData location);

        /// <summary>Get Firmware Size.</summary>
        /// <param name="location">represent which firmware to retrieve</param>
        /// <returns>the firmware data of location</returns>
        ulong GetFirmwareSize(FirmwareData location);

        /// <summary>Gets the firmware description</summary>
        /// <param name="location">represent which firmware to retrieve</param>
        /// <returns>the firmware version/revision</returns>
        string GetFirmwareVersion(FirmwareData location);

        /// <summary>Test Battery Status.</summary>
        /// <remarks>method will block up to 400ms while testing battery</remarks>
        /// <param name="batteryIndex">Gen 8 battery index 0 or 1 (two batteries)</param>
        /// <returns>the battery test successful or failure</returns>
        bool TestBattery(int batteryIndex);

        /// <summary>Sets the state of tower light device.</summary>
        /// <param name="lightIndex">Indicates which light to set. 0 for Tier#1, 1 for Tier#2, 2 for Tier#3, 3 for Tier#4, 4 for strobe.</param>
        /// <param name="lightOn">State of the light on (true) or off (false).</param>
        /// <returns>true if successful, otherwise false.</returns>
        bool SetTowerLight(int lightIndex, bool lightOn);

        /// <summary>Sets the ringing state for the bell</summary>
        /// <param name="ringBell">Whether or not the bell should be ringing</param>
        /// <returns>true if successful, otherwise false</returns>
        bool SetBellState(bool ringBell);

        /// <summary>
        ///     Sets red screen free spin bank show state
        /// </summary>
        /// <param name="bankOn">State of the bank show on (true) or off (false).</param>
        /// <returns>true if successful, otherwise false</returns>
        bool SetRedScreenFreeSpinBankShow(bool bankOn);

        /// <summary>
        ///     Gets the General Purpose register value.
        /// </summary>
        /// <returns></returns>
        ulong GetGeneralPurposeInputs();
    }
}