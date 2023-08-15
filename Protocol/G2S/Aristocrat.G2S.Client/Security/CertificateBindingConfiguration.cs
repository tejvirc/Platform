namespace Aristocrat.G2S.Client.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Runtime.InteropServices;
    using Monaco.NativeSerial;

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

            HttpServiceProvider.CallHttpApi(
                () =>
                {
                    uint token = 0;

                    uint retVal;
                    do
                    {
                        var query = new HttpServiceProvider.HttpServiceConfigSslQuery
                        {
                            QueryDesc = HttpServiceProvider.HttpServiceConfigQueryType.HttpServiceConfigQueryNext,
                            Token = token
                        };

                        var pInputConfigInfo = Marshal.AllocCoTaskMem(
                            Marshal.SizeOf(typeof(HttpServiceProvider.HttpServiceConfigSslQuery)));
                        Marshal.StructureToPtr(query, pInputConfigInfo, false);

                        var pOutputConfigInfo = IntPtr.Zero;
                        var returnLength = 0;

                        try
                        {
                            var inputConfigInfoSize = Marshal.SizeOf(query);
                            retVal = HttpServiceProvider.HttpQueryServiceConfiguration(
                                IntPtr.Zero,
                                HttpServiceProvider.HttpServiceConfigId.HttpServiceConfigSslCertInfo,
                                pInputConfigInfo,
                                inputConfigInfoSize,
                                pOutputConfigInfo,
                                returnLength,
                                out returnLength,
                                IntPtr.Zero);
                            if (retVal == HttpServiceProvider.ErrorNoMoreItems)
                            {
                                break;
                            }

                            if (retVal == HttpServiceProvider.ErrorInsufficientBuffer)
                            {
                                pOutputConfigInfo = Marshal.AllocCoTaskMem(returnLength);

                                try
                                {
                                    retVal = HttpServiceProvider.HttpQueryServiceConfiguration(
                                        IntPtr.Zero,
                                        HttpServiceProvider.HttpServiceConfigId.HttpServiceConfigSslCertInfo,
                                        pInputConfigInfo,
                                        inputConfigInfoSize,
                                        pOutputConfigInfo,
                                        returnLength,
                                        out returnLength,
                                        IntPtr.Zero);
                                    HttpServiceProvider.ThrowWin32ExceptionIfError(retVal);

                                    var outputConfigInfo =
                                        (HttpServiceProvider.HttpServiceConfigSslSet)Marshal.PtrToStructure(
                                            pOutputConfigInfo,
                                            typeof(HttpServiceProvider.HttpServiceConfigSslSet));
                                    var resultItem = CreateCertificateBindingInfo(outputConfigInfo);
                                    result.Add(resultItem);
                                    token++;
                                }
                                finally
                                {
                                    Marshal.FreeCoTaskMem(pOutputConfigInfo);
                                }
                            }
                            else
                            {
                                HttpServiceProvider.ThrowWin32ExceptionIfError(retVal);
                            }
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(pInputConfigInfo);
                        }
                    } while (retVal == HttpServiceProvider.NoError);
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
            HttpServiceProvider.CallHttpApi(
                () =>
                {
                    var sockAddressHandle = SockAddressInterop.CreateSockAddressStructure(endpoint);
                    var pIpPort = sockAddressHandle.AddrOfPinnedObject();
                    var sslKey = new HttpServiceProvider.HttpServiceConfigSslKey(pIpPort);

                    var inputConfigInfoQuery = new HttpServiceProvider.HttpServiceConfigSslQuery
                    {
                        QueryDesc = HttpServiceProvider.HttpServiceConfigQueryType.HttpServiceConfigQueryExact,
                        KeyDesc = sslKey
                    };

                    var pInputConfigInfo = Marshal.AllocCoTaskMem(
                        Marshal.SizeOf(typeof(HttpServiceProvider.HttpServiceConfigSslQuery)));
                    Marshal.StructureToPtr(inputConfigInfoQuery, pInputConfigInfo, false);

                    var pOutputConfigInfo = IntPtr.Zero;
                    var returnLength = 0;

                    try
                    {
                        var inputConfigInfoSize = Marshal.SizeOf(inputConfigInfoQuery);
                        retVal = HttpServiceProvider.HttpQueryServiceConfiguration(
                            IntPtr.Zero,
                            HttpServiceProvider.HttpServiceConfigId.HttpServiceConfigSslCertInfo,
                            pInputConfigInfo,
                            inputConfigInfoSize,
                            pOutputConfigInfo,
                            returnLength,
                            out returnLength,
                            IntPtr.Zero);
                        if (retVal == HttpServiceProvider.ErrorFileNotFound)
                        {
                            return;
                        }

                        if (retVal == HttpServiceProvider.ErrorInsufficientBuffer)
                        {
                            pOutputConfigInfo = Marshal.AllocCoTaskMem(returnLength);
                            try
                            {
                                retVal = HttpServiceProvider.HttpQueryServiceConfiguration(
                                    IntPtr.Zero,
                                    HttpServiceProvider.HttpServiceConfigId.HttpServiceConfigSslCertInfo,
                                    pInputConfigInfo,
                                    inputConfigInfoSize,
                                    pOutputConfigInfo,
                                    returnLength,
                                    out returnLength,
                                    IntPtr.Zero);
                                HttpServiceProvider.ThrowWin32ExceptionIfError(retVal);

                                var outputConfigInfo = (HttpServiceProvider.HttpServiceConfigSslSet)Marshal.PtrToStructure(
                                    pOutputConfigInfo,
                                    typeof(HttpServiceProvider.HttpServiceConfigSslSet));
                                result = CreateCertificateBindingInfo(outputConfigInfo);
                            }
                            finally
                            {
                                Marshal.FreeCoTaskMem(pOutputConfigInfo);
                            }
                        }
                        else
                        {
                            HttpServiceProvider.ThrowWin32ExceptionIfError(retVal);
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
            HttpServiceProvider.CallHttpApi(
                () =>
                {
                    var sockAddressHandle = SockAddressInterop.CreateSockAddressStructure(binding.Endpoint);
                    var pIpPort = sockAddressHandle.AddrOfPinnedObject();
                    var httpServiceConfigSslKey = new HttpServiceProvider.HttpServiceConfigSslKey(pIpPort);

                    var hash = GetHash(binding.Thumbprint);
                    var handleHash = GCHandle.Alloc(hash, GCHandleType.Pinned);
                    var options = binding.Options;
                    var configSslParam = new HttpServiceProvider.HttpServiceConfigSslParam
                    {
                        AppId = binding.AppId,
                        DefaultCertCheckMode =
                            (!options.VerifyCertificateRevocation
                                ? HttpServiceProvider.CertCheckModes.DoNotVerifyCertificateRevocation
                                : 0)
                            |
                            (options.VerifyRevocationWithCachedCertificateOnly
                                ? HttpServiceProvider.CertCheckModes.VerifyRevocationWithCachedCertificateOnly
                                : 0)
                            |
                            (options.EnableRevocationFreshnessTime
                                ? HttpServiceProvider.CertCheckModes.EnableRevocationFreshnessTime
                                : 0)
                            | (!options.UsageCheck ? HttpServiceProvider.CertCheckModes.NoUsageCheck : 0),
                        DefaultFlags =
                            (options.NegotiateClientCertificate
                                ? HttpServiceProvider.HttpServiceConfigSslFlag.NegotiateClientCert
                                : 0)
                            | (options.Mapped ? HttpServiceProvider.HttpServiceConfigSslFlag.UseDsMapper : 0)
                            |
                            (options.DoNotPassRequestsToRawFilters
                                ? HttpServiceProvider.HttpServiceConfigSslFlag.NoRawFilter
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

                    var configSslSet = new HttpServiceProvider.HttpServiceConfigSslSet
                    {
                        ParamDesc = configSslParam,
                        KeyDesc = httpServiceConfigSslKey
                    };

                    var pInputConfigInfo = Marshal.AllocCoTaskMem(
                        Marshal.SizeOf(typeof(HttpServiceProvider.HttpServiceConfigSslSet)));
                    Marshal.StructureToPtr(configSslSet, pInputConfigInfo, false);

                    try
                    {
                        var retVal = HttpServiceProvider.HttpSetServiceConfiguration(
                            IntPtr.Zero,
                            HttpServiceProvider.HttpServiceConfigId.HttpServiceConfigSslCertInfo,
                            pInputConfigInfo,
                            Marshal.SizeOf(configSslSet),
                            IntPtr.Zero);

                        if (retVal != HttpServiceProvider.ErrorAlreadyExists)
                        {
                            HttpServiceProvider.ThrowWin32ExceptionIfError(retVal);
                            bindingUpdated = true;
                        }
                        else
                        {
                            retVal = HttpServiceProvider.HttpDeleteServiceConfiguration(
                                IntPtr.Zero,
                                HttpServiceProvider.HttpServiceConfigId.HttpServiceConfigSslCertInfo,
                                pInputConfigInfo,
                                Marshal.SizeOf(configSslSet),
                                IntPtr.Zero);
                            HttpServiceProvider.ThrowWin32ExceptionIfError(retVal);

                            retVal = HttpServiceProvider.HttpSetServiceConfiguration(
                                IntPtr.Zero,
                                HttpServiceProvider.HttpServiceConfigId.HttpServiceConfigSslCertInfo,
                                pInputConfigInfo,
                                Marshal.SizeOf(configSslSet),
                                IntPtr.Zero);
                            HttpServiceProvider.ThrowWin32ExceptionIfError(retVal);
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

            HttpServiceProvider.CallHttpApi(
                () =>
                {
                    foreach (var ipPort in ipEndPoints)
                    {
                        var sockAddressHandle = SockAddressInterop.CreateSockAddressStructure(ipPort);
                        var pIpPort = sockAddressHandle.AddrOfPinnedObject();
                        var httpServiceConfigSslKey =
                            new HttpServiceProvider.HttpServiceConfigSslKey(pIpPort);

                        var configSslSet = new HttpServiceProvider.HttpServiceConfigSslSet
                        {
                            KeyDesc = httpServiceConfigSslKey
                        };

                        var pInputConfigInfo = Marshal.AllocCoTaskMem(
                            Marshal.SizeOf(typeof(HttpServiceProvider.HttpServiceConfigSslSet)));
                        Marshal.StructureToPtr(configSslSet, pInputConfigInfo, false);

                        try
                        {
                            var retVal = HttpServiceProvider.HttpDeleteServiceConfiguration(
                                IntPtr.Zero,
                                HttpServiceProvider.HttpServiceConfigId.HttpServiceConfigSslCertInfo,
                                pInputConfigInfo,
                                Marshal.SizeOf(configSslSet),
                                IntPtr.Zero);
                            HttpServiceProvider.ThrowWin32ExceptionIfError(retVal);
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

        private static CertificateBinding CreateCertificateBindingInfo(HttpServiceProvider.HttpServiceConfigSslSet configInfo)
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
                    !HasFlag(checkModes, HttpServiceProvider.CertCheckModes.DoNotVerifyCertificateRevocation),
                VerifyRevocationWithCachedCertificateOnly =
                    HasFlag(checkModes, HttpServiceProvider.CertCheckModes.VerifyRevocationWithCachedCertificateOnly),
                EnableRevocationFreshnessTime =
                    HasFlag(checkModes, HttpServiceProvider.CertCheckModes.EnableRevocationFreshnessTime),
                UsageCheck = !HasFlag(checkModes, HttpServiceProvider.CertCheckModes.NoUsageCheck),
                RevocationFreshnessTime = TimeSpan.FromSeconds(configInfo.ParamDesc.DefaultRevocationFreshnessTime),
                UrlRetrievalTimeout =
                    TimeSpan.FromMilliseconds(configInfo.ParamDesc.DefaultRevocationUrlRetrievalTimeout),
                SslControlIdentifier = configInfo.ParamDesc.DefaultSslCtlIdentifier,
                SslControlStoreName = configInfo.ParamDesc.DefaultSslCtlStoreName,
                NegotiateClientCertificate =
                    HasFlag(
                        configInfo.ParamDesc.DefaultFlags,
                        HttpServiceProvider.HttpServiceConfigSslFlag.NegotiateClientCert),
                Mapped =
                    HasFlag(configInfo.ParamDesc.DefaultFlags, HttpServiceProvider.HttpServiceConfigSslFlag.UseDsMapper),
                DoNotPassRequestsToRawFilters =
                    HasFlag(configInfo.ParamDesc.DefaultFlags, HttpServiceProvider.HttpServiceConfigSslFlag.NoRawFilter)
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