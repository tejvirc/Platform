namespace Aristocrat.Monaco.G2S
{
    using System.Collections.Generic;
    using Common.GAT.CommandHandlers;
    using Common.GAT.Models;
    using Common.GAT.Storage;
    using Kernel.Contracts.Components;
    using Monaco.Common.Models;

    /// <summary>
    ///     Base interface for GAT service.
    /// </summary>
    public interface IGatService
    {
        /// <summary>
        ///     Checks for a verification Id has been received from a host.
        /// </summary>
        /// <param name="verificationId">The verification identifier.</param>
        /// <returns>True if verificationId exists</returns>
        bool HasVerificationId(long verificationId);

        /// <summary>
        ///     Checks that a transaction Id has been used on the EGM.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns>
        ///     True if transactionId exists
        /// </returns>
        bool HasTransactionId(long transactionId);

        /// <summary>
        ///     Gets GatVerificationRequest the verification Id
        /// </summary>
        /// <param name="verificationId">Verification Id</param>
        /// <returns>GatVerificationRequest, null if not found</returns>
        GatVerificationRequest GetVerificationRequestById(long verificationId);

        /// <summary>
        ///     Load components
        /// </summary>
        /// <returns>Returns components list</returns>
        IEnumerable<Component> GetComponentList();

        /// <summary>
        ///     Gets a component by it's Id
        /// </summary>
        /// <param name="componentId">The unique component Id</param>
        /// <returns>A component</returns>
        Component GetComponent(string componentId);

        /// <summary>
        ///     Save or update component
        /// </summary>
        /// <param name="componentEntity">Component entity</param>
        /// <returns>Returns save entity result</returns>
        SaveEntityResult SaveComponent(Component componentEntity);

        /// <summary>
        ///     Verify components
        /// </summary>
        /// <param name="initVerificationArgs">
        ///     InitVerificationArgs contains verification identifier, transaction identifier and
        ///     component to verification
        /// </param>
        /// <returns>Returns list verified components, verification identifier, transaction identifier</returns>
        VerificationStatus DoVerification(DoVerificationArgs initVerificationArgs);

        /// <summary>
        ///     Get verification status by verification identifier, transaction identifier
        /// </summary>
        /// <param name="getVerificationStatusByTransactionArgs">
        ///     getVerificationStatusByTransactionArgs contains verification
        ///     identifier, transaction identifier
        /// </param>
        /// <returns>Returns verified components or components for verification</returns>
        VerificationStatusResult GetVerificationStatus(
            GetVerificationStatusByTransactionArgs getVerificationStatusByTransactionArgs);

        /// <summary>
        ///     Get verification status by verification identifier, device identifier
        /// </summary>
        /// <param name="getVerificationStatusByDeviceArgs">
        ///     getVerificationStatusByDeviceArgs contains verification identifier,
        ///     device identifier
        /// </param>
        /// <returns>Returns verified components or components for verification</returns>
        VerificationStatusResult GetVerificationStatus(
            GetVerificationStatusByDeviceArgs getVerificationStatusByDeviceArgs);

        /// <summary>
        ///     Update component verification state of components list
        /// </summary>
        /// <param name="saveVerificationAckArgs">
        ///     SaveVerificationAckArgs contains verification identifier, transaction identifier,
        ///     components
        /// </param>
        void SaveVerificationAck(SaveVerificationAckArgs saveVerificationAckArgs);

        /// <summary>
        ///     Load GAT special functions
        /// </summary>
        /// <returns>Returns GAT special functions list</returns>
        IEnumerable<GatSpecialFunction> GetSpecialFunctions();

        /// <summary>
        ///     Save or updates GAT special function
        /// </summary>
        /// <param name="gatSpecialFunctionEntity">GAT special function entity</param>
        /// <returns>Returns GAT special functions list</returns>
        SaveEntityResult SaveSpecialFunction(GatSpecialFunction gatSpecialFunctionEntity);

        /// <summary>
        ///     Load GAT verification requests
        /// </summary>
        /// <param name="transactionId">Transaction Id</param>
        /// <returns>Returns GAT verification requests list</returns>
        GatVerificationRequest GetLogForTransactionId(long transactionId);

        /// <summary>
        ///     Get supported algorithms list
        /// </summary>
        /// <param name="type">Component type.</param>
        /// <returns>Returns supported algorithms list</returns>
        IEnumerable<IAlgorithm> GetSupportedAlgorithms(ComponentType type);

        /// <summary>
        ///     Get max logSequence verification requests from and count verification requests
        /// </summary>
        /// <returns>
        ///     Returns GetLogStatusResult contains max logSequence verification requests from and count verification
        ///     requests
        /// </returns>
        GetLogStatusResult GetLogStatus();

        /// <summary>
        ///     Deletes component from persisted data.
        /// </summary>
        /// <param name="componentId">Component Id</param>
        /// <param name="type">Component type.</param>
        void DeleteComponent(string componentId, ComponentType type);

        /// <summary>
        ///     Gets Gat component verification entity.
        /// </summary>
        /// <param name="componentId">Component Id</param>
        /// <param name="verificationId">Verification Id</param>
        /// <returns>Gat component verification.</returns>
        GatComponentVerification GetGatComponentVerificationEntity(string componentId, long verificationId);

        /// <summary>
        ///     Updates GatComponentVerificationEntity.
        /// </summary>
        /// <param name="entity">GatComponentVerificationEntity to update.</param>
        void UpdateGatComponentVerificationEntity(GatComponentVerification entity);
    }
}
