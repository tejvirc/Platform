namespace Aristocrat.Monaco.G2S.Tests.Options
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig.ChangeOptionConfig;
    using G2S.Options;
    using G2S.Services;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using DeviceClass = Aristocrat.G2S.DeviceClass;

    [TestClass]
    public class ApplyOptionConfigToDownloadDeviceTest : BaseApplyOptionConfigToDeviceTest
    {
        private const int NoMessageTimer = 60000;

        private const int DefaultMinPackageLogEntries = 35;

        private const int NewMinPackageLogEntries = 36;

        private const int DefaultMinScriptLogEntries = 35;

        private const int NewMinScriptLogEntries = 36;

        private const int DefaultMinPackageListEntries = 10;

        private const int NewMinPackageListEntries = 37;

        private const int DefaultMinScriptListEntries = 10;

        private const int NewMinScriptListEntries = 38;

        private const int DefaultAuthenticationWaitTimeOut = 60000;

        private const int NewAuthenticationWaitTimeOut = 70000;

        private const int DefaultAuthenticationWaitRetries = 2;

        private const int NewAuthenticationWaitRetries = 5;

        private const int TransferProgressFrequency = 90000;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);

            var properties = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Loose);

            properties.Setup(m => m.GetProperty(ApplicationConstants.ReadOnlyMediaRequired, false)).Returns(false);
        }

        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void WhenApplyExpectSuccess()
        {
            var device = new DownloadDevice(_deviceObserverMock.Object, true);

            Assert.AreEqual(device.ConfigurationId, 0);
            Assert.IsTrue(device.RestartStatus);
            Assert.IsFalse(device.UseDefaultConfig);
            Assert.IsFalse(device.RequiredForPlay);
            Assert.AreEqual(device.TimeToLive, DefaultTimeToLive);
            Assert.AreEqual(device.NoResponseTimer, new TimeSpan(0, 0, 0));
            Assert.AreEqual(device.NoMessageTimer, 0);
            Assert.AreEqual(device.MinPackageLogEntries, DefaultMinPackageLogEntries);
            Assert.AreEqual(device.MinScriptLogEntries, DefaultMinScriptLogEntries);
            Assert.AreEqual(device.MinPackageListEntries, DefaultMinPackageListEntries);
            Assert.AreEqual(device.MinScriptListEntries, DefaultMinScriptListEntries);
            Assert.AreEqual(device.AuthenticationWaitTimeOut, DefaultAuthenticationWaitTimeOut);
            Assert.AreEqual(device.AuthenticationWaitRetries, DefaultAuthenticationWaitRetries);
            Assert.IsTrue(device.DownloadEnabled);
            Assert.IsTrue(device.UploadEnabled);
            Assert.IsTrue(device.ScriptingEnabled);
            Assert.IsFalse(device.ProtocolListSupport);
            Assert.AreEqual(device.TransferProgressFrequency, 0);
            Assert.IsFalse(device.PauseSupported);
            Assert.IsFalse(device.AbortTransferSupported);
            Assert.IsTrue(device.ConfigComplete);
            Assert.AreEqual(device.ConfigDateTime, default(DateTime));

            _egmMock.Setup(x => x.GetDevice(DeviceClass.G2S_download.TrimmedDeviceClass(), DeviceId))
                .Returns(device);

            var service = new ApplyOptionConfigToDevicesService(
                _egmMock.Object,
                _profileServiceMock.Object,
                new List<IDeviceOptions>
                {
                    new DownloadDeviceOptions()
                });

            service.UpdateDeviceProfiles(CreateChangeOptionConfigRequest(DeviceClass.G2S_download));

            Assert.AreEqual(device.ConfigurationId, ConfigurationId);
            Assert.IsFalse(device.RestartStatus);
            Assert.IsTrue(device.UseDefaultConfig);
            Assert.IsTrue(device.RequiredForPlay);
            Assert.AreEqual(device.TimeToLive, NewTimeToLive);
            Assert.AreEqual(device.NoResponseTimer, TimeSpan.FromMilliseconds(NewNoResponseTimer));
            Assert.AreEqual(device.NoMessageTimer, NoMessageTimer);
            Assert.AreEqual(device.MinPackageLogEntries, NewMinPackageLogEntries);
            Assert.AreEqual(device.MinScriptLogEntries, NewMinScriptLogEntries);
            Assert.AreEqual(device.MinPackageListEntries, NewMinPackageListEntries);
            Assert.AreEqual(device.MinScriptListEntries, NewMinScriptListEntries);
            Assert.AreEqual(device.AuthenticationWaitTimeOut, NewAuthenticationWaitTimeOut);
            Assert.AreEqual(device.AuthenticationWaitRetries, NewAuthenticationWaitRetries);
            Assert.IsFalse(device.DownloadEnabled);
            Assert.IsFalse(device.UploadEnabled);
            Assert.IsFalse(device.ScriptingEnabled);
            Assert.IsTrue(device.ProtocolListSupport);
            Assert.AreEqual(device.TransferProgressFrequency, TransferProgressFrequency);
            Assert.IsTrue(device.PauseSupported);
            Assert.IsTrue(device.AbortTransferSupported);

            _profileServiceMock.Verify(
                x => x.Save(It.Is<IDevice>(d => d.Id == DeviceId && d.DeviceClass == typeof(download).Name)),
                Times.Once);
        }

        protected override OptionCurrentValue CreateOptionCurrentValue()
        {
            return new OptionCurrentValue
            {
                ParameterType = OptionConfigParameterType.Complex,
                ChildValues = new List<OptionCurrentValue>
                {
                    new OptionCurrentValue
                    {
                        ParamId = "G2S_configurationId",
                        Value = ConfigurationId.ToString(),
                        ParameterType = OptionConfigParameterType.Integer
                    },
                    new OptionCurrentValue
                    {
                        ParameterType = OptionConfigParameterType.Complex,
                        ChildValues = new List<OptionCurrentValue>
                        {
                            new OptionCurrentValue
                            {
                                ParamId = "G2S_restartStatus",
                                Value = "false",
                                ParameterType = OptionConfigParameterType.Boolean
                            },
                            new OptionCurrentValue
                            {
                                ParameterType = OptionConfigParameterType.Complex,
                                ChildValues = new List<OptionCurrentValue>
                                {
                                    new OptionCurrentValue
                                    {
                                        ParamId = "G2S_useDefaultConfig",
                                        Value = "true",
                                        ParameterType = OptionConfigParameterType.Boolean
                                    },
                                    new OptionCurrentValue
                                    {
                                        ParameterType = OptionConfigParameterType.Complex,
                                        ChildValues = new List<OptionCurrentValue>
                                        {
                                            new OptionCurrentValue
                                            {
                                                ParamId = "G2S_requiredForPlay",
                                                Value = "true",
                                                ParameterType = OptionConfigParameterType.Boolean
                                            },
                                            new OptionCurrentValue
                                            {
                                                ParameterType = OptionConfigParameterType.Complex,
                                                ChildValues = new List<OptionCurrentValue>
                                                {
                                                    new OptionCurrentValue
                                                    {
                                                        ParamId = "G2S_timeToLive",
                                                        Value = NewTimeToLive.ToString(),
                                                        ParameterType = OptionConfigParameterType.Integer
                                                    },
                                                    new OptionCurrentValue
                                                    {
                                                        ParameterType = OptionConfigParameterType.Complex,
                                                        ChildValues = new List<OptionCurrentValue>
                                                        {
                                                            new OptionCurrentValue
                                                            {
                                                                ParamId = "G2S_noResponseTimer",
                                                                Value = NewNoResponseTimer.ToString(),
                                                                ParameterType = OptionConfigParameterType.Integer
                                                            },
                                                            new OptionCurrentValue
                                                            {
                                                                ParamId = "G2S_noMessageTimer",
                                                                Value = NoMessageTimer.ToString(),
                                                                ParameterType = OptionConfigParameterType.Integer
                                                            },
                                                            new OptionCurrentValue
                                                            {
                                                                ParamId = "G2S_minPackageLogEntries",
                                                                Value = NewMinPackageLogEntries.ToString()
                                                            },
                                                            new OptionCurrentValue
                                                            {
                                                                ParamId = "G2S_minScriptLogEntries",
                                                                Value = NewMinScriptLogEntries.ToString()
                                                            },
                                                            new OptionCurrentValue
                                                            {
                                                                ParamId = "G2S_minPackageListEntries",
                                                                Value = NewMinPackageListEntries.ToString()
                                                            },
                                                            new OptionCurrentValue
                                                            {
                                                                ParamId = "G2S_minScriptListEntries",
                                                                Value = NewMinScriptListEntries.ToString()
                                                            },
                                                            new OptionCurrentValue
                                                            {
                                                                ParamId = "G2S_authWaitTimeOut",
                                                                Value = NewAuthenticationWaitTimeOut.ToString()
                                                            },
                                                            new OptionCurrentValue
                                                            {
                                                                ParamId = "G2S_authWaitRetry",
                                                                Value = NewAuthenticationWaitRetries.ToString()
                                                            },
                                                            new OptionCurrentValue
                                                            {
                                                                ParamId = "G2S_downloadEnabled",
                                                                Value = "false"
                                                            },
                                                            new OptionCurrentValue
                                                            {
                                                                ParamId = "G2S_uploadEnabled",
                                                                Value = "false"
                                                            },
                                                            new OptionCurrentValue
                                                            {
                                                                ParamId = "G2S_scriptingEnabled",
                                                                Value = "false"
                                                            },
                                                            new OptionCurrentValue
                                                            {
                                                                ParamId = "G2S_protocolListSupport",
                                                                Value = "true"
                                                            },
                                                            new OptionCurrentValue
                                                            {
                                                                ParamId = "GTK_transferProgressFreq",
                                                                Value = TransferProgressFrequency.ToString()
                                                            },
                                                            new OptionCurrentValue
                                                            {
                                                                ParamId = "GTK_pauseSupported",
                                                                Value = "true"
                                                            },
                                                            new OptionCurrentValue
                                                            {
                                                                ParamId = "GTK_abortTransferSupported",
                                                                Value = "true"
                                                            },
                                                            CreateCommonOptionCurrentValue()
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}