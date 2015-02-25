using System;
using System.IO;
using log4net;
using log4net.Config;
using NEventStore.Cqrs;
using NEventStore.Cqrs.Messages;

namespace PetProject.Books.Host.Impl
{
    public class Log4NetLogger : ILogger
    {
        private readonly ILog log;

        public Log4NetLogger(string name = null)
        {
            var configFile = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "/log4net.config");
            if (configFile.Exists)
                XmlConfigurator.ConfigureAndWatch(configFile);
            string app = name ?? AppDomain.CurrentDomain.FriendlyName
                .Replace(".exe", string.Empty)
                .Replace(".dll", string.Empty)
                .Replace(".vshost", string.Empty)
                .Replace(".Host", string.Empty);
            log = LogManager.GetLogger(app);
        }

        public void Info(string message)
        {
            log.Info(message);
        }

        public void Debug(string message)
        {
            log.Debug(message);
        }

        public void Error(Exception ex)
        {
            log.Error("", ex);
        }

        public void Error(Exception ex, ICommand cmd)
        {
            log.Error("", ex);
        }

        public void Error(Exception ex, IEvent evt)
        {
            log.Error("", ex);
        }
    }
}