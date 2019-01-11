using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PurpleShine.Core.Expansions;

namespace PurpleShine.Core.Helpers
{
    public static class IPGet
    {
        /// <summary>
        /// 取得IPV4
        /// </summary>
        /// <returns></returns>
        public static IPAddress IPV4()
        {
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (IPAddressInformation ipInfo in nics[0].GetIPProperties().UnicastAddresses)
                {
                    if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                        return ipInfo.Address;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        /// <summary>
        /// 取得IPV6
        /// </summary>
        /// <returns></returns>
        public static IPAddress IPV6()
        {
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (IPAddressInformation ipInfo in nics[0].GetIPProperties().UnicastAddresses)
                {
                    if (ipInfo.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        return ipInfo.Address;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }
    }
    
    public class Json
    {
        /// <summary>
        /// 空的Json Object
        /// </summary>
        public const string EmptyOjbect = "{}";

        /// <summary>
        /// 空的Json Array
        /// </summary>
        public const string EmptyArray = "[]";

        /// <summary>
        /// 迭代JPropertys
        /// </summary>
        /// <param name="propertys"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, string>> StringEnumerator(IEnumerable<JProperty> propertys)
        {
            foreach (var property in propertys)
            {
                string value = property.Value.Type == JTokenType.Object || property.Value.Type == JTokenType.Array ? property.Value.ToString(Newtonsoft.Json.Formatting.None) : property.Value.ToString();
                yield return new KeyValuePair<string, string>(property.Name, value);
            }       
        }

        /// <summary>
        /// 迭代JPropertys
        /// </summary>
        /// <param name="propertys"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, JToken>> Enumerator(IEnumerable<JProperty> propertys)
        {
            foreach (var property in propertys)
            {
                yield return new KeyValuePair<string, JToken>(property.Name, property.Value);
            }
        }

        public static JObject Marge(params JObject[] objs)
        {
            if (!objs.Any()) throw new ArgumentOutOfRangeException(nameof(objs));
            JObject one = objs[0];

            for (int i = 1; i < objs.Length; i++)
            {
                one.Merge(JObject.FromObject(objs[i]), new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });
            }
            return one;
        }

        /// <summary>
        /// String 轉成 struct T
        /// </summary>
        /// <typeparam name="T">回傳的資料形態</typeparam>
        /// <param name="jsonString">接收的資料</param>
        /// <returns></returns>
        public static T DeserializeStruct<T>(string jsonString) where T : struct
        {
            if (string.IsNullOrEmpty(jsonString)) throw new NullReferenceException(nameof(jsonString));
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch
            {
                throw new JsonReaderException(typeof(T).Name + "該型別無法反序列化此Json訊息: " + jsonString);
            }
        }

        /// <summary>
        /// String 轉成 class T
        /// </summary>
        /// <typeparam name="T">回傳的資料形態</typeparam>
        /// <param name="jsonString">接收的資料</param>
        /// <returns></returns>
        public static T DeserializeClass<T>(string jsonString) where T : class
        {
            if (string.IsNullOrEmpty(jsonString)) throw new NullReferenceException(nameof(jsonString));
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonString, new JsonSerializerSettings
                {
                   MissingMemberHandling = MissingMemberHandling.Ignore
                });
            }
            catch 
            {
                throw new JsonReaderException(typeof(T).Name + "該型別無法反序列化此Json訊息: " + jsonString);
            }
        }

        /// <summary>
        /// 將新的Json合併至指定Object內
        /// </summary>
        /// <param name="o"></param>
        /// <param name="jsonString"></param>
        public static void PopulateClass(object o, string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString)) throw new NullReferenceException(nameof(jsonString));
            try
            {
                JsonConvert.PopulateObject(jsonString, o);
            }
            catch
            {
                throw new JsonReaderException(o.GetType() + " 該型別不符合該Json格式 " + jsonString);
            }
        }

