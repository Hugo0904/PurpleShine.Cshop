using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PurpleShine.Core.Delegates;
using PurpleShine.Core.Delegates.Args;
using PurpleShine.Core.Expansions;
using PurpleShine.Core.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Super8Customer.App.Helpers
{
    /// <summary>
    /// RabbitMQ Manager
    /// </summary>
    public class RabbitMQPool
    {
        #region Singleton Pattern
        public static RabbitMQPool Singleton { get; private set; }

        static RabbitMQPool()
        {
            Singleton = new RabbitMQPool();
        }

        private RabbitMQPool()
        {
            //
        }
        #endregion

        private readonly ConcurrentDictionary<string, LinkedList<EventListener>> _listeners = new ConcurrentDictionary<string, LinkedList<EventListener>>();
        private readonly ConcurrentDictionary<string, ConsumerManager> _listenChannels = new ConcurrentDictionary<string, ConsumerManager>();
        private readonly ReaderWriterLockSlim rwls = new ReaderWriterLockSlim();
        private IConnection _connection;
        private int _status = Status.Stopped;

        /// <summary>
        /// 發生例外時
        /// </summary>
        public event GenericEventHandler<Exception> OnException;

        /// <summary>
        /// 確認否與RabbitMQ資料庫連線中
        /// </summary>
        public bool IsConnected => _connection.IsNonNull() ? _connection.IsOpen : false;

        #region Function
        /// <summary>
        /// 連線至MQ
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public bool Connect(ConnectionFactory factory)
        {
            if (Interlocked.CompareExchange(ref _status, Status.Starting, Status.Stopped) == Status.Stopped)
            {
                try
                {
                    _connection = factory.CreateConnection();
                }
                catch (Exception)
                {
                    if (_connection != null)
                    {
                        _connection.Dispose();
                        _connection = null;
                    }
                    Interlocked.Exchange(ref _status, Status.Stopped);
                }
            }
            return _connection.IsNonNull() && _connection.IsOpen;
        }

        /// <summary>
        /// 結束連線
        /// </summary>
        public void Disconnect()
        {
            if (Interlocked.CompareExchange(ref _status, Status.Stopped, Status.Starting) == Status.Starting)
            {
                try
                {
                    _connection.Close();
                }
                catch (Exception)
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }
        }

        /// <summary>
        /// 確認目前某個頻道是否監聽中
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool IsChannelExist(string channel)
        {
            return _listenChannels.ContainsKey(channel);
        }

        /// <summary>
        /// 取得某頻道的監聽事件數量
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public int GetListenerCount(string channel)
        {
            if (_listeners.TryGetValue(channel, out LinkedList<EventListener> listeners))
            {
                return listeners.Count;
            }
            return 0;
        }

        /// <summary>
        /// 新增監聽者, 並由訂閱者接收到事件後, 
        /// 會通知所有訂閱該頻道的所有監聽者。
        /// </summary>
        /// <param name="channel">要監聽的頻道</param>
        /// <param name="RemoveCondition">該物件被移除的條件, 若為null 則為永久監聽事件</param>
        /// <param name="OnMessage">收到頻道的訊息該做什麼(請勿阻塞Thread)</param>
        /// <returns></returns>
        public void CreateChannelEventListener(string channel, Func<bool> RemoveCondition, Predicate<string> OnMessage)
        {
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
        /// 移除所有監聽事件者
        /// </summary>
        public void RemoveAllChannelEventListener()
        {
            try
            {
                rwls.EnterWriteLock();
                _listeners.Clear();
            }
            finally
            {
                rwls.ExitWriteLock();
            }
        }

        /// <summary>
        /// 建立通道
        /// </summary>
        /// <returns></returns>
        public IModel CreateModel()
        {
            if (IsConnected == false)
                throw new IOException("Connection error");

            return _connection.CreateModel();
        }

        private IModel last;

        /// <summary>
        /// 訂閱頻道
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool CreateChannelListener(string channel)
        {
            if (IsConnected == false)
                throw new IOException("Connection error");

            ConsumerManager consumerManager = new ConsumerManager()
            {
                Channel = channel
            };

            if (_listenChannels.TryAdd(channel, consumerManager))
            {
                IModel model = _connection.CreateModel();
                if (last != null)
                {
                    last = model;
                }
                var consumer = new EventingBasicConsumer(model);
                BindReceiveMessageEvent(model, channel, consumer);

                string consumerTag = model.BasicConsume(channel, false, consumer);
                consumerManager.Model = model;
                consumerManager.ConsumerTag = consumerTag;
                Console.WriteLine("監聽: " + channel);
                return true;
            }
            return false;
        }

        private void BindReceiveMessageEvent(IModel model, string channel, EventingBasicConsumer consumer)
        {
            consumer.Received += (ch, ea) =>
            {
                string message = Encoding.UTF8.GetString(ea.Body);
                Console.WriteLine("監聽: " + message);
                bool passAndDelete = true;
                if (_listeners.TryGetValue(channel, out LinkedList<EventListener> listeners))
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
                            passAndDelete &= listener.EventTrigger.Invoke(message);
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
                            OnException?.Invoke(Singleton, new ValueArgs<Exception>(ex));
                        }
                    });
                }

                // 若監聽事件有一個回傳失敗則不移除
                if (passAndDelete && model.IsClosed == false)
                    model.BasicAck(ea.DeliveryTag, false);
            };
        }

        /// <summary>
        /// 取消相關訂閱的頻道
        /// </summary>
        /// <param name="chs"></param>
        public void RemoveListenChannel(string channel)
        {
            if (_listenChannels.TryRemove(channel, out ConsumerManager manager) == false)
                return;

            using (manager.Model)
            {
                manager.Model.BasicCancel(manager.ConsumerTag);
                Console.WriteLine("取消並移除監聽: " + manager.Channel);
            }
        }

        /// <summary>
        /// 取消所有訂閱的頻道
        /// </summary>
        public void RemoveAllListenChannel()
        {
            _listenChannels.Keys
                .ToList()
                .ForEach(channel => RemoveListenChannel(channel));
        }
        #endregion Function

        /// <summary>
        /// 狀態
        /// </summary>
        struct Status
        {
            public const int Stopped = 0;
            public const int Starting = 1;
            public const int Waiting = 0;
            public const int Fail = 1;
            public const int Success = 2;
        }

        /// <summary>
        /// 頻道監聽管理者
        /// </summary>
        class ConsumerManager
        {
            public IModel Model { get; set; }

            public string Channel { get; set; }

            public string ConsumerTag { get; set; }
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
            public Predicate<string> EventTrigger { get; set; }
        }
    }
}