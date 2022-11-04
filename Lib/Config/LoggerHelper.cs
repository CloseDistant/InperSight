using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperSight.Lib.Config
{
    public class LoggerHelper
    {
        #region Fields
        private static readonly ILog logger;
        #endregion

        #region Constructors
        static LoggerHelper()
        {
            if (logger == null)
            {
                var repository = LogManager.CreateRepository("MixdoRepository");
                //log4net从log4net.config文件中读取配置信息
                var log4netConfig = new FileInfo("Lib/Config/LogConfig.xml");
                XmlConfigurator.Configure(repository, log4netConfig);
                logger = LogManager.GetLogger(repository.Name, "InfoLogger");
            }
        }
        #endregion

        #region Utilities
        /// <summary>
        /// 普通日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Info(string message, Exception exception = null)
        {
            if (exception == null)
                logger.Info(message);
            else
                logger.Info(message, exception);
        }
        /// <summary>
        /// 告警日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Warn(string message, Exception exception = null)
        {

            if (exception == null)
                logger.Warn(message);
            else
                logger.Warn(message, exception);
        }
        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Error(string message, Exception exception = null)
        {

            if (exception == null)
                logger.Error(message);
            else
                logger.Error(message, exception);
        }

        #endregion
    }
}
