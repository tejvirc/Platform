namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    /// <summary>The type of Sas change requests reported to the host</summary>
    public enum ChangeType
    {
        /// <summary>Meter changes that are required to be pre-authorized by the Sas host</summary>
        Meters,
        /// <summary>Game or Paytable changes that are required to be pre-authorized by the Sas host</summary>
        GameOrPaytable
    }

    /// <summary>The interface for Sas change requests</summary>
    public interface ISasChangeRequest
    {
        /// <summary>The type of change</summary>
        ChangeType Type { get; }

        /// <summary>The method to commit the change</summary>
        void Commit();

        /// <summary>The method to cancel the change</summary>
        void Cancel();
    }
}
