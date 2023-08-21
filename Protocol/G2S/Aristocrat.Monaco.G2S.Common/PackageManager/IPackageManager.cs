namespace Aristocrat.Monaco.G2S.Common.PackageManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Data.Model;
    using CommandHandlers;
    using PackageManifest.Models;
    using Storage;

    /// <summary>
    ///     Defines a contract for a package manager.
    /// </summary>
    public interface IPackageManager
    {
        /// <summary>
        ///     Gets Package Transfer Abort Tokens
        /// </summary>
        IDictionary<string, CancellationTokenSource> PackageTaskAbortTokens { get; }

        /// <summary>
        ///     Gets package count
        /// </summary>
        int PackageCount { get; }

        /// <summary>
        ///     Gets script count
        /// </summary>
        int ScriptCount { get; }


        /// <summary>
        ///     Gets Package Entity list
        /// </summary>
        IReadOnlyCollection<Package> PackageEntityList { get; }

        /// <summary>
        ///     Gets Module Entity list
        /// </summary>
        IReadOnlyCollection<Module> ModuleEntityList { get; }

        /// <summary>
        ///     Gets Script Entity list
        /// </summary>
        IReadOnlyCollection<Script> ScriptEntityList { get; }

        /// <summary>
        ///     Gets Transfer Entity list
        /// </summary>
        IReadOnlyCollection<TransferEntity> TransferEntityList { get; }

        /// <summary>
        ///     Creates a package.
        /// </summary>
        /// <param name="packageEntity">PackageLog entity instance.</param>
        /// <param name="module">Module entity.</param>
        /// <param name="overwrite">Indicates either package should be overwritten or not.</param>
        /// <param name="format">Archive format.</param>
        /// <returns>The state</returns>
        CreatePackageState CreatePackage(PackageLog packageEntity, Module module, bool overwrite, string format);

        /// <summary>
        ///     Uploads package.
        /// </summary>
        /// <param name="packageId">Package Id.</param>
        /// <param name="transfer">Transfer entity.</param>
        /// <param name="changeStatusCallback">Callback function.</param>
        /// <param name="ct">Cancellation token to abort upload.</param>
        /// <param name="packageLog">Package log.</param>
        void UploadPackage(
            string packageId,
            TransferEntity transfer,
            Action<PackageTransferEventArgs> changeStatusCallback,
            CancellationToken ct,
            PackageLog packageLog);

        /// <summary>
        ///     Deletes packages by package entity.
        /// </summary>
        /// <param name="deletePackageArgs">Delete package args.</param>
        void DeletePackage(DeletePackageArgs deletePackageArgs);

        /// <summary>
        ///     Validates a package.
        /// </summary>
        /// <param name="filePath">The file Path</param>
        /// <returns>true, if valid</returns>
        bool ValidatePackage(string filePath);

        /// <summary>
        ///     Gets package transfer entity
        /// </summary>
        /// <param name="packageId">Package Id</param>
        /// <returns>Transfer entity.</returns>
        TransferEntity GetTransferEntity(string packageId);

        /// <summary>
        ///     Gets package transfer entity
        /// </summary>
        /// <param name="transferId">Transfer Id</param>
        /// <returns>Transfer entity.</returns>
        TransferEntity GetTransferEntity(long transferId);

        /// <summary>
        ///     Gets module entity
        /// </summary>
        /// <param name="moduleId">Module Id</param>
        /// <returns>Module entity</returns>
        Module GetModuleEntity(string moduleId);

        /// <summary>
        ///     Gets package log entity.
        /// </summary>
        /// <param name="packageId">Package Id</param>
        /// <returns>PackageLog entity</returns>
        PackageLog GetPackageLogEntity(string packageId);

        /// <summary>
        ///     Gets package entity.
        /// </summary>
        /// <param name="packageId">Package Id</param>
        /// <returns>Package entity</returns>
        Package GetPackageEntity(string packageId);

        /// <summary>
        ///     Gets package error entity.
        /// </summary>
        /// <param name="packageId">Package Id</param>
        /// <returns>Package entity</returns>
        PackageError GetPackageErrorEntity(string packageId);

        /// <summary>
        ///     Retries last upload or download action for specified package.
        /// </summary>
        /// <param name="packageId">Package Id.</param>
        /// <param name="changeStatusCallback">Callback function.</param>
        /// <param name="packageLog">Package Log.</param>
        /// <param name="ct">Cancellation token to abort transfer</param>
        void RetryTransfer(
            string packageId,
            Action<PackageTransferEventArgs> changeStatusCallback,
            PackageLog packageLog,
            CancellationToken ct);

        /// <summary>
        ///     Checks if the package exists
        /// </summary>
        /// <param name="packageId">Package Id</param>
        /// <returns>True if the package has been added to the DB</returns>
        bool HasPackage(string packageId);

        /// <summary>
        ///     Checks if the module exists
        /// </summary>
        /// <param name="moduleId">Package Id</param>
        /// <returns>True if the module has been added to the DB</returns>
        bool HasModule(string moduleId);

        /// <summary>
        ///     Checks is script exists
        /// </summary>
        /// <param name="scriptId">Script Id</param>
        /// <returns>True if the script has been added to the DB</returns>
        bool HasScript(int scriptId);

        /// <summary>
        ///     Gets script instance exists
        /// </summary>
        /// <param name="scriptId">Script Id</param>
        /// <returns>Script entity</returns>
        Script GetScript(int scriptId);

        /// <summary>
        ///     Checks if the package is being transferred
        /// </summary>
        /// <param name="packageId">The package identifier</param>
        /// <returns>True if the package is being transferred</returns>
        bool IsTransferring(string packageId);

        /// <summary>
        ///     Downloads package.
        /// </summary>
        /// <param name="packageId">Package Id.</param>
        /// <param name="transfer">Transfer entity.</param>
        /// <param name="changeStatusCallback">Callback function.</param>
        /// <param name="ct">Cancellation token to abort upload.</param>
        /// <param name="packageLog">The package log.</param>
        /// <param name="deviceId">Device Id.</param>
        void DownloadPackage(
            string packageId,
            TransferEntity transfer,
            Action<PackageTransferEventArgs> changeStatusCallback,
            CancellationToken ct,
            PackageLog packageLog,
            int deviceId);

        /// <summary>
        ///     Installs package modules.
        /// </summary>
        /// <param name="package">Package entity</param>
        /// <param name="changeStatusCallback">Callback function.</param>
        /// <param name="deleteAfter">Flag to delete the package after install.</param>
        void InstallPackage(Package package, Action<InstallPackageArgs> changeStatusCallback, bool deleteAfter);

        /// <summary>
        ///     Uninstalls module.
        /// </summary>
        /// <param name="module">Module entity.</param>
        /// <param name="changeStatusCallback">Status callback.</param>
        void UninstallModule(Module module, Action<UninstallModuleArgs> changeStatusCallback);

        /// <summary>
        ///     Uninstalls package.
        /// </summary>
        /// <param name="package">Package entity.</param>
        /// <param name="changeStatusCallback">Status callback.</param>
        /// <param name="deleteAfter">Flag to delete the package after install.</param>
        void UninstallPackage(Package package, Action<InstallPackageArgs> changeStatusCallback, bool deleteAfter);

        /// <summary>
        ///     Adds if it doesn't already exist
        /// </summary>
        /// <param name="packageLog">Package Log.</param>
        void AddPackageLog(PackageLog packageLog);

        /// <summary>
        ///     Updates package, adds if it doesn't already exist
        /// </summary>
        /// <param name="packageLog">Package Log.</param>
        void UpdatePackage(PackageLog packageLog);

        /// <summary>
        ///     Updates transfer entity
        /// </summary>
        /// <param name="entity">Transfer entity</param>
        void UpdateTransferEntity(TransferEntity entity);

        /// <summary>
        ///     Updates script, adds if it doesn't already exist
        /// </summary>
        /// <param name="script">Script</param>
        void UpdateScript(Script script);

        /// <summary>
        ///     Updates or adds module entity
        /// </summary>
        /// <param name="module">Module entity</param>
        /// <returns>True if added new module.</returns>
        bool UpdateModuleEntity(Module module);

        /// <summary>
        ///     Search and reads manifest from directory that contains unpacked package.
        /// </summary>
        /// <param name="packageId">Package Id</param>
        /// <returns>Returns manifest instance.</returns>
        Image ReadManifest(string packageId);

        /// <summary>
        ///     Gets the last sequence number from the script repo.
        /// </summary>
        /// <returns>Last sequence number.</returns>
        long GetScriptLastSequence();

        /// <summary>
        ///     Verifies all packages against the stored package hash
        /// </summary>
        void VerifyPackages();

        /// <summary>
        ///     Converts XML to target type
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="xml">XML to convert</param>
        /// <returns>Deserialized XML object</returns>
        T ParseXml<T>(string xml)
            where T : class;

        /// <summary>
        ///     Converts XML serializable object to string
        /// </summary>
        /// <typeparam name="T">Type of class to be serialized</typeparam>
        /// <param name="class">Class object</param>
        /// <returns>XML string</returns>
        string ToXml<T>(T @class)
            where T : class;
    }
}
