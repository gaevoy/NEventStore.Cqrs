using System;
using System.IO;
using log4net;
using log4net.Config;

namespace PetProject.Books.Host.Impl.ProjectorIntegration
{
    public class Log4NetLog : global::EventStream.Projector.Logger.ILog
    {
        private readonly ILog log;

        public Log4NetLog(string name = null)
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

        public void Debug(object message)
        {
            log.Debug(message);
        }

        public void Debug(object message, Exception exception)
        {
            log.Debug(message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            log.DebugFormat(format, args);
        }

        public void Info(object message)
        {
            log.Info(message);
        }

        public void Info(object message, Exception exception)
        {
            log.Info(message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            log.InfoFormat(format, args);
        }

        public void Warn(object message)
        {
            log.Warn(message);
        }

        public void Warn(object message, Exception exception)
        {
            log.Warn(message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            log.WarnFormat(format, args);
        }

        public void Error(object message)
        {
            log.Error(message);
        }

        public void Error(object message, Exception exception)
        {
            log.Error(message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            log.ErrorFormat(format, args);
        }

        public void Fatal(object message)
        {
            log.Fatal(message);
        }

        public void Fatal(object message, Exception exception)
        {
            log.Fatal(message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            log.FatalFormat(format, args);
        }
    }
}