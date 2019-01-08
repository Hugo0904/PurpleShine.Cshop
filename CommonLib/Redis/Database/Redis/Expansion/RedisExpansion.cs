﻿using CommonLib.Expansion;
using CommonLib.Helpers;
using Newtonsoft.Json;
using ServiceStack.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MegaMedia.Database.Redis.Expansion
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

        /*********************************StackExchange.Redis******************************************/
        /// <summary>
        /// 將Dictionary所有key value設置近去Redis
        /// Key = Field
        /// Value = Value
        /// </summary>
        /// <param name="client"></param>
        /// <param name="hashid"></param>
        /// <param name="ertrys"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static IDatabase SetEntrysToHash(this IDatabase client, string hashid, Dictionary<string, string> ertrys, CommandFlags flags = CommandFlags.None)
        {
            client.HashSet(hashid, ertrys.Select(pair => new HashEntry(pair.Key, pair.Value)).ToArray(), flags);
            return client;
        }

        /// <summary>
        /// 將Dictionary所有key value設置近去Redis, 若該Field不存在
        /// Key = Field
        /// Value = Value
        /// </summary>
        /// <param name="client"></param>
        /// <param name="hashid"></param>
        /// <param name="ertrys"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static async Task<IDatabase> HashSetIfNotExistAsync(this IDatabase client, string hashid, Dictionary<string, string> ertrys, CommandFlags flags = CommandFlags.None)
        {
            await Task.WhenAll(from a in ertrys select client.HashSetAsync(hashid, a.Key, a.Value, When.NotExists, flags));
            return client;
        }

        /// <summary>
        /// 將Dictionary所有key value設置近去Redis, 若該Field不存在
        /// Key = Field
        /// Value = Value
        /// </summary>
        /// <param name="client"></param>
        /// <param name="hashid"></param>
        /// <param name="ertrys"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static IDatabase SetHashEntrysNotExist(this IDatabase client, string hashid, Dictionary<string, string> ertrys, CommandFlags flags = CommandFlags.None)
        {
            foreach (var ertry in ertrys)
            {
                client.HashSet(hashid, ertry.Key, ertry.Value, When.NotExists, flags);
            }
            return client;
        }

        /// <summary>
        /// 取得所有Hash值
        /// </summary>
        /// <param name="client"></param>
        /// <param name="hashid"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static Dictionary<string, string> HashGetAllByDictionary(this IDatabase client, string hashid, CommandFlags flags = CommandFlags.None)
        {
            return client.HashGetAll(hashid, flags).ToStringDictionary();
        }

        /// <summary>
        /// 異步取得所有Hash值
        /// </summary>
        /// <param name="client"></param>
        /// <param name="hashid"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static async Task<Dictionary<string, string>> GetAllEntriesFromHashAsync(this IDatabase client, string hashid, CommandFlags flags = CommandFlags.None)
        {
            var r = await client.HashGetAllAsync(hashid, flags).ConfigureAwait(false);
            return r.ToStringDictionary();
        }

        /// <summary>
        /// 將 string[] 轉換至 RedisValue[]
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static RedisValue[] ConvertToRedisValueArr(this string[] arr) 
        {
            return Array.ConvertAll(arr, item => (RedisValue)item);
        }

        /// <summary>
        /// 將 string[] 轉換至 RedisKey[]
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static RedisKey[] ConvertToRedisKeyArr(this string[] arr)
        {
            return Array.ConvertAll(arr, item => (RedisKey)item);
        }

        /// <summary>
        /// 將一個Dictionary 轉換成  HashEntry[]
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static HashEntry[] ToHashEntries(this Dictionary<string, object> obj)
        {
            return obj.Where(entry => entry.Value.IsNonNull())
                .Select(entry => new HashEntry(entry.Key, entry.Value.ToString()))
                .ToArray();
        }

        /// <summary>
        /// 將一個物件所有Properties 轉換成  HashEntry[]
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static HashEntry[] ToHashEntries(this object obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            return properties.Select(property => new HashEntry(property.Name, property.GetValue(obj).ToString())).ToArray();
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this HashEntry[] hashEntries)
        {
            return new Dictionary<TKey, TValue>((IDictionary<TKey, TValue>)hashEntries.Select((entry) => new KeyValuePair<TKey, TValue>((TKey)Convert.ChangeType(entry.Name, typeof(TKey)), (TValue)Convert.ChangeType(entry.Value, typeof(TValue)))));
        }

        /// <summary>
        /// 將HashEntry[]所有資料 填入 T 的 Fields
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashEntries"></param>
        /// <returns></returns>
        public static T ConvertFromRedis<T>(this HashEntry[] hashEntries)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            var obj = Activator.CreateInstance(typeof(T));
            var emptyEntry = new HashEntry();
            foreach (var property in properties)
            {
                HashEntry entry = hashEntries.FirstOrDefault(g => g.Name.ToString().Equals(property.Name));
                if (entry.Equals(emptyEntry)) continue;
                property.SetValue(obj, Convert.ChangeType(entry.Value.ToString(), property.PropertyType));
            }
            return (T)obj;
        }

        /// <summary>
        /// 轉換Class到HashEntry
        /// 若該Class成員有JsonPropertyAttribute則使用該Attribute的Name作為Key
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<HashEntry> ConvertJsonClassToHashEntry(this object obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                var attr = property.GetCustomAttribute<JsonPropertyAttribute>();
                string name = attr.IsNonNull() ? attr.PropertyName : property.Name;
                var objVal = property.GetValue(obj);
                if (objVal.IsNull()) continue;
                string value = property.PropertyType.IsArray ? Json.Serialize(objVal) : objVal.ToString();
                yield return new HashEntry(name, value);
            }
        }

        /// <summary>
        /// 轉換HashEntry到Class
        /// 若該Class成員有JsonPropertyAttribute則使用該Attribute的Name作為Key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashEntries"></param>
        /// <returns></returns>
        public static T ConvertHashEntriesToJsonClass<T>(this IEnumerable<HashEntry> hashEntries)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            var obj = Activator.CreateInstance(typeof(T));
            foreach (var property in properties)
            {
                var attr = property.GetCustomAttribute<JsonPropertyAttribute>();
                string name = attr.IsNonNull() ? attr.PropertyName : property.Name;
                HashEntry entry = hashEntries.FirstOrDefault(g => g.Name.ToString().Equals(name));
                if (entry.Equals(new HashEntry())) continue;

                if (property.PropertyType.IsArray)
                {
                    property.SetValue(obj, JsonConvert.DeserializeObject(entry.Value.ToString(), property.PropertyType));
                }
                else
                {
                    property.SetValue(obj, Convert.ChangeType(entry.Value.ToString(), property.PropertyType));
                }
            }
            return (T)obj;
        }
    }
}