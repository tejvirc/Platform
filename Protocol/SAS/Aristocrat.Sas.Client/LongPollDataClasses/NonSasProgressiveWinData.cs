namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    public class NonSasProgressiveWinHostData : LongPollData
    {
        public byte ClientNumber { get; set; }
    }

    /// <summary>
    ///     Non Sas Progressive Win Data
    /// </summary>
    public class NonSasProgressiveWinData
    {
        /// <summary>
        ///     Gets the controller type for this result
        /// </summary>
        public int ControllerType { get; }

        /// <summary>
        ///     Gets the controller ID
        /// </summary>
        public int ControllerID { get; }

        /// <summary>
        ///     Gets the level
        /// </summary>
        public int Level { get; }

        /// <summary>
        ///     Gets the amount
        /// </summary>
        public long Amount { get; }

        /// <summary>
        ///     Gets the base amount
        /// </summary>
        public long BaseAmount { get; }

        /// <summary>
        ///     Gets the escrow amount
        /// </summary>
        public long EscrowAmount { get; }

        /// <summary>
        ///     Creates the NonSasProgressiveWinResponse
        /// </summary>
        /// <param name="controllerType">the controller type for this result</param>
        /// <param name="controllerID">the controller ID</param>
        /// <param name="level">the level</param>
        /// <param name="amount">the amount</param>
        /// <param name="baseAmount">the base amount</param>
        /// <param name="escrowAmount">the escrow amount</param>
        public NonSasProgressiveWinData(
            int controllerType,
            int controllerID,
            int level,
            long amount,
            long baseAmount,
            long escrowAmount)
        {
            ControllerType = controllerType;
            ControllerID = controllerID;
            Level = level;
            Amount = amount;
            BaseAmount = baseAmount;
            EscrowAmount = escrowAmount;
        }
    }
}