        /// <summary>
        /// 反序列化Json
        /// </summary>
        /// <param name="jsonString">接收的資料</param>
        /// <returns></returns>
        public static object Deserialize(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString)) throw new NullReferenceException(nameof(jsonString));
            return JsonConvert.DeserializeObject(jsonString);
        }

        /// <summary>
        /// 解析Json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString)) throw new NullReferenceException(nameof(jsonString));
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary>
        ///  T 轉成 String , 迴避null值
        /// </summary>
        /// <typeparam name="T">屬性</typeparam>
        /// <param name="value">要轉byte[]</param>
        /// <returns></returns>
        public static string Serialize(object value, bool nullingore = true)
        {
            if (value.IsNull()) throw new NullReferenceException(nameof(value));
            if (nullingore)
                return JsonConvert.SerializeObject(value, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore    // null值迴避
                });
            else
                return JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// 確認是否為Json
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public static bool IsValidJson(string strInput)
        {
            if (!string.IsNullOrEmpty(strInput))
            {
                strInput = strInput.Trim();
                if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                    (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
                {
                    try
                    {
                        return JToken.Parse(strInput).IsNonNull();
                    }
                    catch (JsonReaderException)
                    {
                        //
                    }
                    catch (Exception)
                    {
                        //
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 使用Selector檢查指定元素是否存在
        /// </summary>
        /// <param name="json">要檢查的Josn字串</param>
        /// <param name="errorKey">若回傳false, 顯示錯誤的Json元素位址集合</param>
        /// <param name="checks">要檢查的項目</param>
        /// <returns>true if pass</returns>
        public static bool CheckJsonKeys(object json, out List<string> errorKey, params string[] checks)
        {
            JToken mainToken;

            if (json is JToken)
                mainToken = json as JToken;
            else if (json is JObject)
                mainToken = (json as JObject).SelectToken("");
            else if (json is string)
                mainToken = JToken.Parse(json as string);
            else
                throw new Exception("Json must be type of JToken or JObject or Json.");

            errorKey = new List<string>();
            for (int i = 0; i < checks.Length; i++)
            {
                string[] array_querys = checks[i].Split('?');
                if (array_querys.Length > 1)
                {
                    List<string> paths = new List<string>();
                    JToken _token = null;
                    for (int j = 0; j < array_querys.Length - 1; j++)
                    {
                        if (paths.Any())
                        {
                            foreach (string item in paths)
                            {
                                paths.Remove(item);
                                _token = mainToken.SelectToken(item + "." + array_querys[j]);
                                if (!TokenExit(_token, array_querys[j + 1], paths))
                                    errorKey.Add(paths.Last() + "." + array_querys[j + 1]);
                            }
                        }
                        else if ((_token = mainToken.SelectToken(array_querys[j])).IsNull())
                            errorKey.Add(paths.Last() + "." + array_querys[j]);
                        else if (!TokenExit(_token, array_querys[j + 1], paths))
                            errorKey.Add(paths.Last() + "." + array_querys[j + 1]);
                    }
                    paths.Clear();
                    paths = null;
                    _token = null;
                }
                else
                {
                    if (!TokenExit(mainToken, checks[i]))
                        errorKey.Add(checks[i]);
                }
                array_querys = null;
            }

            if (!errorKey.Any())
                errorKey = null;

            return errorKey.IsNull();

            bool TokenExit(JToken outer, string path, List<string> paths = null)
            {
                if (paths.IsNonNull())
                {
                    return outer.All(s =>
                    {
                        paths.Add(s.Path);
                        return s.SelectToken(path).IsNonNull();
                    });
                }
                else
                {
                    return outer.SelectToken(path).IsNonNull();
                }
            }
        }

        /// <summary>
        /// 檢查Key是否存在
        /// </summary>
        /// <param name="json">Json字串</param>
        /// <param name="checkitem">要檢查的key集合</param>
        /// <param name="errorList">輸入錯誤的key集合</param>
        /// <returns>true if pass</returns>
        public static bool CheckJsonKeys(object json, List<string> checkitem, out List<string> errorList)
        {
            JObject mainObject;

            if (json is JObject)
                mainObject = json as JObject;
            else if (json is string)
                mainObject = JObject.Parse(json as string);
            else
                throw new Exception("Json must type of JObject or Json");

            errorList = (from a in mainObject.Descendants()
                         where a is JProperty
                         let q = (a as JProperty).Name
                         where !checkitem.Contains(q)
                         select q).ToList();

            if (!errorList.Any())
                errorList = null;

            return errorList.IsNull();
        }

        /// <summary>
        /// 確認指定Key的長度是否符合
        /// </summary>
        /// <param name="json"></param>
        /// <param name="checkitem"></param>
        /// <param name="errorList"></param>
        /// <returns>true if pass</returns>
        public static bool CheckJsonKeyLength(object json, Dictionary<string, int> checkitem, out List<string> errorList)
        {
            JObject mainObject;

            if (json is JObject)
                mainObject = json as JObject;
            else if (json is string)
                mainObject = JObject.Parse(json as string);
            else
                throw new Exception("Json must type of JObject or Json");

            errorList = (from a in mainObject.Descendants()
                         where a is JProperty
                         let q = a as JProperty
                         where checkitem.ContainsKey(q.Name) && checkitem[q.Name] != q.Value.ToString().Length
                         group q by q.Name into q
                         select q.Key).ToList();

            if (!errorList.Any()) errorList = null;

            return errorList.IsNull();
        }

        /// <summary>
        /// 深層複製
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(T @this)
        {
            var serialized = JsonConvert.SerializeObject(@this);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }

    public class Tools
    {
        /// <summary>
        /// 取得本專案路徑
        /// </summary>
        /// <returns></returns>
        public static string GetDirectoryPath() => Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));

    }

    public static class Actions
    {
        /// <summary>
        /// 重複做某件事直到失敗至上限為止
        /// </summary>
        /// <param name="retry">重試次數</param>
        /// <param name="func">動作</param>
        public static void Loop(int retry, Func<bool> func)
        {
            bool complete = false;
            for (int i = 0; i < retry && complete == false; i++)
            {
                complete = func.Invoke();
            }
        }

        /// <summary>
        /// 重複做某件事直到失敗至上限為止
        /// </summary>
        /// <param name="retry">重試次數</param>
        /// <param name="func">動作</param>
        public static async Task LoopAsync(int retry, Func<Task<bool>> func)
        {
            bool complete = false;
            for (int i = 0; i < retry && complete == false; i++)
            {
                complete = await func.Invoke();
            }
        }
    }

    public static class XML
    {
        /// <summary>
        /// 將指定XML檔案所有node移除並保存
        /// </summary>
        /// <param name="filename">檔案名稱</param>
        /// <returns>false: 無此檔案或是移除失敗</returns>
        public static bool ResetXML(string filename)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
                XmlElement root = doc.DocumentElement;
                root.RemoveAll();
                doc.Save(filename);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            return false;
        }

        public static bool RemoveNodeInXML(string filename, params string[] nodes)
        {
            try
            {
                XDocument doc = XDocument.Load(filename);
                doc.Descendants()
                     .Where(x => nodes.Contains(x.Name.LocalName)).Remove();

                doc.Save(filename);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            return false;
        }

        /// <summary>
        /// 將XML檔案反序列化, 若找不到檔案則產生 Type 為 T 的新實體
        /// </summary>
        /// <typeparam name="T">檔案類型</typeparam>
        /// <param name="filename">檔案路徑</param>
        /// <returns></returns>
        public static T DeserializeXML<T>(string filename) where T : class, new()
        {
            try
            {
                if (File.Exists(filename))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    using (StreamReader stringReader = new StreamReader(filename))
                    {
                        return serializer.Deserialize(stringReader) as T;
                    }
                }
                else
                    return new T();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 將XML序列化
        /// </summary>
        /// <typeparam name="T">檔案類型</typeparam>
        /// <param name="t">檔案</param>
        /// <param name="filename">檔案路徑</param>
        public static void SerializeXML<T>(T t, string filename)
        {
            using (Stream stream = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings { Indent = true };
                using (var xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
                {
                    XmlSerializer xsSubmit = new XmlSerializer(t.GetType());
                    xsSubmit.Serialize(xmlWriter, t);
                }
            }
        }

        /// <summary>
        /// 將XML檔案反序列化
        /// </summary>
        /// <typeparam name="T">檔案類型</typeparam>
        /// <param name="xmlstring">檔案路徑</param>
        /// <returns></returns>
        public static T ParseXML<T>(string xmlstring) where T : class
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(xmlstring.Trim());
                writer.Flush();
                stream.Position = 0;
                using (var reader = XmlReader.Create(stream, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Document }))
                {
                    return new XmlSerializer(typeof(T)).Deserialize(reader) as T;
                }
            }
        }

        public static void Serialize(object obj, string filename)
        {
            try
            {
                using (FileStream stream = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    DataContractSerializer serializer = new DataContractSerializer(obj.GetType());

                    var xmlWriterSettings = new XmlWriterSettings { Indent = true };
                    using (var xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
                    {
                        serializer.WriteObject(xmlWriter, obj);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
   
        }

        public static T Deserialize<T>(string filename) where T : class, new()
        {
            if (File.Exists(filename))
            {
                using (Stream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        return serializer.ReadObject(reader) as T;
                    }
                }
            }
            else
                return new T();
        }
    }
}
