namespace Aristocrat.Monaco.Hardware.Contracts.TicketContent
{
    /// <summary>Definition of the IRenderFactory interface.</summary>
    public interface IRenderFactory
    {
        /// <summary>Gets the factory product key.</summary>
        /// <returns>Factory product key object.</returns>
        object FactoryProductKey { get; }
    }
}