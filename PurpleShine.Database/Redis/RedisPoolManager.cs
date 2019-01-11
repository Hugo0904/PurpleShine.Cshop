using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Redis;
using PurpleShine.Core.Helpers;
using PurpleShine.Database.Redis.Expansion;
using PurpleShine.Trace.Logging;

namespace PurpleShine.Database.Redis
{
    public class RedisPoolManager : RedisNativeClient
    {
        #region Singleton Pattern
        private static RedisPoolManager _instance;
        private static readonly object _lock = new object();
        public static RedisPoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new RedisPoolManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private RedisPoolManager()
        {
            // 設置timeout
            RedisConfig.DefaultConnectTimeout = 3000;
            RedisConfig.DefaultRetryTimeout = 3000;
            RedisConfig.DefaultSendTimeout = 5000;
            RedisConfig.DefaultIdleTimeOutSecs = 5;
            ChannelListeners = new ConcurrentDictionary<ChannelListener, long>();
            EventListeners = new ConcurrentDictionary<string, HashSet<EventListener>>();
        }
        #endregion

        private readonly ConcurrentDictionary<string, HashSet<EventListener>> EventListeners;
        private readonly ConcurrentDictionary<ChannelListener, long> ChannelListeners;
        public event CustomEventHandler<long> OnHeartBeat;
        public event CustomEventHandler<Exception> OnHeartBeatFail;
        private RedisClient enginRedisClient;
        private volatile bool close;
        private int heartbeat_delay;
        private int heartbeat_status;
        private long cid;

        /// <summary>
        /// 設置 > 0 啟動心跳功能 value = 間隔
        /// 設置 0 = 取消
        /// </summary>
        public int Heartbeat
        {
            get => heartbeat_delay;
            set
            {
                if (heartbeat_status == Status.Stopped && value > 0)
                {
                    heartbeat_delay = value;
                    CreateHeartbeat();
                }
                else
                {
                    heartbeat_delay = 0;
                }
            }
        }

        /// <summary>
        /// 建立PUB/SUB 等待動做
        /// </summary>
        /// <param name="client"></param>
        /// <param name="channel"></param>
        /// <param name="failRetryTimes"></param>
        /// <param name="OnFilter"></param>
        /// <param name="OnResponse"></param>
        /// <param name="OnSpecialCase"></param>
        /// <param name="message"></param>
        /// <param name="OnLog"></param>
        /// <param name="OnSuccess"></param>
        /// <param name="OnFail"></param>
        //public static void CreateWaitPubAction(RedisClient client, string channel, int failRetryTimes, Predicate<JToken> OnFilter,
        //    Action OnResponse, Func<bool> OnSpecialCase, string message, Action<string> OnLog, Action<long> OnSuccess, Action OnFail)
        //{
        //    int status = Status.Waiting;
        //    long notify_count = 0;
        //    using (ManualResetEvent manual = new ManualResetEvent(false))
        //    {
        //        Instance.AddChannelEventListener(
        //          channel: channel, // 頻道
        //          removeCondition: () => Interlocked.CompareExchange(ref status, Status.Waiting, Status.Waiting) != Status.Waiting, // 取消動作
        //          eventTriger: (publish_message) =>
        //          {
        //              if (Interlocked.CompareExchange(ref status, Status.Fail, Status.Fail) == Status.Fail)
        //                  return;

        //              try
        //              {
        //                  SubPub.HandleSubMessage(publish_message, (error, content) =>
        //                  {
        //                      if (error != null) throw error;
        //                      if (OnFilter(content)) return;
        //                      if (Interlocked.CompareExchange(ref status, Status.Success, Status.Waiting) == Status.Waiting)
        //                      {
        //                          OnResponse();
        //                      }
        //                      manual.Set();
        //                  });
        //              }
        //              catch (Exception ex)
        //              {
        //                  OnLog?.Invoke(string.Format("發生Exception, Message: {0}, Reason: {1}.", ex.Message, ex.StackTrace));
        //              }
        //          });

        //        try
        //        {
        //            for (int trys = 0; Interlocked.CompareExchange(ref status, Status.Waiting, Status.Waiting) == Status.Waiting; trys++)
        //            {
        //                lock (client)
        //                {
        //                    notify_count = client.PublishMsg(channel, message);
        //                }

