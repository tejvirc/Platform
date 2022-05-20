namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using Hardware.Contracts.Door;
    /// <summary>
    /// DoorLogicId mapped for ASP (TXM-2300)
    /// </summary>
    public enum AspDoorLogicalId
    {
        Main = DoorLogicalId.Main,
        MainOptic = DoorLogicalId.MainOptic,
        Belly = DoorLogicalId.Belly,
        TopMain = DoorLogicalId.TopBox,
        TopMainOptic = DoorLogicalId.TopBoxOptic,
        CashBox = DoorLogicalId.DropDoor,
        BillStacker = DoorLogicalId.CashBox,
        Logic = DoorLogicalId.Logic
    }
}
