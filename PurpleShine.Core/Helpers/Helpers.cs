using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Reflection;
using PurpleShine.Core.Expansions;

namespace PurpleShine.Core.Helpers
{
    public static class IPGet
    {
        /// <summary>
        /// 取得IPV4
        /// </summary>
        /// <returns></returns>
        public static IPAddress Ipv4()
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
        public static IPAddress Ipv6()
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
        /// 空的Json字串
        /// </summary>
        public const string EmptyJson = "{}";

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

    public static class Safe
    {
        /// <summary>
        /// 將字串作MD5加密
        /// </summary>
        /// <param name="content"></param>
        /// <param name="lower"></param>
        /// <returns></returns>
        public static string MD5(string content, bool lower = true)
        {
            using (MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] b = md5.ComputeHash(Encoding.UTF8.GetBytes(content));
                string result = BitConverter.ToString(b).Replace("-", string.Empty);
                return lower ? result.ToLower() : result;
            }
        }

        /// <summary>
        /// SHA256加密
        /// </summary>
        /// <param name="str">待加密的字串</param>
        /// <returns>加密後的字串(十六進位 兩位數)</returns>
        public static string SHA256(string str)
        {
            SHA256 sha256 = new SHA256Managed();
            byte[] sha256Bytes = Encoding.Default.GetBytes(str);
            byte[] cryString = sha256.ComputeHash(sha256Bytes);
            string sha256Str = string.Empty;
            for (int i = 0; i < cryString.Length; i++)
            {
                sha256Str += cryString[i].ToString("X2");
            }
            return sha256Str;
        }

        public static bool IsPositiveIntegerr(string strNumber)
        {
            //看要用哪種規則判斷，自行修改strValue即可
            //string strValue = @"^\d+[.]?\d*$";//非負數字 
            //strValue = @"^\d+(\.)?\d*$";//數字
            //strValue = @"^\d+$";//非負整數
            //strValue = @"^-?\d+$";//整數
            //strValue = @"^-[0-9]*[1-9][0-9]*$";//負整數
            string strValue = @"^[0-9]*[1-9][0-9]*$";//正整數
            //strValue = @"^((-\d+)|(0+))$";//非正整數                    
            Regex r = new Regex(strValue);
            return r.IsMatch(strNumber);
        }
    }

    public static class Dynamic
    {
        /// <summary>
        /// 檢查元素是否存在於該dynamic
        /// </summary>
        /// <param name="obj">物件</param>
        /// <param name="name">元素名稱</param>
        /// <returns></returns>
        public static bool HasProperty(dynamic obj, string name)
        {
            Type objType = obj.GetType();

            if (objType == typeof(ExpandoObject))
            {
                return ((IDictionary<string, object>)obj).ContainsKey(name);
            }

            return objType.GetProperty(name).IsNonNull();
        }
    }

    public class Tools
    {
        /// <summary>
        /// 判斷有無任何一個物件是null
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static bool StringAnyEmpty(params string[] objs) => objs.Any(i => string.IsNullOrEmpty(i));

        /// <summary>
        /// 判斷有無任何一個物件是null
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static bool AnyNull(params object[] objs) => objs.Any(i => i.IsNull());

        /// <summary>
        /// 判萬有無任何一個物件不是null
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static bool AnyNotNull(params object[] objs) => objs.Any(i => i.IsNonNull());

        /// <summary>
        /// 產生新的Thread 執行action
        /// </summary>
        /// <param name="e"></param>
        public static void Excute(ThreadStart e)
        {
            Thread thread = new Thread(e) { IsBackground = true };
            thread.Start();
        }

        /// <summary>
        /// 匿名類轉換
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anonymous"></param>
        /// <param name="anonymousType"></param>
        /// <returns></returns>
        public static T CastAnonymous<T>(object anonymous, T anonymousType)
        {
            return (T) anonymous;
        }

