using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using PurpleShine.Core.Expansions;
using PurpleShine.Core.Helpers;
using StackExchange.Redis;

namespace PurpleShine.Database.Redis
{
    public class AddressItem
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public static implicit operator AddressItem(string address)
        {
            string[] res = address.Split(':');

            if (res.Length != 2) return null;

            return new AddressItem()
            {
                Host = res[0],
                Port = res[1].ToInt32()
            };
        }
    }

    /// <summary>
    /// Redis Manager
    /// </summary>
    public class RedisUtil : IDisposable
    {
        #region Singleton Pattern
        private static RedisUtil _instance;

        static RedisUtil()
        {
            _instance = new RedisUtil();
        }

        private RedisUtil()
        {
            //
        }

        public static RedisUtil Multiplexer
        {
            get { return _instance; }
        }
        #endregion

        /// <summary>
        /// 發生例外時
        /// </summary>
        public static event CustomEventHandler<Exception> OnException;

        private readonly ConcurrentDictionary<string, LinkedList<EventListener>> _listeners = new ConcurrentDictionary<string, LinkedList<EventListener>>();
        private readonly ReaderWriterLockSlim rwls = new ReaderWriterLockSlim();
        private readonly HashSet<string> _listenChannels = new HashSet<string>();
        private ConnectionMultiplexer _manager;
        private volatile IDatabase _database;
        private volatile IServer _server;
        private volatile int _heartBeatState;

        public bool IsDisposed { get; private set; }

        ~RedisUtil()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 監聽到訊息時
        /// [0] = channel
        /// [1] = message
        /// </summary>
        public event CustomEventHandler<string[]> OnSubMessage;

        /// <summary>
        /// 當每次正常心跳時
        /// </summary>
        public event CustomEventHandler<long> OnHeartBeat;

        /// <summary>
        /// 當心跳發生錯誤時
        /// </summary>
        public event CustomEventHandler<Exception> OnHeartBeatFail;

        /// <summary>
        /// 是否啟動心跳中
        /// </summary>
        public static bool IsHeartBeat => Multiplexer._heartBeatState == Status.Starting;

        /// <summary>
        /// 最後一次Ping的時間
        /// </summary>
        private static DateTime LastPing { get; set; }

        /// <summary>
        /// 確認Pool是否與Redis資料庫連線中
        /// </summary>
        public static bool IsConnected => Manager != null ? Manager.IsConnected : false;

        /// <summary>
        /// 主要管理員
        /// </summary>
        public static ConnectionMultiplexer Manager => Multiplexer._manager;

        /// <summary>
        /// Redis clinet for this database
        /// </summary>
        public static IDatabase Database
        {
            get
            {
                if (Manager != null)
                {
                    if (!Manager.IsConnected)
                        throw new Exception("Manager connect error.");

                    return Multiplexer._database;
                }
                throw new Exception("Manager not yet to open.");
            }
        }

        /// <summary>
        /// Redis server for this database
        /// </summary>
        public static IServer Server
        {
            get
            {
                if (Manager != null)
                {
                    if (!Manager.IsConnected)
                        throw new Exception("Manager connect error.");

                    return Multiplexer._server;
                }
                throw new Exception("Manager not yet to open.");
            }
        }

        /// <summary>
        /// 異步處理Channel訊息
        /// (預設 False)
        /// </summary>
        public bool PreserveAsyncOrder
        {
            get => Manager.PreserveAsyncOrder;
            set => Manager.PreserveAsyncOrder = value;
        }

        #region Function
        /// <summary>
        /// 建立交易
        /// </summary>
        /// <param name="OnDoing"></param>
        /// <param name="OnExcute"></param>
        public static void ExcuteTransaction(Action<ITransaction> OnDoing, Action<Exception, bool> OnExcute)
        {
            Excute((err, client) =>
            {
                if (err != null)
                {
                    OnExcute.Invoke(err, false);
                    return;
                }
                var trans = client.CreateTransaction();
                OnDoing(trans);
                OnExcute.Invoke(null, trans.Execute(CommandFlags.HighPriority));
            });
        }

        /// <summary>
        /// 異步執行redis
        /// </summary>
        /// <param name="doing">{exception, database}</param>
        public static Task ExcuteAsync(Func<Exception, IDatabase, Task> doing)
        {
            return Task.Run(async () =>
            {
                try { await doing.Invoke(null, Database); }
                catch (Exception e)
                {
                    OnException?.Invoke(Multiplexer, new ValueArgs<Exception>(e));
                    await doing.Invoke(e, null);
                }
            });
        }

        /// <summary>
        /// 異步執行Redis
        /// </summary>
        /// <param name="doing">{exception, database}</param>
        public static Task ExcuteAsync(Action<Exception, IDatabase> doing)
        {
            return Task.Run(() => Excute(doing));
        }


        /// <summary>
        /// 同步執行redis
        /// </summary>
        /// <param name="doing">{exception, database}</param>
        public static void Excute(Action<Exception, IDatabase> doing)
        {
            try { doing.Invoke(null, Database); }
            catch (Exception e)
            {
                OnException?.Invoke(Multiplexer, new ValueArgs<Exception>(e));
                doing.Invoke(e, null);
            }
        }

        /// <summary>
        /// 異步執行Redis
        /// </summary>
        /// <param name="doing">{exception, database}</param>
        public static Task<T> ExcuteAsync<T>(Func<Exception, IDatabase, T> doing)
        {
            return Task.Run(() => Excute(doing));
        }

        /// <summary>
        /// 同步執行redis
        /// </summary>
        /// <param name="doing">{exception, database}</param>
        public static T Excute<T>(Func<Exception, IDatabase, T> doing)
        {
            try { return doing.Invoke(null, Database); }
            catch (Exception e)
            {
                OnException?.Invoke(Multiplexer, new ValueArgs<Exception>(e));
                return doing.Invoke(e, null);
            }
        }

        /// <summary>
        /// 新增監聽者, 並由訂閱者接收到事件後, 
        /// 會通知所有訂閱該頻道的所有監聽者。
        /// </summary>
        /// <param name="channel">要監聽的頻道</param>
        /// <param name="RemoveCondition">該物件被移除的條件, 若為null 則為永久監聽事件</param>
        /// <param name="OnMessage">收到頻道的訊息該做什麼(請勿阻塞Thread)</param>
        /// <returns></returns>
        public void CreateChannelEventListener(string channel, Func<bool> RemoveCondition, Action<string> OnMessage)
        {
            if (channel == null)
                throw new NullReferenceException(nameof(channel));

            try
            {
                rwls.EnterWriteLock();
                var listeners = _listeners.GetOrAdd(channel, new LinkedList<EventListener>());
                listeners.AddLast(new EventListener
                {
                    RemoveCondition = RemoveCondition,
                    EventTrigger = OnMessage
                });
            }
            finally
            {
                rwls.ExitWriteLock();
            }
        }

        /// <summary>
        /// 訂閱頻道
        /// </summary>
        /// <param name="channel">監聽的頻道</param>
        /// <returns></returns>
        public RedisUtil CreateChannelListener(string channel)
        {
            if (Manager != null && Manager.IsConnected && _listenChannels.Add(channel))
            {
                Manager.GetSubscriber().Subscribe(channel, (ch, message) =>
                {
                    OnSubMessage?.Invoke(this, new ValueArgs<string[]>(new string[] { ch, message }));
                    if (string.IsNullOrEmpty(message)) return;
                    if (_listeners.TryGetValue(ch, out LinkedList<EventListener> listeners))
                    {
                        List<EventListener> copyListeners;
                        try
                        {
                            rwls.EnterReadLock();
                            if (listeners.Count == 0) return;
                            copyListeners = listeners.ToList();
                        }
                        finally
                        {
                            rwls.ExitReadLock();
                        }

                        // 使用平行迭代, 加速迴圈
                        Parallel.ForEach(copyListeners, (listener, loopState) =>
                        {
                            try
                            {
                                listener.EventTrigger.Invoke(message);
                                if (listener.RemoveCondition != null && listener.RemoveCondition())
                                {
                                    try
                                    {
                                        rwls.EnterWriteLock();
                                        listeners.Remove(listener);
                                    }
                                    finally
                                    {
                                        rwls.ExitWriteLock();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                OnException.Invoke(Multiplexer, new ValueArgs<Exception>(ex));
                            }
                        });
                    }
                }, CommandFlags.HighPriority);
            }
            return this;
        }

        /// <summary>
        /// 關閉心跳
        /// </summary>
        /// <returns></returns>
        public RedisUtil CloseHeartbeat()
        {
            Interlocked.Exchange(ref _heartBeatState, Status.Stopped);
            return this;
        }

        /// <summary>
        /// 建立心跳
        /// </summary>
        /// <param name="interval">頻率 100~10000 ms</param>
        /// <returns></returns>
        public bool CreateHeartbeat(int interval)
        {
            if (Interlocked.CompareExchange(ref _heartBeatState, Status.Starting, Status.Stopped) != Status.Stopped)
                return false;

            if (interval < 100) interval = 100;
            if (interval > 10000) interval = 10000;

            Thread heart = new Thread(() =>
            {
                while (_heartBeatState == Status.Starting)
                {
                    try
                    {
                        OnHeartBeat?.Invoke(this, new ValueArgs<long>((long)Database.Ping(CommandFlags.HighPriority).TotalMilliseconds));
                        LastPing = DateTime.Now;
                    }
                    catch (Exception e)
                    {
                        if (!IsDisposed) OnHeartBeatFail?.Invoke(this, new ValueArgs<Exception>(e));
                    }
                    SpinWait.SpinUntil(() => _heartBeatState == Status.Stopped, interval);
                }
                heart = null;
            })
            {
                IsBackground = true
            };
            heart.Start();

            return heart.IsAlive;
        }

        /// <summary>
        /// 建立通道管理員
        /// </summary>
        /// <param name="config">設定檔</param>
        /// <returns></returns>
        public bool Connect(ConfigurationOptions config)
        {
            if (config == null) return false;

            try
            {
                _manager = ConnectionMultiplexer.Connect(config);

                if (_manager != null && _manager.IsConnected)
                {
                    _database = _manager.GetDatabase();
                    GetServer();
                    PreserveAsyncOrder = false;
                }
                return _database != null && _database.Ping().TotalMilliseconds > 0;
            }
            catch (Exception ex)
            {
                _manager = null;
                OnException?.Invoke(Multiplexer, new ValueArgs<Exception>(ex));
            }
            return false;
        }

        private void GetServer()
        {
            EndPoint[] endpoints = _manager.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _manager.GetServer(endpoint);
                if (server.IsSlave || !server.IsConnected) continue;
                if (_server != null) throw new InvalidOperationException("Requires exactly one master endpoint (found " + server.EndPoint + " and " + _server.EndPoint + ")");
                _server = server;
            }
            if (_server == null) throw new InvalidOperationException("Requires exactly one master endpoint (found none)");
        }

        public bool DisConnect()
        {
            if (_manager != null)
            {
                _manager.Dispose();
                _manager = null;
                _database = null;
            }
            return false;
        }

        /// <summary>
        /// 建立通道管理員
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Connect(AddressItem address)
        {
            if (address == null) return false;
            ConfigurationOptions config = new ConfigurationOptions
            {
                ConnectRetry = 10,
                ConnectTimeout = 6000,
                SyncTimeout = 8000,
                ResponseTimeout = 8000,
                CommandMap = CommandMap.Create(new HashSet<string>
                { // EXCLUDE a few commands // INFO // PING
                    "CONFIG", "CLUSTER",
                    "ECHO", "CLIENT"
                }, available: false),
                //KeepAlive = 180,
                DefaultVersion = new Version(3, 2, 100),
                Password = "",
                AllowAdmin = true
            };
            config.EndPoints.Add(address.Host, address.Port);
            return Connect(config);
        }
        #endregion Function

        /// <summary>
        /// 取消相關訂閱的頻道
        /// </summary>
        /// <param name="chs"></param>
        private void RemoveListenChannels(string[] chs)
        {
            ISubscriber sub = Manager.GetSubscriber();
            foreach (var ch in chs)
            {
                if (_listenChannels.Remove(ch))
                {
                    sub.UnsubscribeAsync(ch).ConfigureAwait(false);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Interlocked.Exchange(ref _heartBeatState, Status.Stopped);
                if (disposing)
                {
                    _manager?.Dispose();
                }
                _manager = null;
                _database = null;
                _instance = null;
            }
        }

        struct Status
        {
            public const int Stopped = 0;
            public const int Starting = 1;
            public const int Waiting = 0;
            public const int Fail = 1;
            public const int Success = 2;
        }

        /// <summary>
        /// Channel訊息監聽者
        /// </summary>
        struct EventListener
        {
            /// <summary>
            /// 移除監聽條件
            /// </summary>
            public Func<bool> RemoveCondition { get; set; }

            /// <summary>
            /// 當有訊息的動作
            /// </summary>
            public Action<string> EventTrigger { get; set; }
        }
    }
}
