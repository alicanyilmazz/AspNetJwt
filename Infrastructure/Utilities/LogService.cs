using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Utilities
{
    public class LogService : ILogService
    {
        private dynamic logger { get; set; }
        public LogService(dynamic logger)
        {
            this.logger = logger;
        }
        public void Debug(object message)
        {
            logger.Debug(message);
        }

        public void Debug(string message, Exception exception)
        {
            logger.Debug(message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            logger.DebugFormat(format, args);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            logger.DebugFormat(provider, format, args);
        }

        public void Error(object message)
        {
            logger.Error(message);
        }

        public void Error(string message, Exception exception)
        {
            logger.Error(message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            logger.ErrorFormat(format, args);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            logger.ErrorFormat(provider, format, args);
        }

        public void Fatal(object message)
        {
            logger.Fatal(message);
        }

        public void Fatal(string message, Exception exception)
        {
            logger.Fatal(message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            logger.FatalFormat(format, args);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            logger.FatalFormat(provider, format, args);
        }

        public void Info(object message)
        {
            logger.Info(message);
        }

        public void Info(string message, Exception exception)
        {
            logger.Info(message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            logger.InfoFormat(format, args);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            logger.InfoFormat(provider, format, args);
        }

        public bool IsDebugEnabled()
        {
            return logger.IsDebugEnabled;
        }

        public void Warn(object message)
        {
            logger.Warn(message);
        }

        public void Warn(string message, Exception exception)
        {
            logger.Warn(message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            logger.WarnFormat(format, args);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            logger.WarnFormat(provider, format, args);
        }

        #region Disposable Support
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