        /// <summary>
        /// 取得本專案路徑
        /// </summary>
        /// <returns></returns>
        public static string GetDirectoryPath() => Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));

        /// <summary>
        /// 將text繪製成圖片
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="textColor"></param>
        /// <param name="backColor"></param>
        /// <returns></returns>
        public static Image DrawTextPicture(string text, Font font, Color textColor, Color backColor)
        {
            using (Image img = new Bitmap(1, 1))
            using (Graphics drawing = Graphics.FromImage(img))
            {
                SizeF textSize = drawing.MeasureString(text, font);
                Image img2 = new Bitmap((int)textSize.Width, (int)textSize.Height);
                using (Graphics drawing2 = Graphics.FromImage(img2))
                {
                    drawing2.SmoothingMode = SmoothingMode.AntiAlias;
                    drawing2.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    drawing2.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    drawing2.Clear(backColor);
                    using (Brush textBrush = new SolidBrush(textColor))
                    {
                        drawing2.DrawString(text, font, textBrush, 0, 0);
                        drawing2.Flush();
                        return img2;
                    }
                }
            }
        }

        /// <summary>
        /// 將text繪製成圖片(圓形)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="textColor"></param>
        /// <param name="backColor"></param>
        /// <returns></returns>
        public static Image DrawTextRectanglePicture(string text, Font font, Color textColor, Color backColor)
        {
            using (Image img = new Bitmap(1, 1))
            using (Graphics drawing = Graphics.FromImage(img))
            {
                SizeF textSize = drawing.MeasureString(text, font);
                Image img2 = new Bitmap((int)textSize.Width, (int)textSize.Height);
                using (Graphics drawing2 = Graphics.FromImage(img2))
                using (GraphicsPath path = new GraphicsPath())
                {
                    drawing2.SmoothingMode = SmoothingMode.AntiAlias;
                    drawing2.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    drawing2.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    path.AddEllipse(0, 0, img2.Width, img2.Height);
                    drawing2.SetClip(path);
                    using (Brush textBrush = new SolidBrush(textColor))
                    using (Brush br = new SolidBrush(backColor))
                    {
                        drawing2.FillEllipse(br, drawing2.ClipBounds);
                        drawing2.DrawString(text, font, textBrush, 0, 0);
                    }
                    drawing2.Flush();
                    return img2;
                }
            }
        }

        /// <summary>
        /// 將字劃到圖形內
        /// </summary>
        /// <param name="image"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="textColor"></param>
        /// <param name="site"></param>
        public static void DrawTextInPicture(Image image, string text, Font font, Color textColor, ContentAlignment site)
        {
            using (Graphics drawing = Graphics.FromImage(image))
            {
                drawing.SmoothingMode = SmoothingMode.AntiAlias;
                drawing.InterpolationMode = InterpolationMode.HighQualityBicubic;
                drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (Brush textBrush = new SolidBrush(textColor))
                using (StringFormat sf = new StringFormat())
                {
                    string siteName = site.ToString();
                    sf.LineAlignment = siteName.StartsWith("Top") ? StringAlignment.Near : siteName.StartsWith("Middle") ? StringAlignment.Center : StringAlignment.Far;
                    sf.Alignment = siteName.EndsWith("Left") ? StringAlignment.Near : siteName.EndsWith("Center") ? StringAlignment.Center : StringAlignment.Far;
                    Rectangle rectf = new Rectangle(0, 0, image.Width, image.Height);
                    drawing.DrawString(text, font, textBrush, rectf, sf);
                }
                drawing.Flush();
            }
        }

        /// <summary>
        /// 字體大小自動變更
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="size"></param>
        /// <param name="font"></param>
        /// <param name="str"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static float NewFontSize(Graphics graphics, Size size, Font font, string str, Func<float, float> func = null)
        {
            SizeF stringSize = graphics.MeasureString(str, font);
            float f = font.Size * Math.Min(size.Height * 0.96f / stringSize.Height, size.Width * 0.9f / stringSize.Width);
            f = func.IsNull() ? f : func(f);
            return f < 0 || float.IsInfinity(f)? 1f : f;
        }

        /// <summary>
        /// 異步處理上下文
        /// </summary>
        /// <param name="actions"></param>
        public static void ContextAsync(Action<IProgress<Action>> actions)
        {
            Progress<Action> progress = new Progress<Action>();
            progress.ProgressChanged += (object sender, Action e) => e.Invoke();
            Task.Run(() => actions.Invoke(progress));
        }

        /// <summary>
        /// 重複做某件事直到失敗至上限為止
        /// </summary>
        /// <param name="retry">重試次數</param>
        /// <param name="func">動作</param>
        public static void ActionLoop(int retry, Func<bool> func)
        {
            bool complete = false;
            for (int i = 0; i < retry && complete == false; i++)
            {
                complete = func.Invoke();
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
