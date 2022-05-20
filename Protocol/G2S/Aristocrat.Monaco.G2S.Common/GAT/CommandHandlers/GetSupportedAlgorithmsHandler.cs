namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts.Authentication;
    using Kernel.Contracts.Components;
    using Models;
    using Monaco.Common.CommandHandlers;

    /// <summary>
    ///     Get supported algorithms handler
    /// </summary>
    public class GetSupportedAlgorithmsHandler : IFuncHandler<IEnumerable<IAlgorithm>>
    {
        private readonly ComponentType _type;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetSupportedAlgorithmsHandler" /> class.
        /// </summary>
        /// <param name="type">Component type.</param>
        public GetSupportedAlgorithmsHandler(ComponentType? type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        ///     Executes this instance.
        /// </summary>
        /// <returns>List of algorithms.</returns>
        public IEnumerable<IAlgorithm> Execute()
        {
            var result = new List<IAlgorithm>
            {
                new Algorithm
                {
                    Type = AlgorithmType.Md5,
                    SupportsOffsets = true,
                    SupportsSeed = false,
                    SupportsSalt = true
                },
                new Algorithm
                {
                    Type = AlgorithmType.Sha1,
                    SupportsOffsets = true,
                    SupportsSeed = false,
                    SupportsSalt = true
                },
                new Algorithm
                {
                    Type = AlgorithmType.Sha256,
                    SupportsOffsets = true,
                    SupportsSeed = false,
                    SupportsSalt = true
                },
                new Algorithm
                {
                    Type = AlgorithmType.Sha384,
                    SupportsOffsets = true,
                    SupportsSeed = false,
                    SupportsSalt = true
                },
                new Algorithm
                {
                    Type = AlgorithmType.Sha512,
                    SupportsOffsets = true,
                    SupportsSeed = false,
                    SupportsSalt = true
                }
                //// TODO: Not supported by the IGT host.  Need to add this based on the host version
                ////new Algorithm
                ////{
                ////    Type = AlgorithmType.HmacSha1,
                ////    SupportsOffsets = false,
                ////    SupportsSeed = false,
                ////    SupportsSalt = true
                ////}
            };

            if (_type == ComponentType.Hardware)
            {
                result.Add(new Algorithm
                {
                    Type = AlgorithmType.Crc32,
                    SupportsOffsets = false,
                    SupportsSalt = false,
                    SupportsSeed = true
                });
            }

            return result;
        }
    }
}
