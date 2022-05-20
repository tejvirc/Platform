namespace Aristocrat.Monaco.Application.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Serialization;
    using Cabinet.Contracts;
    using Contracts;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class CabinetFeaturesPropertiesProviderTest
    {
        private Mock<IPersistentStorageManager> _storageManager;
        private Mock<IPersistentStorageAccessor> _accessor;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ICabinetDetectionService> _cabinetDetection;
        private Features _features;

        private const string CabinetFeaturesXml = @".\CabinetFeatures.xml";

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Strict);
            _storageManager = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Default);
            _accessor = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            _propertiesManager.Setup(p => p.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);
            _propertiesManager.Setup(
                m => m.AddPropertyProvider(It.IsAny<CabinetFeaturesPropertiesProvider>()));

            _cabinetDetection = MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Strict);
            _cabinetDetection.Setup(c => c.Type).Returns(It.IsAny<CabinetType>());

            var storageName = typeof(CabinetFeaturesPropertiesProvider).ToString();
            _storageManager.Setup(m => m.BlockExists(It.Is<string>(s => s == storageName))).Returns(true); _storageManager.Setup(m => m.GetBlock(It.Is<string>(s => s == storageName))).Returns(_accessor.Object);
            _accessor.Setup(a => a[ApplicationConstants.ScreenBrightness]).Returns(100);
            _accessor.Setup(a => a[ApplicationConstants.MaximumAllowedEdgeLightingBrightnessKey]).Returns(100);
           var sr = new StreamReader(CabinetFeaturesXml);

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sr.ReadToEnd())))
            {
                var serializer = new XmlSerializer(typeof(Features));
                _features = (Features)serializer.Deserialize(stream);
            }

        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            var target = new CabinetFeaturesPropertiesProvider();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void VerifyScreenBrightnessIsAddedAndSet()
        {
            var brightness = 55;
            _accessor.Setup(a => a[ApplicationConstants.ScreenBrightness]).Returns(brightness);
            var target = new CabinetFeaturesPropertiesProvider();

            Assert.IsTrue(
                target.GetCollection.Contains(new KeyValuePair<string, object>(ApplicationConstants.ScreenBrightness, brightness)));
            Assert.AreEqual(target.GetProperty(ApplicationConstants.ScreenBrightness), brightness);
        }

        [TestMethod]
        public void VerifyMaximumAllowedEdgeLightingBrightnessKeyIsAddedAndSet()
        {
            var maxBrightness = 99;
            _accessor.Setup(a => a[ApplicationConstants.MaximumAllowedEdgeLightingBrightnessKey]).Returns(maxBrightness);
            var target = new CabinetFeaturesPropertiesProvider();

            Assert.IsTrue(
                target.GetCollection.Contains(new KeyValuePair<string, object>(ApplicationConstants.MaximumAllowedEdgeLightingBrightnessKey, maxBrightness)));
            Assert.AreEqual(target.GetProperty(ApplicationConstants.MaximumAllowedEdgeLightingBrightnessKey), maxBrightness);
        }

        [TestMethod]
        public void VerifySoundChannelPropertiesAddedAndSet()
        {
            foreach (CabinetType cabinetType in Enum.GetValues(typeof(CabinetType)))
            {
                _cabinetDetection.Setup(c => c.Type).Returns(cabinetType);
                var target = new CabinetFeaturesPropertiesProvider();

                Assert.IsNotNull(target);
                var soundChannel =
                    _features.SoundChannel.FirstOrDefault(
                        x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex)) ??
                    new FeaturesSoundChannel();
                var enabledSpeakersMask = SpeakerMix.None;
 
                foreach (var channel in soundChannel.Channel)
                {
                    Enum.TryParse(channel, out SpeakerMix parsedChannel);
                    if (parsedChannel == SpeakerMix.All)
                    {
                        enabledSpeakersMask = SpeakerMix.All;
                        break;
                    }
 
                    enabledSpeakersMask |= parsedChannel;
                }
 
                Assert.IsTrue(
                    target.GetCollection.Contains(
                        new KeyValuePair<string, object>(
                            ApplicationConstants.EnabledSpeakersMask,
                            enabledSpeakersMask)));
                Assert.AreEqual(target.GetProperty(ApplicationConstants.EnabledSpeakersMask), enabledSpeakersMask);
            }
        }

        [TestMethod]
        public void VerifyCabinetScreenBrightnessPropertiesAddedAndSet()
        {
            foreach (CabinetType cabinetType in Enum.GetValues(typeof(CabinetType)))
            {
                var edgeLightFeature = _features.EdgeLightBrightnessControl.FirstOrDefault(
                    x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex));
                 _cabinetDetection.Setup(c => c.Type).Returns(cabinetType);
                 var target = new CabinetFeaturesPropertiesProvider();

                 Assert.IsNotNull(target);

                Assert.IsTrue(
                     target.GetCollection.Contains(
                         new KeyValuePair<string, object>(
                             ApplicationConstants.EdgeLightingBrightnessControlEnabled,
                             edgeLightFeature.Enabled)));

                 Assert.AreEqual(target.GetProperty(ApplicationConstants.EdgeLightingBrightnessControlEnabled), edgeLightFeature.Enabled);

                 Assert.IsTrue(
                     target.GetCollection.Contains(
                         new KeyValuePair<string, object>(
                             ApplicationConstants.EdgeLightingBrightnessControlDefault,
                             edgeLightFeature.Default)));

                 Assert.AreEqual(target.GetProperty(ApplicationConstants.EdgeLightingBrightnessControlDefault), edgeLightFeature.Default);

                 Assert.IsTrue(
                     target.GetCollection.Contains(
                         new KeyValuePair<string, object>(
                             ApplicationConstants.EdgeLightingBrightnessControlMin,
                             edgeLightFeature.Min)));

                 Assert.AreEqual(target.GetProperty(ApplicationConstants.EdgeLightingBrightnessControlMin), edgeLightFeature.Min);

                 Assert.IsTrue(
                     target.GetCollection.Contains(
                         new KeyValuePair<string, object>(
                             ApplicationConstants.EdgeLightingBrightnessControlMax,
                             edgeLightFeature.Max)));

                 Assert.AreEqual(target.GetProperty(ApplicationConstants.EdgeLightingBrightnessControlMax), edgeLightFeature.Max);
            }
        }

        [TestMethod]
        public void VerifyBottomStripPropertiesAddedAndSet()
        {
            foreach (CabinetType cabinetType in Enum.GetValues(typeof(CabinetType)))
            {
                var bottomStripFeature = _features.BottomStrip.FirstOrDefault(x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex))
                                         ?? new FeaturesBottomStrip
                                         {
                                             Enabled = false
                                         };

                _cabinetDetection.Setup(c => c.Type).Returns(cabinetType);
                var target = new CabinetFeaturesPropertiesProvider();

                Assert.IsNotNull(target);

                Assert.IsTrue(
                    target.GetCollection.Contains(
                        new KeyValuePair<string, object>(
                            ApplicationConstants.BottomStripEnabled,
                            bottomStripFeature.Enabled)));

                Assert.AreEqual(target.GetProperty(ApplicationConstants.BottomStripEnabled), bottomStripFeature.Enabled);

            }
        }

        [TestMethod]
        public void VerifyEdgeLightAsTowerLightPropertiesAddedAndSet()
        {
            foreach (CabinetType cabinetType in Enum.GetValues(typeof(CabinetType)))
            {
                var edgeLightAsTowerLight =
                    _features.EdgeLightAsTowerLight.FirstOrDefault(
                        x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex))
                    ?? new FeaturesEdgeLightAsTowerLight { Enabled = false };
                
                _cabinetDetection.Setup(c => c.Type).Returns(cabinetType);
                var target = new CabinetFeaturesPropertiesProvider();

                Assert.IsNotNull(target);

                Assert.IsTrue(
                    target.GetCollection.Contains(
                        new KeyValuePair<string, object>(
                            ApplicationConstants.EdgeLightAsTowerLightEnabled,
                            edgeLightAsTowerLight.Enabled)));

                Assert.AreEqual(target.GetProperty(ApplicationConstants.EdgeLightAsTowerLightEnabled), edgeLightAsTowerLight.Enabled);

            }
        }

        [TestMethod]
        public void VerifyBarkeeperPropertiesAddedAndSet()
        {
            foreach (CabinetType cabinetType in Enum.GetValues(typeof(CabinetType)))
            {
                var barkeeper =
                    _features.Barkeeper.FirstOrDefault(
                        x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex)) ??
                    new FeaturesBarkeeper { Enabled = false };
                Assert.IsNotNull(barkeeper);

                _cabinetDetection.Setup(c => c.Type).Returns(cabinetType);
                var target = new CabinetFeaturesPropertiesProvider();
                Assert.IsNotNull(target);

                Assert.IsTrue(
                    target.GetCollection.Contains(
                        new KeyValuePair<string, object>(
                            ApplicationConstants.BarkeeperEnabled,
                            barkeeper.Enabled)));

                Assert.AreEqual(target.GetProperty(ApplicationConstants.BarkeeperEnabled), barkeeper.Enabled);

            }
        }

        [TestMethod]
        public void VerifyUniversalInterfaceBoxPropertiesAddedAndSet()
        {
            var anyMatch = false;
            foreach (CabinetType cabinetType in Enum.GetValues(typeof(CabinetType)))
            {
                var universalInterfaceBox =
                    _features.UniversalInterfaceBox.FirstOrDefault(
                        x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex));

                if (universalInterfaceBox == null)
                {
                    continue;
                }

                anyMatch = true;

                _cabinetDetection.Setup(c => c.Type).Returns(cabinetType);
                var target = new CabinetFeaturesPropertiesProvider();
                Assert.IsNotNull(target);

                Assert.IsTrue(
                    target.GetCollection.Contains(
                        new KeyValuePair<string, object>(
                            ApplicationConstants.UniversalInterfaceBoxEnabled,
                            universalInterfaceBox.Enabled)));

                Assert.AreEqual(target.GetProperty(ApplicationConstants.UniversalInterfaceBoxEnabled), universalInterfaceBox.Enabled);
            }

            Assert.IsTrue(anyMatch);
        }


        [TestMethod]
        public void VerifyHarkeyReelControllerPropertiesAddedAndSet()
        {
            var anyMatch = false;
            foreach (CabinetType cabinetType in Enum.GetValues(typeof(CabinetType)))
            {
                var harkeyReelController =
                    _features.HarkeyReelController.FirstOrDefault(
                        x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex));

                if (harkeyReelController == null)
                {
                    continue;
                }

                anyMatch = true;

                _cabinetDetection.Setup(c => c.Type).Returns(cabinetType);
                var target = new CabinetFeaturesPropertiesProvider();
                Assert.IsNotNull(target);

                Assert.IsTrue(
                    target.GetCollection.Contains(
                        new KeyValuePair<string, object>(
                            ApplicationConstants.HarkeyReelControllerEnabled,
                            harkeyReelController.Enabled)));

                Assert.AreEqual(target.GetProperty(ApplicationConstants.HarkeyReelControllerEnabled), harkeyReelController.Enabled);
            }

            Assert.IsTrue(anyMatch);
        }
    }
}
