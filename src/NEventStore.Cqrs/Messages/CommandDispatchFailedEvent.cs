using System;

namespace NEventStore.Cqrs.Messages
{
    public class CommandDispatchFailedEvent : IEvent
    {
        public Guid Id { get; set; }
        public string ErrorType { get; set; }
        public string ErrorMessage { get; set; }
        public string CommandType { get; set; }
        public string Command { get; set; }
    }
}