        //                if (!manual.WaitOne(3000))
        //                {
        //                    if ((OnSpecialCase == null || OnSpecialCase()) && ++trys < failRetryTimes)
        //                        OnLog?.Invoke(string.Format("結果第{0}次無回應.", trys));
        //                    else
        //                    {
        //                        OnLog?.Invoke(string.Format("結果第{0}次無回應, 放棄此次等待", failRetryTimes));
        //                        Interlocked.Exchange(ref status, Status.Fail);  // 放棄移除
        //                    }
        //                }
        //            }
        //        }
        //        finally
        //        {
        //            // 若要離開此區塊判斷狀態是否不為waiting, 若是 將修改為錯誤
        //            // 避免程式碼在這區快發生exception時, 導致跳脫區塊而status還是waiting,
        //            // 會使事件仍然繼續監聽
        //            Interlocked.CompareExchange(ref status, Status.Fail, Status.Waiting);
        //        }
        //    }

        //    if (status == Status.Fail)
        //        OnFail?.Invoke();
        //    else
        //        OnSuccess?.Invoke(notify_count);
        //}

        /// <summary>
        /// 新增一個監聽者, 並由訂閱者接收到事件後, 
        /// 會通知所有訂閱該頻道的所有監聽者。
        /// </summary>
        /// <param name="channel">要監聽的頻道</param>
        /// <param name="removeCondition">該物件被移除的條件, 若為null 則為永久監聽事件</param>
        /// <param name="events">收到頻道的訊息該做什麼</param>
        /// <returns></returns>
        public bool AddChannelEventListener(string channel, Func<bool> removeCondition, Action<string> eventTriger)
        {
            try
            {
                EventListeners.GetOrAdd(channel, new HashSet<EventListener>()).Add(new EventListener()
                {
                    RemoveCondition = removeCondition,
                    EventTriger = eventTriger
                });
                return true;
            }
            catch (Exception ex)
            {
                FLog.Fatal("Default", $"AddChannelEventListener發生Exception, Message: {ex.Message}, Reason: {ex.StackTrace}");
            }
            return false;
        }

        /// <summary>
        /// 檢查目前該頻道有無任何訂閱者正在監聽
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool ChannelListenExist(string ch, out RedisClient client)
        {
            var item = ChannelListeners.Where(i => i.Key.Channels.Contains(ch));

            if (item.Any())
            {
                client = item.First().Key.Client;
                return true;
            }

            client = null;
            return false;
        }

        /// <summary>
        /// (若msgAction是自定的則無效)
        /// 取消該client id 所訂閱的所有頻道
        /// </summary>
        /// <param name="id">client id</param>
        public void RemoveListenChannels(RedisClient client)
        {
            var item = ChannelListeners.Where(i => i.Key.Client == client);

            if (item.Any())
            {
                item.First().Key.Cancel = true;
            }
        }

        /// <summary>
        /// (若msgAction是自定的則無效)
        /// 取消所有相關訂閱的頻道
        /// </summary>
        /// <param name="chs"></param>
        public void RemoveListenChannels(params string[] chs)
        {
            foreach (var item in ChannelListeners)
            {
                Remove_Listen_Channels(item.Key, chs);
            }
        }

        private void Remove_Listen_Channels(ChannelListener items, string[] chs)
        {
            var same = items.Channels.Intersect(chs);
            if (same.Any())
            {
                lock (items)
                {
                    if (items.FeatureRemoveChannels == null)
                        items.FeatureRemoveChannels = new HashSet<string>(same);
                    else
                        items.FeatureRemoveChannels.UnionWith(same);
                    //sItem.channels.RemoveAll(i => same.Contains(i));
                }
            }
        }

        /// <summary>
        /// 確認所有的訂閱者是否還活著
        /// 若不存在則釋放掉它。
        /// 
        /// 目前還在測試中, 正常來說若出問題會自動退訂
        /// 但卻發生了, 訂閱出問題但client還掛在線上
        /// </summary>
        public void CheckChannelListenersAlive()
        {
            ChannelListeners.ToList().ForEach(i =>
            {
                FLog.Debug("Default", "id: {0}, IsAlive: {1}, count: {2}, progress: {3}", i.Key.Client.Id, i.Key.Client.IsAlive(), i.Key.Subscription.SubscriptionCount, i.Key.Progress);
                if (!i.Key.Client.IsAlive() || i.Key.Subscription.SubscriptionCount == 0)
                {
                    FLog.Debug("Default", "Dispose clinet id {0}", i.Key.Client.Id);
                    i.Key.Client.Dispose();
                }
            });
        }

