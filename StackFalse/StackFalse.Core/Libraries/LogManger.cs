using CommonLib.Helpers;
using log4net;
using log4net.Config;
using log4net.Core;
using login.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace live_server.Model
{
    public enum Logger
    {
        [StringValue("Sync")]
        Sync,
        [StringValue("Async")]
        Async
    }

    public enum LevelType
    {
       DEBUG, INFO, WARN, ERROR, FATAL
    }

    public class LogManger
    {


        #region Singleton Pattern
        private static LogManger _instance;
        private static readonly object obj = new object();
        #endregion

        private readonly Dictionary<Logger, ILog> logs = new Dictionary<Logger, ILog>();

        private LogManger()
        {
            //讀取設定檔
            XmlConfigurator.Configure(new FileInfo(Path.GetDirectoryName(Tool.GetDirectoryPath() + "/log4netconfig.xml")));
        }

        public void log(Logger log, string msg, LevelType level = LevelType.INFO)
        {
            lock (logs)
            {
                if ( ! logs.ContainsKey(log))
                {
                    logs.Add(log, LogManager.GetLogger(log.GetStringValue()));
                }
                ILog _log = logs[log];
                switch (level)
                {
                    case LevelType.DEBUG:
                        _log.Debug(msg);
                        break;
                    case LevelType.WARN:
                        _log.Warn(msg);
                        break;
                    case LevelType.INFO:
                        _log.Info(msg);
                        break;
                    case LevelType.FATAL:
                        _log.Fatal(msg);
                        break;
                    case LevelType.ERROR:
                        _log.Error(msg);
                        break;
                }
            }
        }

        public static LogManger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (obj)
                    {
                        if (_instance == null)
                        {
                            _instance = new LogManger();
                        }
                    }
                }
                return _instance;
            }
        }
    }

    public class StringValueAttribute : Attribute
    {
 
        public string StringValue { get; protected set; }
        public StringValueAttribute(string value)
        {
            this.StringValue = value;
        }
    }

}
