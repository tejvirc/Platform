namespace Aristocrat.Monaco.Hhr.Client.Data
{
    /// <summary>
    ///     GtCommand enumeration, received as part of CMD_COMMAND request.
    /// </summary>
    public enum GtCommand
    {
        /// <summary> Do Nothing </summary>
        NoOperation = 0,
        /// <summary> Allow device to play </summary> 
        Play,
        /// <summary> Shutdown a device </summary>
        Exit,
        /// <summary> Reboot a device to play  </summary>
        Reboot,
        /// <summary> GT powers down </summary>
        Poweroff,
        /// <summary> Force a player to logoff from a device </summary>
        LogoffPlayer,
        /// <summary> Lock a device </summary>
        Lock,
        /// <summary> Have a device update it's meters </summary>
        UpdateMeters,
        /// <summary> Have a device clear it's meters </summary>
        ClearMeters,
        /// <summary> Compute the checksum (CRC_32) of a devices critical files </summary>
        ComputeChecksum32,
        /// <summary> Reinitialize the communications connection (Drop connection and start over) </summary>
        ReInitialization,               
        /// <summary> Display this text on the player terminal </summary>
        SignMessage,          
        /// <summary> Compute the checksum (CRC_16) of a devices critical files (SAS TPS) </summary>
        ComputeChecksum16,  
        /// <summary> Mute all audio </summary>
        Mute,                 
        /// <summary> Unmute all audio </summary>
        UnMute,               
        /// <summary> Enable bill acceptor </summary>
        EnableBa,            
        /// <summary> Disable bill acceptor </summary>
        DisableBa,           
        /// <summary> Enter maintenance screen (PIN screen) </summary>
        EnterMaintenance,    
        /// <summary> Exit maintenance screen </summary>
        ExitMaintenance,     
        /// <summary> Remotely clear a large win/ IRS lockup </summary>
        RemoteReset,         
        /// <summary> Configure bill denominations </summary>
        BillDenoms,          
        /// <summary> Configure Hand Pay Device </summary>
        SetHandpayDevice,   
        /// <summary> Enable or Disable Voucher Printer </summary>
        SetVoucherStatus,   		
        /// <summary> Reinitialize all games and game data </summary>
        Reinitgames,          
        /// <summary> Requires a UNC name to the file to download </summary>
        Download,             
        /// <summary> Requires a UNC name to the file to execute </summary>
        Execute,              
        /// <summary> Setup a different jackpot reset method (than the default) </summary>
        SetupResetMethod,   
        /// <summary> Go to an AFT Lock state. </summary>
        SasAftlock,          
        /// <summary> Leave an AFT Lock state. </summary>
        SasAftunlock,        
        /// <summary> Set the SAS Legacy Bonus Game Delay value. </summary>
        SasLegacybonusdelay, 
        /// <summary> Set the SAS Legacy Bonus Award. </summary>
        SasLegacybonusaward, 
        /// <summary> Set the SAS Legacy Bonus Award. </summary>
        LockoutBoardState, 
        /// <summary> Have GT pause and the re-ask for GTCMD_PLAY </summary>
        PlayPause,
    }
}