        /// <summary>
        /// 初始化多個執行緒, 每個訂閱的頻道為單一執行緒
        /// </summary>
        /// <param name="chs">要監聽的頻道群</param>
        /// <param name="msgAction">當接收到訊息該做什麼, 若未指定動作, 則是預設將訊息送至Items 做訊息loop</param>
        /// <param name="onAction">當訂閱成功該做什麼</param>
        /// <param name="endAction">當取消訂閱該做什麼</param>
        /// <returns>返回監聽中的client id, 若建立失敗返回 -1</returns>
        public bool CreateChannelListener(bool sSignThread, string host, string[] chs, out Dictionary<string, RedisClient> ids, Action<string> onAction = null, Action<string> endAction = null) //  Action<string, string> msgAction,
        {
            ids = new Dictionary<string, RedisClient>();

            if (!chs.Any())
                return false;

            foreach (var ch in chs)
            {
                if (CreateChannelListener(host, new[] { ch }, out RedisClient client, onAction, endAction))
                {
                    ids.Add(ch, client);
                }
            }
            return true;
        }

        /// <summary>
        /// 初始化單一執行緒, 所有訂閱的頻道共享同一執行緒
        /// </summary>
        /// <param name="channels">要監聽的頻道群</param>
        /// <param name="msgAction">當接收到訊息該做什麼, 若未指定動作, 則是預設將訊息送至Items 做訊息loop</param>
        /// <param name="OnSubscribe">當訂閱成功該做什麼</param>
        /// <param name="OnUnSubscribe">當取消訂閱該做什麼</param>
        /// <returns>返回監聽中的client id, 若建立失敗返回 -1</returns>
        public bool CreateChannelListener(string host, string[] channels, out RedisClient client, Action<string> OnSubscribe = null, Action<string> OnUnSubscribe = null) //  Action<string, string> msgAction,
        {
            client = null;

            string[] _chs = channels.Where(i => !ChannelListeners.Any(s => s.Key.Channels.Contains(i))).ToArray();

            if (_chs.Any())
            {
                RedisClient _client = new RedisClient(host)
                {
                    Id = Interlocked.Increment(ref cid)
                };
                //RedisClient client = CreateReadClient();

                if (_client.Ping() && _client.IsAlive())
                {
                    client = _client;
                    ChannelListener item = new ChannelListener(_client, _chs);
                    ChannelListeners.TryAdd(item, _client.Id);

                    // 另外建立執行序來做監聽, 因為監聽會block執行序
                    Thread thread = new Thread(() =>
                    {
                        Console.WriteLine("[{0}] Listener start...", Thread.CurrentThread.ManagedThreadId);
                        try
                        {
                            item.Subscription.SubscribeChannels(_chs, OnSubscribe, (channel, msg) =>
                            {
                                if (string.IsNullOrEmpty(msg))
                                    return;

                                item.Progress = 1;
                                FLog.Debug("Pubsub", "[{0} - {1}] {2} = {3}", Thread.CurrentThread.ManagedThreadId, item.Id, channel, msg);

                                if (item.Cancel || !item.Channels.Any())
                                {
                                    item.Progress = 2;
                                    item.Subscription.UnSubscribeFromAllChannels();
                                    item.Progress = 3;
                                }
                                else
                                {
                                    item.Progress = 4;
                                    if (item.FeatureRemoveChannels != null)
                                    {
                                        item.Progress = 5;
                                        lock (item)
                                        {
                                            item.Progress = 6;
                                            item.Subscription.UnSubscribeFromChannels(item.FeatureRemoveChannels.ToArray());
                                            item.Progress = 7;
                                            item.FeatureRemoveChannels = null;
                                        }
                                    }
                                    item.Progress = 8;
                                    if (item.Channels.Contains(channel) && EventListeners.TryGetValue(channel, out HashSet<EventListener> _items) && _items.Any())
                                    {
                                        item.Progress = 9;
                                        Parallel.ForEach(_items.ToList(), (listener, loopState) =>   // 使用平行迭代
                                        {
                                            item.Progress = 10;
                                            try
                                            {
                                                item.Progress = 11;
                                                listener.EventTriger.Invoke(msg);
                                                item.Progress = 12;
                                                if (listener.RemoveCondition != null && listener.RemoveCondition())
                                                {
                                                    item.Progress = 13;
                                                    _items.Remove(listener);
                                                }
                                                item.Progress = 14;
                                            }
                                            catch (Exception ex)
                                            {
                                                FLog.Fatal("Default", "處理監聽到的channel事件發生Exception, channel: {0}, message: {1}, Message: {2}, Reason: {3}", channel, msg, ex.Message, ex.StackTrace);
                                            }
                                            item.Progress = 15;
                                        });
                                        item.Progress = 16;
                                    }
                                }
                                item.Progress = 17;
                            }, (unSubscribeChannel) =>
                            {
                                // 重清單移除此ch
                                item.Channels.RemoveWhere(c => c.Equals(unSubscribeChannel));

                                // 取消事件
                                OnUnSubscribe.Invoke(unSubscribeChannel);
                            });    // broking endAction

                            OnUnSubscribe = null;
                        }
                        catch (Exception ex)
                        {
                            if (!close)
                                FLog.Fatal("Default", "監聽發生Exception, channel: {0}, Message: {1}, Reason: {2}", Json.Serialize(item.Channels), ex.Message, ex.StackTrace);
                        }
                        finally
                        {
                            ChannelListeners.TryRemove(item, out long _id);
                            _client.Dispose();

                            if (OnUnSubscribe != null)
                            {
                                foreach (var ch in item.Channels)
                                {
                                    OnUnSubscribe?.Invoke(ch);
                                }
                                FLog.Fatal("Default", "監聽者已經退出, 歷史監聽頻道 = {0}", Json.Serialize(item.Channels));
                            }
                        }
                    })
                    {
                        IsBackground = true
                    };
                    thread.Start();
                }
            }
            return client != null;
        }

