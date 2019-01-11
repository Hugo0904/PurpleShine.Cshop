using Newtonsoft.Json;
using PurpleShine.Core.Expansions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PurpleShine.Core.Helpers
{
    public static class DictionaryConverter
    {
        static DictionaryConverter()
        {
            CustomSerializer.TryAdd(typeof(byte), DefaultSerializer);
            CustomSerializer.TryAdd(typeof(bool), DefaultSerializer);
            CustomSerializer.TryAdd(typeof(short), DefaultSerializer);
            CustomSerializer.TryAdd(typeof(int), DefaultSerializer);
            CustomSerializer.TryAdd(typeof(long), DefaultSerializer);
            CustomSerializer.TryAdd(typeof(float), DefaultSerializer);
            CustomSerializer.TryAdd(typeof(double), DefaultSerializer);
            CustomSerializer.TryAdd(typeof(string), DefaultSerializer);

            CustomDeserializer.TryAdd(typeof(byte), DefaultDeserializer<byte>);
            CustomDeserializer.TryAdd(typeof(bool), DefaultDeserializer<bool>);
            CustomDeserializer.TryAdd(typeof(short), DefaultDeserializer<short>);
            CustomDeserializer.TryAdd(typeof(int), DefaultDeserializer<int>);
            CustomDeserializer.TryAdd(typeof(long), DefaultDeserializer<long>);
            CustomDeserializer.TryAdd(typeof(float), DefaultDeserializer<float>);
            CustomDeserializer.TryAdd(typeof(double), DefaultDeserializer<double>);
            CustomDeserializer.TryAdd(typeof(string), DefaultDeserializer<string>);
        }

        private static string GetPropertyName(PropertyInfo propertyInfo)
        {
            var jsonAttribute = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
            if (jsonAttribute.IsNonNull())
                return jsonAttribute.PropertyName;
            else
                return propertyInfo.Name;
        }

        private static bool IsAllowDefault(PropertyInfo propertyInfo)
        {
            var jsonAttribute = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
            if (jsonAttribute.IsNonNull())
                return jsonAttribute.Required == Required.AllowNull;
            else
                return false;
        }

        private static bool IsDefault<T>(T value)
        {
            if (value.IsNull())
                return true;
            Type t = value.GetType();
            if (t.IsValueType)
                return value.Equals(Activator.CreateInstance<T>());
            else
                return false;
        }
        #region Serialize
        private static readonly ConcurrentDictionary<Type, Func<object, object>> CustomSerializer = new ConcurrentDictionary<Type, Func<object, object>>();

        public static void RegisterCunstomSerializer(Type type, Func<object, object> serializer)
        {
            CustomSerializer[type] = serializer;
        }

        public static Dictionary<string, object> Serialize(object data)
        {
            Dictionary<string, object> response = new Dictionary<string, object>();

            if (data.IsNull())
                return response;

            foreach (var property in data.GetType().GetProperties())
            {
                object propertyValue = property.GetValue(data);
                string propertyName = GetPropertyName(property);

                if(!IsDefault(propertyValue) || IsAllowDefault(property))
                    response.Add(propertyName, SerializeProperty(propertyValue));
            }

            return response;
        }

        private static object SerializeProperty(object propertyValue)
        {
            if (propertyValue.IsNull())
                return null;

            Type propertyType = propertyValue.GetType();
            if (propertyType.GetInterface("IDictionary").IsNonNull())
                return SerializeDictionary(propertyValue as IDictionary);

            if (propertyType.IsArray)
                return SerializeArray(propertyValue as Array);

            if (propertyType.IsEnum)
                return Convert.ChangeType(propertyValue, propertyType.GetEnumUnderlyingType());

            return SerializeValue(propertyValue);
        }

        private static object SerializeValue(object data)
        {
            if (data.IsNull())
                return null;

            Type dataType = data.GetType();

            if (CustomSerializer.ContainsKey(dataType))
                return CustomSerializer[dataType].Invoke(data);

            return Serialize(data);
        }

        private static Dictionary<object, object> SerializeDictionary(IDictionary dicData)
        {
            Dictionary<object, object> response = new Dictionary<object, object>();
            foreach (DictionaryEntry entry in dicData)
            {
                response.Add(SerializeProperty(entry.Key), SerializeProperty(entry.Value));
            }
            return response;
        }

        private static object[] SerializeArray(Array arrData)
        {
            object[] response = new object[arrData.Length];
            for (int i = 0; i < arrData.Length; i++)
            {
                var arrDatum = arrData.GetValue(i);
                response[i] = SerializeProperty(arrDatum);
            }
            return response.ToArray();
        }

        private static object DefaultSerializer(object value) => value;
        #endregion
        #region Deserialize
        private static readonly ConcurrentDictionary<Type, Func<object, object>> CustomDeserializer = new ConcurrentDictionary<Type, Func<object, object>>();

        public static void RegisterCunstomDeserializer(Type type, Func<object, object> deserializer)
        {
            CustomDeserializer[type] = deserializer;
        }
        public static T Deserialize<T>(IDictionary dicData)
        {
            T response = Activator.CreateInstance<T>();

            foreach (var property in response.GetType().GetProperties())
            {
                string propertyName = GetPropertyName(property);
                if (!dicData.Contains(propertyName))
                    continue;

                Type propertyType = property.PropertyType;
                object propertyValue = DeserializeProperty(propertyType, dicData[propertyName]);

                property.SetValue(response, propertyValue);
            }

            return response;
        }

        public static object Deserialize(Type type, IDictionary dicData)
        {
            if (dicData.IsNull())
                return null;

            object response = Activator.CreateInstance(type);

            foreach (var property in response.GetType().GetProperties())
            {
                string propertyName = GetPropertyName(property);
                if (!dicData.Contains(propertyName))
                    continue;

                Type propertyType = property.PropertyType;
                object propertyValue = DeserializeProperty(propertyType, dicData[propertyName]);

                property.SetValue(response, propertyValue);
            }

            return response;
        }

        private static object DeserializeProperty(Type propertyType, object propertyValue)
        {
            if (propertyValue.IsNull())
                return null;

            if (propertyType.IsAbstract)
                return null;
            
            if (propertyType.GetInterface("IDictionary").IsNonNull())
                return DeserializeDictionary(propertyType, propertyValue as IDictionary);

            if (propertyType.IsArray)
                return DeserializeArray(propertyType, propertyValue as Array);

            if (propertyType.IsEnum)
                return Convert.ChangeType(propertyValue, propertyType.GetEnumUnderlyingType());

            return DeserializeValue(propertyType, propertyValue);
        }

        private static IDictionary DeserializeDictionary(Type propertyType, IDictionary propertyValue)
        {
            Type KeyType = propertyType.GetGenericArguments()[0];
            Type ValueType = propertyType.GetGenericArguments()[1];
            IDictionary response = Activator.CreateInstance(propertyType) as IDictionary;
            foreach (DictionaryEntry entry in propertyValue)
            {
                response.Add(DeserializeProperty(KeyType, entry.Key), DeserializeProperty(ValueType, entry.Value));
            }
            return response;
        }

        private static Array DeserializeArray(Type propertyType, Array propertyValue)
        {
            Array response = Array.CreateInstance(propertyType.GetElementType(), propertyValue.Length);
            for (int i = 0; i < propertyValue.Length; i++)
            {
                response.SetValue(DeserializeProperty(propertyType.GetElementType(), propertyValue.GetValue(i)), i);
            }
            return response;
        }

        private static object DeserializeValue(Type propertyType, object propertyValue)
        {
            if (CustomDeserializer.ContainsKey(propertyType))
                return CustomDeserializer[propertyType].Invoke(propertyValue);
            else
                return Deserialize(propertyType, propertyValue as IDictionary);
        }

        private static object DefaultDeserializer<T>(object value) => Convert.ChangeType(value, typeof(T));
        #endregion
    }
}
