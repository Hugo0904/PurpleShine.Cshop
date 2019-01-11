using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using log4net;
using log4net.Config;


namespace PurpleShine.Trace.Logging
{
    public enum LevelType
    {
        DEBUG, INFO, WARN, ERROR, FATAL
    }

    public partial class FLog : IDisposable
    {
        #region Singleton Pattern
        private static FLog _instance;
        private static readonly object obj = new object();
        public static FLog Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (obj)
                    {
                        if (_instance == null)
                        {
                            _instance = new FLog();
                        }
                    }
                }
                return _instance;
            }
        }
        #endregion

        private const int keepFileDay = 3;  // 保留log記錄天數

        private readonly string _filePath = Environment.CurrentDirectory + @"/Logs";  // Log
        private readonly string _xmlPath = Environment.CurrentDirectory + @"/log4netconfig.xml";  // XML
        private readonly ConcurrentDictionary<string, ILog> _logs = new ConcurrentDictionary<string, ILog>();
        private readonly System.Timers.Timer _timer;
        private int _clearDelay = 3600000;

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// 檢查清除間隔
        /// </summary>
        public int ClearDelay
        {
            get => _clearDelay;
            set
            {
                _clearDelay = value;
                _timer.Interval = _clearDelay;
            }
        }

        /// <summary>
        /// 輸出至Console的級別, null = cancel
        /// </summary>
        public LevelType ConsoleOutput { get; set; }

        ~FLog()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                if (disposing)
                {
                    _timer.Dispose();
                }
            }
        }

        private FLog()
        {
            GlobalContext.Properties["LogFilePath"] = _filePath;
            XmlConfigurator.Configure(new FileInfo(_xmlPath));

            // load logger 
            _logs.TryAdd("Default", LogManager.GetLogger("Default"));

            ConsoleOutput = LevelType.DEBUG;

            Timer_Elapsed(null, null);
            _timer = new System.Timers.Timer(ClearDelay);
            _timer.Elapsed += Timer_Elapsed; ;
            _timer.Start();
        }

        #region Event
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var filt = from file in Directory.GetFiles(_filePath, "*.*", SearchOption.AllDirectories)
                           let fileInfo = new FileInfo(file)
                           where fileInfo.LastAccessTime < DateTime.Now.AddDays(-keepFileDay) || fileInfo.CreationTime < DateTime.Now.AddDays(-keepFileDay)
                           select fileInfo;

                if (filt.Any())
                {
                    int deleteCount = 0;
                    filt.ToList().ForEach(file =>
                    {
                        try
                        {
                            file.Delete();
                            deleteCount++;
                        }
                        catch
                        {
                            // 正在使用中的不移除
                        }
                    });
                    Console.WriteLine($"刪除 {deleteCount} 個記錄檔");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
        #endregion Event

        public static string Debug(string logger, string msg, params object[] args)
        {
            return Instance.Log(logger, LevelType.DEBUG, msg, args);
        }

        public static string Info(string logger, string msg, params object[] args)
        {
            return Instance.Log(logger, LevelType.INFO, msg, args);
        }

        public static string Warn(string logger, string msg, params object[] args)
        {
            return Instance.Log(logger, LevelType.WARN, msg, args);
        }

        public static string Error(string logger, string msg, params object[] args)
        {
            return Instance.Log(logger, LevelType.ERROR, msg, args);
        }

        public static string Fatal(string logger, string msg, params object[] args)
        {
            return Instance.Log(logger, LevelType.FATAL, msg, args);
        }

        public string Log(string logger, LevelType level, string message, params object[] args)
        {
            try
            {
                if (!_logs.TryGetValue(logger, out ILog _log))
                {
                    if (LogManager.Exists(logger) == null)
                    {
                        _logs.TryGetValue("Default", out _log);
                    }
                    else
                    {
                        _log = LogManager.GetLogger(logger);
                        _logs.TryAdd(logger, _log);
                    }
                }

                if (args.Length > 0)
                    message = string.Format(message, args);

                message = $"[{Thread.CurrentThread.ManagedThreadId.ToString("D2")}] {message}";

                switch (level)
                {
                    case LevelType.DEBUG:
                        _log.Debug(message);
                        break;
                    case LevelType.WARN:
                        _log.Warn(message);
                        break;
                    case LevelType.INFO:
                        _log.Info(message);
                        break;
                    case LevelType.FATAL:
                        _log.Fatal(message);
                        break;
                    case LevelType.ERROR:
                        _log.Error(message);
                        break;
                }

                if (level >= ConsoleOutput) Console.WriteLine(message);
            }
            catch (Exception)
            {
                throw;
            }

            return message;
        }
    }
}
