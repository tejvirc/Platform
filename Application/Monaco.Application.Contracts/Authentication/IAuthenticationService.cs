namespace Aristocrat.Monaco.Application.Contracts.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Kernel;
    using Kernel.Contracts.Components;

    /// <summary>
    ///     Constants to support IAuthenticationService
    /// </summary>
    public enum AuthenticationServiceConstants
    {
        /// <summary>
        ///     End of stream
        /// </summary>
        EndOfStream = -1
    }

    /// <summary>
    ///     Authentication service interface
    /// </summary>
    [CLSCompliant(false)]
    public interface IAuthenticationService : IService
    {
        /// <summary>
        ///     Get a list of supported hash algorithms.
        /// </summary>
        /// <returns>List of hash algorithms</returns>
        IEnumerable<AlgorithmType> SupportedAlgorithms { get; }

        /// <summary>
        ///     Get a list of component verifications.
        /// </summary>
        /// <returns>List of component verifications</returns>
        IEnumerable<ComponentVerification> GetVerifications();

        /// <summary>
        ///     Get one component's verification
        /// </summary>
        /// <param name="key">Which component</param>
        /// <returns>The component's verification</returns>
        ComponentVerification GetVerification(Component key);

        /// <summary>
        ///     Calculates the hash.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="verification">The component verification.</param>
        void CalculateHash(
            Component component,
            ComponentVerification verification);

        /// <summary>
        ///     Calculates the hash for a given file.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="algorithm">The hash algorithm.</param>
        /// <param name="salt">The salt (default: none).</param>
        /// <param name="key">The key to be used for HMAC based algorithms (default: none).</param>
        /// <param name="startOffset">The start offset (default: 0).</param>
        /// <param name="endOffset">The end offset (default: EndOfStream.</param>
        /// <returns>Calculated hash.</returns>
        byte[] ComputeHash(
            Stream stream,
            string algorithm,
            byte[] salt = null,
            byte[] key = null,
            long startOffset = 0,
            long endOffset = (long)AuthenticationServiceConstants.EndOfStream);

        /// <summary>
        ///     Verifies the provided hash against the provided file.
        /// </summary>
        /// <param name="hash">The hash.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="algorithm">The hash algorithm.</param>
        /// <param name="key">The key to be used for HMAC based algorithms (default: none).</param>
        /// <param name="offset">The offset within the file (default: 0).</param>
        /// <param name="endOffset">The end offset (default: EndOfStream).</param>
        /// <returns>
        ///     true if it succeeds, false if it fails.
        /// </returns>
        bool VerifyHash(
            byte[] hash,
            Stream stream,
            string algorithm,
            byte[] key = null,
            long offset = 0,
            long endOffset = (long)AuthenticationServiceConstants.EndOfStream);

        /// <summary>
        ///     Create a task to hash all components in parallel.
        ///     Each will publish <see cref="ComponentHashCompleteEvent" /> when done.
        ///     The task will publish <see cref="AllComponentsHashCompleteEvent" /> when done.
        /// </summary>
        /// <param name="algorithmType">The hash algorithm</param>
        /// <param name="cancellationToken">A task cancellation token</param>
        /// <param name="seedOrSalt">The seed/salt parameter for certain algorithms, if necessary</param>
        /// <param name="componentTypes">The component types to compute the hash for</param>
        /// <returns>Success of operation</returns>
        Task GetComponentHashesAsync(
            AlgorithmType algorithmType,
            CancellationToken cancellationToken,
            byte[] seedOrSalt,
            IEnumerable<ComponentType> componentTypes);

        /// <summary>
        ///     Create a task to hash all components in parallel.
        ///     Each will publish <see cref="ComponentHashCompleteEvent" /> when done.
        ///     The task will publish <see cref="AllComponentsHashCompleteEvent" /> when done.
        /// </summary>
        /// <param name="algorithmType">The hash algorithm</param>
        /// <param name="cancellationToken">A task cancellation token</param>
        /// <param name="seedOrSalt">The seed/salt parameter for certain algorithms, if necessary</param>
        /// <param name="singleComponent">The name of one component, if hashing just one</param>
        /// <param name="offset">Where to start the data processing, relative to start of object (if single component)</param>
        /// <returns>Success of operation</returns>
        Task GetComponentHashesAsync(
            AlgorithmType algorithmType,
            CancellationToken cancellationToken,
            byte[] seedOrSalt = null,
            string singleComponent = null,
            long offset = 0);

        /// <summary>
        ///     Calculates Rom Hash for the EGM.
        /// </summary>
        /// <param name="seed">Seed value.</param>
        /// <returns>Crc32 value.</returns>
        int CalculateRomCrc32(int seed);

        /// <summary>
        ///     Calculates Hash for the EGM.
        /// </summary>
        /// <param name="algorithmType"><see cref="AlgorithmType"/>.</param>
        /// <returns>EGM hash value.</returns>
        string CalculateRomHash(AlgorithmType algorithmType);
    }
}