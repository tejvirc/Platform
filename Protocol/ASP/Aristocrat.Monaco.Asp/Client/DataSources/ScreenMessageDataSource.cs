namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using Contracts;
    using Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Data source for Device Class 7 Type 1
    /// </summary>
    public class ScreenMessageDataSource : IDataSource
    {
        private readonly IMessageDisplay _messageDisplayService;
        private readonly Dictionary<string, DisplayableMessage> _handlers;

        public IReadOnlyList<string> Members => _handlers.Keys.ToList();

        public string Name => "ScreenMessages";

        public event EventHandler<Dictionary<string, object>> MemberValueChanged;

        public ScreenMessageDataSource(IMessageDisplay messageDisplayService)
        {
            _messageDisplayService =
                messageDisplayService ?? throw new ArgumentNullException(nameof(messageDisplayService));

            _handlers = GetMembersMap();
        }

        public object GetMemberValue(string member)
        {
            return _handlers[member]?.Message;
        }

        public void SetMemberValue(string member, object value)
        {
            if (!Members.Contains(member))
                throw new ArgumentOutOfRangeException(nameof(member), @"Invalid member name provided");

            if (string.IsNullOrWhiteSpace((string)value))
            {
                if (_handlers[member] != null) _messageDisplayService.RemoveMessage(_handlers[member]);
                _handlers[member] = null;
                return;
            }

            var displayableMessage = new DisplayableMessage(
                () => (string)value,
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Immediate,
                _handlers[member]?.Id ?? Guid.NewGuid());

            _messageDisplayService.DisplayMessage(displayableMessage);

            _handlers[member] = displayableMessage;

            MemberValueChanged?.Invoke(null, new Dictionary<string, object> { {member, displayableMessage.Message} });
        }

        private static Dictionary<string, DisplayableMessage> GetMembersMap()
        {
            return new Dictionary<string, DisplayableMessage>
            {
                { "Screen_Message_One", null },
                { "Screen_Message_Two", null }
            };
        }
    }
}