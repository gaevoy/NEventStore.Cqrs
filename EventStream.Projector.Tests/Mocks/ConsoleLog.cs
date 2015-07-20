using System;
using EventStream.Projector.Logger;

namespace EventStream.Projector.Tests.Mocks
{
    public class ConsoleLog : ILog
    {
        public void Debug(object message)
        {
            Console.WriteLine(message);
        }

        public void Debug(object message, Exception exception)
        {
            Console.WriteLine(message);
        }

        public void DebugFormat(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Info(object message)
        {
            Console.WriteLine(message);
        }

        public void Info(object message, Exception exception)
        {
            Console.WriteLine(message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Warn(object message)
        {
            Console.WriteLine(message);
        }

        public void Warn(object message, Exception exception)
        {
            Console.WriteLine(message);
        }

        public void WarnFormat(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Error(object message)
        {
            Console.WriteLine(message);
        }

        public void Error(object message, Exception exception)
        {
            Console.WriteLine(message);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Fatal(object message)
        {
            Console.WriteLine(message);
        }

        public void Fatal(object message, Exception exception)
        {
            Console.WriteLine(message);
        }

        public void FatalFormat(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}
