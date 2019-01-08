using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StackExchange.Redis;

namespace FalseApple.Database.Serialization
{
    public class RedisConverter
    {
        /// <summary>
        /// 取得Property名字的函式
        /// </summary>
        public Func<PropertyInfo, string> GetPropertyNameFunc { get; set; } = (property) => property.Name;
        /// <summary>
        /// 自訂類型的序列化方法
        /// </summary>
        private Dictionary<Type, Func<object, RedisValue>> CustomSerializeFunc { get; set; } = new Dictionary<Type, Func<object, RedisValue>>();
        /// <summary>
        /// 自訂類型的反序列化方法
        /// </summary>
        private Dictionary<Type, Func<RedisValue, Type, object>> CustomDeserializeFunc { get; set; } = new Dictionary<Type, Func<RedisValue, Type, object>>();
        /// <summary>
        /// 預設的序列化方法
        /// </summary>
        protected virtual Func<object, RedisValue> DefaultSerialize { get; set; } = (objValue) => objValue.ToString();
        /// <summary>
        /// 預設的反序列化方法
        /// </summary>
        protected virtual Func<RedisValue, Type, object> DefaultDeserialize { get; set; } = (redisValue, resultType) => Convert.ChangeType(redisValue.ToString(), resultType);

        /// <summary>
        /// 轉換Class到HashEntry
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public IEnumerable<HashEntry> Serialize(object obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                var objVal = property.GetValue(obj);
                if (objVal == null) continue;

                string redisValue = string.Empty;
                if (CustomSerializeFunc.ContainsKey(property.PropertyType))
                    redisValue = CustomSerializeFunc[property.PropertyType](objVal);
                else
                    redisValue = DefaultSerialize(objVal);

                string name = GetPropertyNameFunc(property);
                yield return new HashEntry(name, redisValue);
            }
        }

        /// <summary>
        /// 轉換HashEntry到Class
        /// 若該Class成員有JsonPropertyAttribute則使用該Attribute的Name作為Key
        /// </summary>
        /// <typeparam name="T">目標類型</typeparam>
        /// <param name="hashEntries">Hash Entries</param>
        /// <returns></returns>
        public T Deserialize<T>(IEnumerable<HashEntry> hashEntries)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            var obj = Activator.CreateInstance(typeof(T));
            foreach (var property in properties)
            {
                string name = GetPropertyNameFunc(property);

                HashEntry entry = hashEntries.FirstOrDefault(g => g.Name.ToString().Equals(name));
                if (entry.Equals(new HashEntry())) continue;

                if (CustomDeserializeFunc.ContainsKey(property.PropertyType))
                    property.SetValue(obj, CustomDeserializeFunc[property.PropertyType](entry.Value, property.PropertyType));
                else
                    property.SetValue(obj, DefaultDeserialize(entry.Value, property.PropertyType));
            }
            return (T)obj;
        }
    }
}
