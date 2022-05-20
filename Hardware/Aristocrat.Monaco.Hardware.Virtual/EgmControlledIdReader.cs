namespace Aristocrat.Monaco.Hardware.Virtual
{
    /// <summary>A virtual Egm Controlled ID reader.</summary>
    public class EgmControlledIdReader : VirtualCommunicator
    {
        /// <summary>Base name is used to fake out various identification strings (overrideable).</summary>
        protected override string BaseName => "Virtual2";
    }
}
