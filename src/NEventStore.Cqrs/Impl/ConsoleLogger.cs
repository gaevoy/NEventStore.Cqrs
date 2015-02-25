using System;
using NEventStore.Cqrs.Messages;

namespace NEventStore.Cqrs.Impl
{
    public class ConsoleLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }

        public void Debug(string message)
        {
            Console.WriteLine(message);
        }

        public void Error(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        public void Error(Exception ex, ICommand cmd)
        {
            Console.WriteLine(ex.ToString());
        }

        public void Error(Exception ex, IEvent evt)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
