using System;
using NEventStore.Cqrs.Messages;

namespace NEventStore.Cqrs.Impl
{
    public class NullLogger : ILogger
    {
        public void Info(string message)
        {
        }
        public void Debug(string message)
        {
        }
        public void Error(Exception ex)
        {
        }
        public void Error(Exception ex, ICommand cmd)
        {
        }
        public void Error(Exception ex, IEvent evt)
        {
        }
    }
}
