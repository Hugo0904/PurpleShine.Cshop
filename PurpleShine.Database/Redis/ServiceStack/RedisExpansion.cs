using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Redis;

namespace PurpleShine.Database.Redis.ServiceStack
{
    public static class RedisExpansion
    {
        /// <summary>
        /// 確認此Connect是否還活著 (此確認不一定正確, 此為最後一次操作的狀態)
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static bool IsAlive(this RedisClient client)
        {
            try
            {
                return client.IsSocketConnected() && !client.HadExceptions;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static RedisClient SetEntrysToHash(this RedisClient client, string key, Dictionary<string, string> fields)
        {
            client.SetRangeInHash(key, fields);
            return client;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static RedisClient SetEntrysToHashIfNotExists(this RedisClient client, string key, Dictionary<string, string> fields)
        {
            if (client.Exists(key) == 0)
            {
                client.SetRangeInHash(key, fields);
            }
            return client;
        }

        /// <summary>
        /// 監聽某個頻道, 此方法會造成當前執行緒blocking
        /// </summary>
        /// <param name="client"></param>
        /// <param name="channels">要訂閱的頻道</param>
        /// <param name="onAction">成功訂閱頻道事件</param>
        /// <param name="msgAction">接收訊息事件</param>
        /// <param name="endAction">解除訂閱頻道事件</param>
        public static void SubscribeChannels(this IRedisSubscription subscription, string[] channels, Action<string> onAction, Action<string, string> msgAction, Action<string> endAction)
        {
            using (subscription)
            {
                subscription.OnSubscribe = onAction;    // 當成功訂閱頻道事件
                subscription.OnMessage = msgAction;     // 訂閱接收訊息事件
                subscription.OnUnSubscribe = endAction; // 當解除訂閱頻道事件
                subscription.SubscribeToChannels(channels); // 開始訂閱
            }
        }

        /// <summary>Transaction
        /// 建立 Transaction
        /// </summary>
        /// <param name="client"></param>
        /// <param name="watchs">監視keys</param>
        /// <returns></returns>
        public static IRedisTransaction CustomTransaction(this RedisClient client, params string[] watchs)
        {
            if (watchs.Any())
            {
                client.Watch(watchs);
            }
            return client.CreateTransaction();
        }
    }
}