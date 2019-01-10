using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StackFalse.Core.Components
{
    public class IOEDictionary<T, U> : ConcurrentDictionary<T, U> where U : class
    {
        private int _maxCapacity = 0;

        public IOEDictionary()
        {
            _maxCapacity = -1;
        }

        public IOEDictionary(int maxCapacity)
        {
            _maxCapacity = maxCapacity;
        }

        public new bool TryAdd(T t, U u)
        {
            if (_maxCapacity != -1 && Count == _maxCapacity) return false;
            return base.TryAdd(t, u);
        }

        /// <summary>
        /// 將指定的Key 換成 新的Key
        /// </summary>
        /// <param name="Old">原本的key</param>
        /// <param name="New">新key</param>
        /// <param name="u">回傳原本value</param>
        /// <returns>true if update key suceess</returns>
        public bool TryUpdateKey(T Old, T New, out U u)
        {
            if (TryRemove(Old, out u))
            {
                if (TryAdd(New, u))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 將指定的Key 換成 新的Key
        /// 若新的Key 已存在則移除
        /// 並返回被移除的Key的Value
        /// 若沒有重複 返回 null
        /// </summary>
        /// <param name="_old">原本的key</param>
        /// <param name="_new">新key</param>
        /// <param name="u">若New Key 則回傳該Key's value or null</param>
        /// <returns>true if update key success</returns>
        public bool TryUpdateKeyAndRemoveExist(T _old, T _new, out U newValue, out U oldValue)
        {
            TryRemove(_new, out newValue);
            if (TryRemove(_old, out oldValue))
            {
                if (TryAdd(_new, oldValue))
                {
                    return true;
                }
            }
            return false;
        }
    }

    [Serializable]
    public class SDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            try
            {
                XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
                XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

                bool wasEmpty = reader.IsEmptyElement;
                reader.Read();

                if (wasEmpty)
                    return;

                reader.ReadStartElement("item");

                while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
                {
                    reader.ReadStartElement("key");
                    TKey key = (TKey)keySerializer.Deserialize(reader);
                    reader.ReadEndElement();

                    reader.ReadStartElement("value");
                    TValue value = (TValue)valueSerializer.Deserialize(reader);
                    reader.ReadEndElement();

                    Add(key, value);

                    reader.MoveToContent();
                }

                reader.ReadEndElement();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            try
            {
                XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
                XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

                writer.WriteStartElement("item");

                foreach (TKey key in Keys)
                {
                    writer.WriteStartElement("key");
                    keySerializer.Serialize(writer, key);
                    writer.WriteEndElement();

                    writer.WriteStartElement("value");
                    TValue value = this[key];
                    valueSerializer.Serialize(writer, value);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

        }
        #endregion
    }

    public class SConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>, IXmlSerializable
    {
        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            try
            {
                XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
                XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

                bool wasEmpty = reader.IsEmptyElement;
                reader.Read();

                if (wasEmpty)
                    return;

                reader.ReadStartElement("item");

                while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
                {
                    reader.ReadStartElement("key");
                    TKey key = (TKey)keySerializer.Deserialize(reader);
                    reader.ReadEndElement();

                    reader.ReadStartElement("value");
                    TValue value = (TValue)valueSerializer.Deserialize(reader);
                    reader.ReadEndElement();

                    TryAdd(key, value);

                    reader.MoveToContent();
                }

                reader.ReadEndElement();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            try
            {
                XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
                XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

                writer.WriteStartElement("item");

                foreach (TKey key in Keys)
                {
                    writer.WriteStartElement("key");
                    keySerializer.Serialize(writer, key);
                    writer.WriteEndElement();

                    writer.WriteStartElement("value");
                    TValue value = this[key];
                    valueSerializer.Serialize(writer, value);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

        }
        #endregion
    }
}
