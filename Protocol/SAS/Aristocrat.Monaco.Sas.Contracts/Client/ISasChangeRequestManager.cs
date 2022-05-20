namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    /// <summary>Definition of the ISasMeterChangeHandler interface</summary>
    public interface ISasChangeRequestManager
    {
        /// <summary>The method to add a Sas change request</summary>
        /// <param name="request">The Sas change request</param>
        void AddRequest(ISasChangeRequest request);
    }
}
