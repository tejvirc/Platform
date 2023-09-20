namespace Aristocrat.Monaco.G2S.Handlers.Gat
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using Application.Contracts.Authentication;
    using Aristocrat.G2S.Protocol.v21;
    using Common.GAT.Storage;
    using Kernel.Contracts.Components;

    /// <summary>
    ///     GAT Enum Extensions
    /// </summary>
    internal static class GatEnumExtensions
    {
        /// <summary>
        ///     Converts a component verification state to the G2S equivalent.
        /// </summary>
        /// <param name="this">The ComponentVerificationState.</param>
        /// <returns>A verification status</returns>
        /// <exception cref="InvalidEnumArgumentException">if enum is not defined</exception>
        /// <exception cref="ArgumentOutOfRangeException">null</exception>
        public static t_verifyStates ToG2SVerifyState(this ComponentVerificationState @this)
        {
            if (!Enum.IsDefined(typeof(ComponentVerificationState), @this))
            {
                throw new InvalidEnumArgumentException(nameof(@this), (int)@this, typeof(ComponentVerificationState));
            }

            switch (@this)
            {
                case ComponentVerificationState.Queued:
                    return t_verifyStates.G2S_queued;
                case ComponentVerificationState.InProcess:
                    return t_verifyStates.G2S_inProcess;
                case ComponentVerificationState.Complete:
                    return t_verifyStates.G2S_complete;
                case ComponentVerificationState.Error:
                    return t_verifyStates.G2S_error;
                case ComponentVerificationState.Passed:
                    return t_verifyStates.G2S_passed;
                case ComponentVerificationState.Failed:
                    return t_verifyStates.G2S_failed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        /// <summary>
        ///     Converts a component type to G2S compatible component type.
        /// </summary>
        /// <param name="this">The component type.</param>
        /// <returns>string representation of component type</returns>
        /// <exception cref="InvalidEnumArgumentException">if enum is not defined</exception>
        public static string ToG2SComponentType(this ComponentType @this)
        {
            if (!Enum.IsDefined(typeof(ComponentType), @this))
            {
                throw new InvalidEnumArgumentException(nameof(@this), (int)@this, typeof(ComponentType));
            }

            return @this == ComponentType.OS
                ? $"G2S_{ComponentType.Module.ToString().ToLowerInvariant()}"
                : $"G2S_{@this.ToString().ToLowerInvariant()}";
        }

        /// <summary>
        ///     Converts a function type to the G2S equivalent.
        /// </summary>
        /// <param name="this">The FunctionType.</param>
        /// <returns>G2S Function Type</returns>
        /// <exception cref="InvalidEnumArgumentException">if enum is not defined</exception>
        /// <exception cref="ArgumentOutOfRangeException">null</exception>
        public static t_functionTypes ToG2SFunctionType(this FunctionType @this)
        {
            if (!Enum.IsDefined(typeof(FunctionType), @this))
            {
                throw new InvalidEnumArgumentException(nameof(@this), (int)@this, typeof(FunctionType));
            }

            switch (@this)
            {
                case FunctionType.DoVerification:
                    return t_functionTypes.G2S_doVerification;
                case FunctionType.RunSpecialFunction:
                    return t_functionTypes.G2S_runSpecialFunction;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        /// <summary>
        ///     Converts the G2S algorithm to an Algorithm type.
        /// </summary>
        /// <param name="this">The G2S algorithm.</param>
        /// <returns>AlgorithmType from string</returns>
        /// <exception cref="ArgumentNullException">if @this is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">null</exception>
        public static AlgorithmType ToAlgorithmType(this string @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            switch (@this)
            {
                case "G2S_MD5":
                    return AlgorithmType.Md5;
                case "G2S_SHA1":
                    return AlgorithmType.Sha1;
                case "G2S_SHA256":
                    return AlgorithmType.Sha256;
                case "G2S_SHA384":
                    return AlgorithmType.Sha384;
                case "G2S_SHA512":
                    return AlgorithmType.Sha512;
                case "G2S_HMACSHA1":
                    return AlgorithmType.HmacSha1;
                case "G2S_CRC32":
                    return AlgorithmType.Crc32;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }

        /// <summary>
        ///     Converts the AlgorithmType to a G2S Algorithm type.
        /// </summary>
        /// <param name="this">The algorithm type.</param>
        /// <returns>AlgorithmType from string</returns>
        public static string ToG2SAlgorithmType(this AlgorithmType @this)
        {
            return $"G2S_{@this.ToString().ToUpperInvariant()}";
        }

        /// <summary>
        ///     Converts GAT verification request to a gat log.
        /// </summary>
        /// <param name="request">Verification request.</param>
        /// <returns>gat log</returns>
        public static gatLog GetGatLog(GatVerificationRequest request)
        {
            return new gatLog
            {
                logSequence = request.Id,
                deviceId = request.DeviceId,
                transactionId = request.TransactionId,
                functionType = request.FunctionType.ToG2SFunctionType(),
                verificationId = request.VerificationId,
                feature = string.Empty, ////TODO we're not supporting this yet
                parameter = string.Empty, ////TODO we're not supporting this yet
                employeeId = request.EmployeeId,
                gatDateTime = request.Date.UtcDateTime,
                componentLog = request.ComponentVerifications?.Select(
                    m => new componentLog
                    {
                        algorithmType = m.AlgorithmType.ToG2SAlgorithmType(),
                        componentId = m.ComponentId,
                        endOffset = m.EndOffset,
                        gatExec = m.GatExec,
                        salt = m.Salt ?? new byte[0],
                        seed = m.Seed.Length > 0 ? BitConverter.ToUInt32(m.Seed, 0).ToString() : string.Empty,
                        startOffset = m.StartOffset,
                        verifyResult =
                            ConvertExtensions.ToGatResultString(m.Result, m.AlgorithmType != AlgorithmType.Crc32),
                        verifyState = m.State.ToG2SVerifyState()
                    }).ToArray()
            };
        }
    }
}