        private void CreateHeartbeat()
        {
            Thread thread = new Thread(() =>
            {
                Exception error = null;
                try
                {
                    using (var client = CreateReadClient())
                    {
                        Stopwatch sp = new Stopwatch();
                        while (heartbeat_delay != Status.Stopped && error == null && !close)
                        {
                            if (RequestsPerHour > 5000) // 快到6000限制 清空
                                PerReset();

                            sp.Reset();
                            sp.Start();
                            if (client != null && client.Ping())
                            {
                                sp.Stop();
                                Interlocked.Exchange(ref heartbeat_status, Status.Starting);
                                OnHeartBeat?.Invoke(this, new ValueArgs<long>(sp.ElapsedMilliseconds));
                                Thread.Sleep(heartbeat_delay);
                            }
                            else
                                error = new Exception("Ping Error");
                        }
                    }
                }
                catch (Exception e)
                {
                    if (close)
                        error = new Exception("Pool Close.");
                    else
                        error = e;
                }
                finally
                {
                    Interlocked.Exchange(ref heartbeat_delay, Status.Stopped);
                    Interlocked.Exchange(ref heartbeat_status, Status.Stopped);
                    OnHeartBeatFail?.Invoke(this, new ValueArgs<Exception>(error));
                }
            })
            { IsBackground = true };
            thread.Start();
        }

        /// <summary>
        /// 管理員
        /// </summary>
        public PooledRedisClientManager Manager { get; private set; }

        /// <summary>
        /// 建立通道管理員
        /// </summary>
        /// <param name="readWriteHost"></param>
        /// <returns></returns>
        public RedisPoolManager BuildConnect(string readWriteHost)
        {
            return BuildConnect(readWriteHost, readWriteHost);
        }

        /// <summary>
        /// 建立通道管理員
        /// </summary>
        /// <param name="writeHost"></param>
        /// <param name="readHost"></param>
        /// <returns></returns>
        public RedisPoolManager BuildConnect(string writeHost, string readHost)
        {
            return BuildConnect(new string[] { writeHost }, new string[] { readHost }, null);
        }

        /// <summary>
        /// 建立通道管理員
        /// </summary>
        /// <param name="writeHost"></param>
        /// <param name="readHost"></param>
        /// <param name="config"></param>
        /// <param name="poolSizeMultiplier"></param>
        /// <param name="poolTimeOutSeconds"></param>
        /// <returns></returns>
        public RedisPoolManager BuildConnect(string[] writeHost, string[] readHost, RedisClientManagerConfig config, int poolSizeMultiplier = 100, int poolTimeOutSeconds = 1)
        {
            Manager = new PooledRedisClientManager(writeHost, readHost, config, null, poolSizeMultiplier, poolTimeOutSeconds);
            close = false;
            if (RequestsPerHour != 0)
                PerReset();
            return this;
        }

        /// <summary>
        /// 取得讀寫用client
        /// </summary>
        /// <param name="index"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public RedisClient CreateReadWriteClient()
        {
            return CheckClient(Manager.GetClient() as RedisClient);
        }

