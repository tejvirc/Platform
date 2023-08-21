namespace Aristocrat.Monaco.Bingo.Common
{
    using System;

    [Flags]
    public enum EgmStatusFlag
    {
        // Online host disables externally
        None = 0,

        // Online host disables externally
        OnLine = 1,

        // Limited play
        LimitedPlay = (1 << 1),

        // trade show mode in Monaco
        ExhibitMode = (1 << 2),

        // Credit meter out of bounds
        MeterBounds = (1 << 3),

        // Credit value out of range
        InvalidCredit = (1 << 4),

        // Operator option to disable by day of week
        Operator = (1 << 5),

        // Transaction account log is full
        TxLogFull = (1 << 6),

        // Machine serial number not set
        NotEnrolled = (1 << 7),

        // Bingo network disconnect
        NetDown = (1 << 9),

        // Printing a cash ticket
        Printing = (1 << 10),

        // A door is open
        DoorOpen = (1 << 11),

        // Operator menu is active
        InOperatorMenu = (1 << 12),

        // Operator has disabled the machine
        MachineDisabled = (1 << 13),

        // Reel malfunction
        ReelMalfunction = (1 << 14),

        // Game is not in an online state
        GameNotOnline = (1 << 15),

        // Printer is out of paper
        PrnNoPaper = (1 << 16),

        // Disabled until player tracking card is removed
        PlayerTracking = (1 << 17),

        // Event meter log is full
        EventMeterLogFull = (1 << 18),

        // Game log is full
        GameLogFull = (1 << 19),

        // Note acceptor error
        DbaError = (1 << 20),

        // Battery is low
        NvramBatteryLow = (1 << 21),

        // Disabled by a casino management system
        DisabledByCmsBackend = (1 << 22),

        // Disable by progressive controller
        DisabledByProgCntrl = (1 << 24),

        // Disable by progressive client
        DisabledByProgClient = (1 << 25),

        // Printer error
        PrinterError = (1 << 30)
    };
}
