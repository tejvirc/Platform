namespace Vgt.Client12.Hardware.IO
{
    /// <summary>Gen X IO command enumerations.</summary>
    public enum Gen8IOCommands
    {
        /// <summary>Indicates TPCI940 IO command none.</summary>
        ReadBar2Register32 = 0,

		/// <summary>Indicates TPCI940 IO command read.</summary>
        WriteBar2Register32 = 1,
		
        /// <summary>Indicates TPCI940 IO command read.</summary>
        Read = 10,

        /// <summary>Indicates TPCI940 IO command write.</summary>
        Write,

        /// <summary>Indicates TPCI940 IO command watchdog enable.</summary>
        WatchdogEnable,

        /// <summary>Indicates TPCI940 IO command watchdog disable.</summary>
        WatchdogDisable,

        /// <summary>Indicates TPCI940 IO command watchdog reset.</summary>
        WatchdogReset,

        /// <summary>Indicates TPCI940 IO command board info.</summary>
        BoardInfo,

        /// <summary>Get the size of bios [in] void, [out] ulong .</summary>
        BiosSize = 54,

        /// <summary>read bios [in] ulong bytes, [out] data array .</summary>
        BiosRead = 55,

        /// <summary>Indicates TPCI940 IO command input data.</summary>
        InputData = 57,

        /// <summary>Indicates TPCI940 IO command input data.</summary>
        InitializeSpiBus = 58,

        /// <summary>(not used) Indicates TPCI940 IO command out data.</summary>
        OutputData = 59,

        /// <summary>Set the mechanical ligght.</summary>
        MechanicalLight = 60,

        /// <summary>Read the Dear Seal Value.</summary>
        SecReadDoorSeal = 61,

        /// <summary>Reset the Dear Seal Value (clears all).</summary>
        SecResetDoorSeal = 62,

        /// <summary>Increment meter by mask (returns successful increments by mask). .</summary>
        IncrementMeters = 63,

        /// <summary>Set the Key Indicator light (hi byte = on/off, lo byte = 0-16 key).</summary>
        ButtonLamp = 64,

        /// <summary>Set meter by mask (returns successful increments by mask). </summary>
        SetMechanicalMeter = 65,

        /// <summary>Clear meter by mask (returns successful increments by mask). </summary>
        ClearMechanicalMeter = 66,

        /// <summary>Set meter by mask (returns successful increments by mask). </summary>
        StatusMechanicalMeter = 67,

        /// <summary>
        ///     Retrieves the current value of the logic door seal..  If the door was opened (during power down) this value is
        ///     read back as 0x80.  If the door is currently open then the value read back is 0xff.
        /// </summary>
        GetLogicDoorSeal = 68,

        /// <summary>Writes an 8-bit value to the logic door seal. Use the bottom 7 bits only. Keep th e8th bit clear. </summary>
        SetLogicDoorSeal = 69,

        /// <summary>Get the size of fpga [in] void, [out] ulong .</summary>
        FpgaSize = 70,

        /// <summary>read bios [in] ulong bytes, [out] data array .</summary>
        FpgaRead = 71,

        /// <summary>Set Towerlight index, [out] 0-7bit (light index), 8-15 state (0=off, 1-0xff = on) </summary>
        SetTowerLight = 72,
		
		/// <summary>Set light state by 16 bit mask, [out] 0-16bit (light indexes), 0xff0000 state (0=off, 1-0xff = on) </summary>
        ButtonLampByMask = 73
    }
}