        /// <summary>
        /// 取得只能讀的client
        /// </summary>
        /// <param name="index"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public RedisClient CreateReadClient()
        {
            return CheckClient(Manager.GetReadOnlyClient() as RedisClient);
        }

        public bool IsPoolNormal(string host)
        {
            lock (_lock)
            {
                try
                {
                    if (enginRedisClient == null || !enginRedisClient.Host.Equals(host))
                    {
                        enginRedisClient?.Dispose();
                        enginRedisClient = new RedisClient(host)
                        {
                            SendTimeout = 1000,
                            ReceiveTimeout = 1000,
                            ConnectTimeout = 1000,
                            RetryTimeout = 1000
                        };
                    }
                    return enginRedisClient.Ping();
                }
                catch { }

                return false;
            }
        }

        /// <summary>
        /// 異步執行redis
        /// </summary>
        /// <param name="onGet"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static Task RedisWriteExcuteAsync(Action<Exception, RedisClient> on)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (RedisClient client = Instance.CreateReadWriteClient())
                    {
                        if (client != null)
                            on.Invoke(null, client);
                        else
                            on.Invoke(new Exception("Redis error."), null);
                    }
                }
                catch (Exception e)
                {
                    on.Invoke(e, null);
                }
            });
        }

        /// <summary>
        /// 異步執行redis
        /// </summary>
        /// <param name="onGet"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static Task RedisReadExcuteAsync(Action<Exception, RedisClient> on)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (RedisClient client = Instance.CreateReadClient())
                    {
                        if (client != null)
                            on.Invoke(null, client);
                        else
                            on.Invoke(new Exception("Redis error."), null);
                    }
                }
                catch (Exception e)
                {
                    on.Invoke(e, null);
                }
            });
        }

        /// <summary>
        /// 同步執行redis
        /// </summary>
        /// <param name="onGet"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static void RedisWriteExcute(Action<Exception, RedisClient> on)
        {
            try
            {
                using (RedisClient client = Instance.CreateReadWriteClient())
                {
                    if (client != null)
                        on.Invoke(null, client);
                    else
                        on.Invoke(new Exception("Redis error."), null);
                }
            }
            catch (Exception e)
            {
                on.Invoke(e, null);
            }
        }

        /// <summary>
        /// 同步執行redis
        /// </summary>
        /// <param name="onGet"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static void RedisReadExcute(Action<Exception, RedisClient> on)
        {
            try
            {
                using (RedisClient client = Instance.CreateReadClient())
                {
                    if (client != null)
                        on.Invoke(null, client);
                    else
                        on.Invoke(new Exception("Redis error."), null);
                }
            }
            catch (Exception e)
            {
                on.Invoke(e, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private RedisClient CheckClient(RedisClient client)
        {
            //status 為1 = heartbeat ok, 故不用在ping
            if (Interlocked.CompareExchange(ref heartbeat_status, 1, 1) == 1 || IsPoolNormal(client.Host))
            {
                return client;
            }
            client.Dispose();
            return null;
        }

        /// <summary>
        /// 關閉Pool
        /// </summary>
        public void CloseInstance()
        {
            close = true;
            Manager.Dispose();
            Manager = null;
        }

        /// <summary>
        /// 重置Redis使用次數
        /// </summary>
        public void PerReset()
        {
            ServerVersionNumber = 0;
            socket = null;
            DisposeTimers();
            AssertServerVersionNumber();
        }

        class EventListener
        {
            public Func<bool> RemoveCondition { get; set; }
            public Action<string> EventTriger { get; set; }
        }

        class ChannelListener
        {
            public ChannelListener(RedisClient client, string[] channels)
            {
                this.Channels = new HashSet<string>(channels);
                this.Client = client;
                Id = client.Id;
                Subscription = client.CreateSubscription();
            }
            public long Id { get; private set; }
            public RedisClient Client { get; private set; }
            public HashSet<string> Channels { get; private set; }
            public HashSet<string> FeatureRemoveChannels { get; set; }
            public IRedisSubscription Subscription { get; private set; }
            public bool Cancel { get; set; }

            public int Progress { get; set; }

            public static implicit operator long(ChannelListener item)
            {
                return item.Id;
            }
        }

        class Status
        {
            public const int Stopped = 0;
            public const int Starting = 1;
            public const int Waiting = 0;
            public const int Fail = 1;
            public const int Success = 2;
        }
    }
}