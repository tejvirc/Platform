namespace Aristocrat.G2S.Client.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Certificate binding configuration
    /// </summary>
    public class CertificateBindingConfiguration
    {
        /// <summary>
        ///     Queries for the certificate binding
        /// </summary>
        /// <returns>A certificate binding collection</returns>
        public IEnumerable<CertificateBinding> GetAll()
        {
            var result = new List<CertificateBinding>();

            NativeMethods.CallHttpApi(
                () =>
                {
                    uint token = 0;

                    uint retVal;
                    do
                    {
                        var query = new NativeMethods.HttpServiceConfigSslQuery
                        {
                            QueryDesc = NativeMethods.HttpServiceConfigQueryType.HttpServiceConfigQueryNext,
                            Token = token
                        };

                        var pInputConfigInfo = Marshal.AllocCoTaskMem(
                            Marshal.SizeOf(typeof(NativeMethods.HttpServiceConfigSslQuery)));
                        Marshal.StructureToPtr(query, pInputConfigInfo, false);

                        var pOutputConfigInfo = IntPtr.Zero;
                        var returnLength = 0;

                        try
                        {
                            var inputConfigInfoSize = Marshal.SizeOf(query);
                            retVal = NativeMethods.HttpQueryServiceConfiguration(
                                IntPtr.Zero,
                                NativeMethods.HttpServiceConfigId.HttpServiceConfigSslCertInfo,
                                pInputConfigInfo,
                                inputConfigInfoSize,
                                pOutputConfigInfo,
                                returnLength,
                                out returnLength,
                                IntPtr.Zero);
                            if (retVal == NativeMethods.ErrorNoMoreItems)
                            {
                                break;
                            }

                            if (retVal == NativeMethods.ErrorInsufficientBuffer)
                            {
                                pOutputConfigInfo = Marshal.AllocCoTaskMem(returnLength);

                                try
                                {
                                    retVal = NativeMethods.HttpQueryServiceConfiguration(
                                        IntPtr.Zero,
                                        NativeMethods.HttpServiceConfigId.HttpServiceConfigSslCertInfo,
                                        pInputConfigInfo,
                                        inputConfigInfoSize,
                                        pOutputConfigInfo,
                                        returnLength,
                                        out returnLength,
                                        IntPtr.Zero);
                                    NativeMethods.ThrowWin32ExceptionIfError(retVal);

                                    var outputConfigInfo =
                                        (NativeMethods.HttpServiceConfigSslSet?)Marshal.PtrToStructure(
                                            pOutputConfigInfo,
                                            typeof(NativeMethods.HttpServiceConfigSslSet));
                                    if (outputConfigInfo.HasValue)
                                    {
                                        var resultItem = CreateCertificateBindingInfo(outputConfigInfo.Value);
                                        result.Add(resultItem);
                                    }
                                    token++;
                                }
                                finally
                                {
                                    Marshal.FreeCoTaskMem(pOutputConfigInfo);
                                }
                            }
                            else
                            {
                                NativeMethods.ThrowWin32ExceptionIfError(retVal);
                            }
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(pInputConfigInfo);
                        }
                    } while (retVal == NativeMethods.NoError);
                });

            return result;
        }

        /// <summary>
        ///     Queries for the certificate binding
        /// </summary>
        /// <param name="endpoint">The ip port</param>
        /// <returns>A certificate binding</returns>
        public CertificateBinding Get(IPEndPoint endpoint)
        {
            CertificateBinding result = null;

            uint retVal;
            NativeMethods.CallHttpApi(
                () =>
                {
                    var sockAddressHandle = SockAddressInterop.CreateSockAddressStructure(endpoint);
                    var pIpPort = sockAddressHandle.AddrOfPinnedObject();
                    var sslKey = new NativeMethods.HttpServiceConfigSslKey(pIpPort);

                    var inputConfigInfoQuery = new NativeMethods.HttpServiceConfigSslQuery
                    {
                        QueryDesc = NativeMethods.HttpServiceConfigQueryType.HttpServiceConfigQueryExact,
                        KeyDesc = sslKey
                    };

                    var pInputConfigInfo = Marshal.AllocCoTaskMem(
                        Marshal.SizeOf(typeof(NativeMethods.HttpServiceConfigSslQuery)));
                    Marshal.StructureToPtr(inputConfigInfoQuery, pInputConfigInfo, false);

                    var pOutputConfigInfo = IntPtr.Zero;
                    var returnLength = 0;

                    try
                    {
                        var inputConfigInfoSize = Marshal.SizeOf(inputConfigInfoQuery);
                        retVal = NativeMethods.HttpQueryServiceConfiguration(
                            IntPtr.Zero,
                            NativeMethods.HttpServiceConfigId.HttpServiceConfigSslCertInfo,
                            pInputConfigInfo,
                            inputConfigInfoSize,
                            pOutputConfigInfo,
                            returnLength,
                            out returnLength,
                            IntPtr.Zero);
                        if (retVal == NativeMethods.ErrorFileNotFound)
                        {
                            return;
                        }

                        if (retVal == NativeMethods.ErrorInsufficientBuffer)
                        {
                            pOutputConfigInfo = Marshal.AllocCoTaskMem(returnLength);
                            try
                            {
                                retVal = NativeMethods.HttpQueryServiceConfiguration(
                                    IntPtr.Zero,
                                    NativeMethods.HttpServiceConfigId.HttpServiceConfigSslCertInfo,
                                    pInputConfigInfo,
                                    inputConfigInfoSize,
                                    pOutputConfigInfo,
                                    returnLength,
                                    out returnLength,
                                    IntPtr.Zero);
                                NativeMethods.ThrowWin32ExceptionIfError(retVal);

                                var outputConfigInfo = (NativeMethods.HttpServiceConfigSslSet?)Marshal.PtrToStructure(
                                    pOutputConfigInfo,
                                    typeof(NativeMethods.HttpServiceConfigSslSet));
                                if (outputConfigInfo != null)
                                    result = CreateCertificateBindingInfo(outputConfigInfo.Value);
                            }
                            finally
                            {
                                Marshal.FreeCoTaskMem(pOutputConfigInfo);
                            }
                        }
                        else
                        {
                            NativeMethods.ThrowWin32ExceptionIfError(retVal);
                        }
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(pInputConfigInfo);
                        if (sockAddressHandle.IsAllocated)
                        {
                            sockAddressHandle.Free();
                        }
                    }
                });

            return result;
        }

        /// <summary>
        ///     Binds the certificate
        /// </summary>
        /// <param name="binding">The binding</param>
        /// <returns>true if successful</returns>
        public bool Bind(CertificateBinding binding)
        {
            var bindingUpdated = false;
            NativeMethods.CallHttpApi(
                () =>
                {
                    var sockAddressHandle = SockAddressInterop.CreateSockAddressStructure(binding.Endpoint);
                    var pIpPort = sockAddressHandle.AddrOfPinnedObject();
                    var httpServiceConfigSslKey = new NativeMethods.HttpServiceConfigSslKey(pIpPort);

                    var hash = GetHash(binding.Thumbprint);
                    var handleHash = GCHandle.Alloc(hash, GCHandleType.Pinned);
                    var options = binding.Options;
                    var configSslParam = new NativeMethods.HttpServiceConfigSslParam
                    {
                        AppId = binding.AppId,
                        DefaultCertCheckMode =
                            (!options.VerifyCertificateRevocation
                                ? NativeMethods.CertCheckModes.DoNotVerifyCertificateRevocation
                                : 0)
                            |
                            (options.VerifyRevocationWithCachedCertificateOnly
                                ? NativeMethods.CertCheckModes.VerifyRevocationWithCachedCertificateOnly
                                : 0)
                            |
                            (options.EnableRevocationFreshnessTime
                                ? NativeMethods.CertCheckModes.EnableRevocationFreshnessTime
                                : 0)
                            | (!options.UsageCheck ? NativeMethods.CertCheckModes.NoUsageCheck : 0),
                        DefaultFlags =
                            (options.NegotiateClientCertificate
                                ? NativeMethods.HttpServiceConfigSslFlag.NegotiateClientCert
                                : 0)
                            | (options.Mapped ? NativeMethods.HttpServiceConfigSslFlag.UseDsMapper : 0)
                            |
                            (options.DoNotPassRequestsToRawFilters
                                ? NativeMethods.HttpServiceConfigSslFlag.NoRawFilter
                                : 0),
                        DefaultRevocationFreshnessTime = (int)options.RevocationFreshnessTime.TotalSeconds,
                        DefaultRevocationUrlRetrievalTimeout =
                            (int)options.UrlRetrievalTimeout.TotalMilliseconds,
                        SslCertStoreName = binding.StoreName,
                        SslHash = handleHash.AddrOfPinnedObject(),
                        SslHashLength = hash.Length,
                        DefaultSslCtlIdentifier = options.SslControlIdentifier,
                        DefaultSslCtlStoreName = options.SslControlStoreName
                    };

                    var configSslSet = new NativeMethods.HttpServiceConfigSslSet
                    {
                        ParamDesc = configSslParam,
                        KeyDesc = httpServiceConfigSslKey
                    };

                    var pInputConfigInfo = Marshal.AllocCoTaskMem(
                        Marshal.SizeOf(typeof(NativeMethods.HttpServiceConfigSslSet)));
                    Marshal.StructureToPtr(configSslSet, pInputConfigInfo, false);

                    try
                    {
                        var retVal = NativeMethods.HttpSetServiceConfiguration(
                            IntPtr.Zero,
                            NativeMethods.HttpServiceConfigId.HttpServiceConfigSslCertInfo,
                            pInputConfigInfo,
                            Marshal.SizeOf(configSslSet),
                            IntPtr.Zero);

                        if (retVal != NativeMethods.ErrorAlreadyExists)
                        {
                            NativeMethods.ThrowWin32ExceptionIfError(retVal);
                            bindingUpdated = true;
                        }
                        else
                        {
                            retVal = NativeMethods.HttpDeleteServiceConfiguration(
                                IntPtr.Zero,
                                NativeMethods.HttpServiceConfigId.HttpServiceConfigSslCertInfo,
                                pInputConfigInfo,
                                Marshal.SizeOf(configSslSet),
                                IntPtr.Zero);
                            NativeMethods.ThrowWin32ExceptionIfError(retVal);

                            retVal = NativeMethods.HttpSetServiceConfiguration(
                                IntPtr.Zero,
                                NativeMethods.HttpServiceConfigId.HttpServiceConfigSslCertInfo,
                                pInputConfigInfo,
                                Marshal.SizeOf(configSslSet),
                                IntPtr.Zero);
                            NativeMethods.ThrowWin32ExceptionIfError(retVal);
                            bindingUpdated = true;
                        }
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(pInputConfigInfo);
                        if (handleHash.IsAllocated)
                        {
                            handleHash.Free();
                        }

                        if (sockAddressHandle.IsAllocated)
                        {
                            sockAddressHandle.Free();
                        }
                    }
                });

            return bindingUpdated;
        }

        /// <summary>
        ///     Deletes a binding
        /// </summary>
        /// <param name="endPoint">The endpoint</param>
        public void Delete(IPEndPoint endPoint)
        {
            Delete(new List<IPEndPoint> { endPoint });
        }

        /// <summary>
        ///     Deletes a collection of bindings
        /// </summary>
        /// <param name="endPoints">The endpoints</param>
        public void Delete(IEnumerable<IPEndPoint> endPoints)
        {
            if (endPoints == null)
            {
                throw new ArgumentNullException(nameof(endPoints));
            }

            var ipEndPoints = endPoints as IList<IPEndPoint> ?? endPoints.ToList();

            if (!ipEndPoints.Any())
            {
                return;
            }

            NativeMethods.CallHttpApi(
                () =>
                {
                    foreach (var ipPort in ipEndPoints)
                    {
                        var sockAddressHandle = SockAddressInterop.CreateSockAddressStructure(ipPort);
                        var pIpPort = sockAddressHandle.AddrOfPinnedObject();
                        var httpServiceConfigSslKey =
                            new NativeMethods.HttpServiceConfigSslKey(pIpPort);

                        var configSslSet = new NativeMethods.HttpServiceConfigSslSet
                        {
                            KeyDesc = httpServiceConfigSslKey
                        };

                        var pInputConfigInfo = Marshal.AllocCoTaskMem(
                            Marshal.SizeOf(typeof(NativeMethods.HttpServiceConfigSslSet)));
                        Marshal.StructureToPtr(configSslSet, pInputConfigInfo, false);

                        try
                        {
                            var retVal = NativeMethods.HttpDeleteServiceConfiguration(
                                IntPtr.Zero,
                                NativeMethods.HttpServiceConfigId.HttpServiceConfigSslCertInfo,
                                pInputConfigInfo,
                                Marshal.SizeOf(configSslSet),
                                IntPtr.Zero);
                            NativeMethods.ThrowWin32ExceptionIfError(retVal);
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(pInputConfigInfo);
                            if (sockAddressHandle.IsAllocated)
                            {
                                sockAddressHandle.Free();
                            }
                        }
                    }
                });
        }

        private static CertificateBinding CreateCertificateBindingInfo(NativeMethods.HttpServiceConfigSslSet configInfo)
        {
            var hash = new byte[configInfo.ParamDesc.SslHashLength];
            Marshal.Copy(configInfo.ParamDesc.SslHash, hash, 0, hash.Length);
            var appId = configInfo.ParamDesc.AppId;
            var storeName = configInfo.ParamDesc.SslCertStoreName;
            var ipPort = SockAddressInterop.ReadSockAddressStructure(configInfo.KeyDesc.IpPort);
            var checkModes = configInfo.ParamDesc.DefaultCertCheckMode;

            var options = new BindingOptions
            {
                VerifyCertificateRevocation =
                    !HasFlag(checkModes, NativeMethods.CertCheckModes.DoNotVerifyCertificateRevocation),
                VerifyRevocationWithCachedCertificateOnly =
                    HasFlag(checkModes, NativeMethods.CertCheckModes.VerifyRevocationWithCachedCertificateOnly),
                EnableRevocationFreshnessTime =
                    HasFlag(checkModes, NativeMethods.CertCheckModes.EnableRevocationFreshnessTime),
                UsageCheck = !HasFlag(checkModes, NativeMethods.CertCheckModes.NoUsageCheck),
                RevocationFreshnessTime = TimeSpan.FromSeconds(configInfo.ParamDesc.DefaultRevocationFreshnessTime),
                UrlRetrievalTimeout =
                    TimeSpan.FromMilliseconds(configInfo.ParamDesc.DefaultRevocationUrlRetrievalTimeout),
                SslControlIdentifier = configInfo.ParamDesc.DefaultSslCtlIdentifier,
                SslControlStoreName = configInfo.ParamDesc.DefaultSslCtlStoreName,
                NegotiateClientCertificate =
                    HasFlag(
                        configInfo.ParamDesc.DefaultFlags,
                        NativeMethods.HttpServiceConfigSslFlag.NegotiateClientCert),
                Mapped =
                    HasFlag(configInfo.ParamDesc.DefaultFlags, NativeMethods.HttpServiceConfigSslFlag.UseDsMapper),
                DoNotPassRequestsToRawFilters =
                    HasFlag(configInfo.ParamDesc.DefaultFlags, NativeMethods.HttpServiceConfigSslFlag.NoRawFilter)
            };

            return new CertificateBinding(GetThumbprint(hash), storeName, ipPort, appId, options);
        }

        private static string GetThumbprint(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        private static byte[] GetHash(string thumbprint)
        {
            var length = thumbprint.Length;
            var bytes = new byte[length / 2];

            for (var i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(thumbprint.Substring(i, 2), 16);
            }

            return bytes;
        }

        private static bool HasFlag(uint value, uint flag)
        {
            return (value & flag) == flag;
        }

        private static bool HasFlag<T>(T value, T flag)
            where T : IConvertible
        {
            var uintValue = Convert.ToUInt32(value);
            var uintFlag = Convert.ToUInt32(flag);
            return HasFlag(uintValue, uintFlag);
        }
    }
}