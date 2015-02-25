using System;
using NEventStore.Cqrs.Messages;

namespace NEventStore.Cqrs
{
    public interface ILogger
    {
        void Info(string message);
        void Debug(string message);
        void Error(Exception ex);
        void Error(Exception ex, ICommand cmd);
        void Error(Exception ex, IEvent evt);
    }
}
