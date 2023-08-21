namespace Aristocrat.Monaco.Hhr.Client.Tests.Messages
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector;
    using AutoMapper;
    using Mappings;
    using Client.Data;
    using Client.Messages;
    using Client.Messages.Converters;

    [TestClass]
    public class MessageFactoryTests
    {
        private MessageFactory _target;
        private Container _container;
        private readonly Assembly[] _assembly = new[] { Assembly.GetExecutingAssembly() };

        [TestInitialize]
        public void Initialize()
        {
            _container = new Container();
            _container.Register(typeof(IRequestConverter<>), _assembly);
            _container.Register(typeof(IResponseConverter<>), _assembly);

            var config = new MapperConfiguration(cfg => { cfg.AddProfile<ResponseProfile>(); });
            _container.Register(() => config.CreateMapper());

            _target = new MessageFactory(_container, _assembly);
        }
        
        [TestMethod]
        public void CreateInstanceWithNullContainerThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _target = new MessageFactory(null);
            });
        }

        [TestMethod]
        public void CreateInstanceWithContainerExpectNotNullInstance()
        {
            _target = new MessageFactory(_container);
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void SerializeRequestWithoutConverterExpectException()
        {
            Assert.ThrowsException<InvalidDataException>(() => _target.Serialize(new RequestWithoutConverter()));
        }

        [TestMethod]
        public void SerializeRequestWithConverterExpectConversion()
        {
            var request = new InvalidRequest()
            {
                MyRequest = "Test"
            };
            Assert.IsTrue(Encoding.UTF8.GetBytes(request.MyRequest).SequenceEqual(_target.Serialize(request)));
        }

        [TestMethod]
        public void DeserializeResponseWithNoConverterExpectEmptyResponse()
        {
            var bytes = new byte[100];
            Random rnd = new Random();
            rnd.NextBytes(bytes);

            // Set header command to CmdInactive1 where Converter is missing.
            var header = MessageUtility.GetMessage<MessageHeader>(bytes);
            header.Command = (uint)Command.CmdInactive1;
            MessageUtility.SetMessage(bytes, header);

            Assert.AreEqual(Command.CmdInvalidCommand, _target.Deserialize(bytes).Command);
        }

        [TestMethod]
        public void DeserializeResponseWithConverterExpectCorrectResponse()
        {
            var bytes = new byte[100];
            Random rnd = new Random();
            rnd.NextBytes(bytes);
            // set header command to CmdInvalidCommand where we have a Converter which sets MyProperty to byte[0]
            var header = MessageUtility.GetMessage<MessageHeader>(bytes);
            header.Command = (uint)Command.CmdInvalidCommand;

            MessageUtility.SetMessage(bytes, header);
            var withoutEncryptedHeader = MessageUtility.ExtractEncryptedHeader(bytes);
            var commandBytes = MessageUtility.ExtractMessageHeader(withoutEncryptedHeader);
            Assert.AreEqual(commandBytes[0], (_target.Deserialize(withoutEncryptedHeader) as InvalidResponse).MyProperty);
        }
    }  
}