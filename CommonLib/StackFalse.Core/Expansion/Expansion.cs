using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using StackFalse.Core.Data;
using StackFalse.Core.Libraries;
using Newtonsoft.Json.Linq;


namespace StackFalse.Core.Expansion
{
    public static class GlobalExpansion
    {
        /// <summary>
        /// 將時間轉成UnixTimeStamp
        /// </summary>
        /// <param name="this">時間Object</param>
        /// <returns></returns>
        public static long ToUnixTimeStamp(this DateTime @this, DateTimeKind kind = DateTimeKind.Unspecified)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, kind);
            return Convert.ToInt64((@this - epoch).TotalSeconds);
        }

        /// <summary>
        /// 將時間轉成毫秒
        /// </summary>
        /// <param name="this">時間Object</param>
        /// <returns></returns>
        public static long ToLongTime(this DateTime @this, DateTimeKind kind = DateTimeKind.Unspecified)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, kind);
            return Convert.ToInt64((@this - epoch).TotalMilliseconds);
        }

        /// <summary>
        /// 取得該時間與目前時間的差距
        /// </summary>
        /// <param name="this"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static double GetIntervalsTime(this long @this, DateTimeKind type = DateTimeKind.Unspecified)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, type).AddMilliseconds(@this).GetIntervalsTime(type);
        }

        /// <summary>
        /// 取得該時間與目前時間的差距
        /// </summary>
        /// <param name="this"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static double GetIntervalsTime(this DateTime @this, DateTimeKind type = DateTimeKind.Unspecified)
        {
            DateTime _now;
            switch (type)
            {
                case DateTimeKind.Local:
                    _now = DateTime.Now;
                    break;

                case DateTimeKind.Utc:
                default:
                    _now = DateTime.UtcNow;
                    break;
            }
            return (_now - @this).TotalMilliseconds;
        }

        /// <summary>
        /// 將毫秒轉成時間
        /// </summary>
        /// <param name="this">時間Object</param>
        /// <returns></returns>
        public static DateTime ToLocalTime(this long @this, string type = "")
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            DateTime dt;
            switch (type)
            {
                case "second":
                    dt = epoch.AddSeconds(@this);
                    break;
                case "hour":
                    dt = epoch.AddHours(@this);
                    break;
                case "day":
                    dt = epoch.AddDays(@this);
                    break;
                default:
                    dt = epoch.AddMilliseconds(@this);
                    break;
            }
            return dt.ToLocalTime();
        }

        /// <summary>
        /// 判斷該陣列中有無任何項目符合該字串
        /// </summary>
        /// <param name="this"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool EqualsAny(this string @this, params string[] args)
        {
            if (@this != null)
            {
                foreach (var item in args)
                {
                    if (item.Equals(@this)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 取得ENUM標籤的值
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static string GetStringValue(this Enum @this)
        {
            Type type = @this.GetType();
            FieldInfo fieldInfo = type.GetField(@this.ToString());
            StringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
            return attribs.Length > 0 ? attribs[0].StringValue : null;
        }

        /// <summary>
        /// 將指定物件內所有Fields轉成Dictionary
        /// key = field name
        /// value = field value
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ConvertDictionary(this object @this)
        {
            return @this.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop =>
                {
                    object o = prop.GetValue(@this);
                    return o == null ? "" : o.ToString();
                });
        }

        /// <summary>
        /// 確認所有條件是有任一為null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root"></param>
        /// <param name="getter"></param>
        /// <returns></returns>
        public static bool IsNull<T>(this T root, Expression<Func<T, object>> getter)
        {
            var visitor = new NullVisitor()
            {
                CurrentObject = root
            };
            visitor.Visit(getter);
            return visitor.IsNull;
        }

        /// <summary>
        /// 透過型態產生新的實例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetObject<T>(this Type t, params object[] args)
        {
            return (T)Activator.CreateInstance(typeof(T), args);
        }


        /// <summary>
        /// 將Object 轉換成動態類別
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static dynamic ToDynamic(this object @this)
        {
            if (@this is ExpandoObject) return @this;
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(@this.GetType()))
                expando.Add(property.Name, property.GetValue(@this));

            return expando as ExpandoObject;
        }

        /// <summary>
        /// 合併兩個類別所有的field成動態類別
        /// </summary>
        /// <param name="this"></param>
        /// <param name="beMerge"></param>
        /// <returns></returns>
        private static dynamic Merge(this object @this, object beMerge)
        {
            IDictionary<string, object> result = new ExpandoObject();

            foreach (var property in @this.GetType().GetProperties())
            {
                if (property.CanRead)
                    result[property.Name] = property.GetValue(@this);
            }

            foreach (var property in beMerge.GetType().GetProperties())
            {
                if (property.CanRead)
                    result[property.Name] = property.GetValue(beMerge);
            }

            return result;
        }

        /// <summary>
        /// 對字串型態的數字做加減
        /// 加減並回傳成字串
        /// </summary>
        /// <param name="this"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string ComputeSNumber(this string @this, Func<long, long> compute)
        {
            if (long.TryParse(@this, out long v))
            {
                return compute(v).ToString();
            }
            throw new ArgumentException($"{@this} not number");
        }

        /// <summary>
        /// object輸出string
        /// 若為null則輸出空字串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string NullToString(this object value)
        {
            return value == null ? "" : value.ToString();
        }

        public static decimal ToDecimal(this object @this)
        {
            return Convert.ToDecimal(@this);

        }

        public static double ToDuble(this object @this)
        {
            return Convert.ToDouble(@this);
        }

        public static long ToInt64(this object @this)
        {
             return Convert.ToInt64(@this);
        }

        public static int ToInt32(this object @this)
        {
            return Convert.ToInt32(@this);
        }

        public static uint ToUInt32(this object @this)
        {
            return Convert.ToUInt32(@this);
        }

        public static int? ToInt32N(this object @this)
        {
            if (@this == null) return null;
            return ToInt32(@this);
        }

        public static short ToInt16(this object @this)
        {
            return Convert.ToInt16(@this);
        }

        public static short? ToInt16N(this object @this)
        {
            if (@this == null) return null;
            return ToInt16(@this);
        }

        public static byte ToInt8(this object @this)
        {
            return Convert.ToByte(@this);
        }

        public static byte? ToInt8N(this object @this)
        {
            if (@this == null) return null;
            return ToInt8(@this);
        }

        public static bool ToBool(this object @this)
        {
            return Convert.ToBoolean(@this);
        }

        /// <summary>
        /// 取得enum標籤的value
        /// </summary>
        /// <param name="this"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static dynamic GetValue(this Enum @this, int index = -1)
        {
            FieldInfo fieldInfo = @this.GetType().GetField(@this.ToString());
            PlanetAttr[] atts = (PlanetAttr[]) fieldInfo.GetCustomAttributes(typeof(PlanetAttr), false);
            dynamic dync = atts.Length > 0 ? atts[0].value : null;
            if (dync != null && index >= 0)
            {
                try
                {
                    return dync[index];
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return dync;
        }
    }

    public static class JsonExpansion
    {
        /// <summary>
        /// 比較JOject property的值是否一樣
        /// 若是null (empty) return false
        /// </summary>
        /// <param name="this"></param>
        /// <param name="property"></param>
        /// <param name="preItem"></param>
        /// <returns></returns>
        public static bool CompareString(this JToken @this, string property, string preItem)
        {
            return @this[property] != null && @this[property].ToString().Equals(preItem);
        }

        /// <summary>
        /// 確認該Value是否存在
        /// </summary>
        /// <param name="this"></param>
        /// <param name="value">存在則返回value, 不存在返回null</param>
        /// <returns>存在返回true, 不存在返回false</returns>
        public static bool IsExist(this JToken @this, out string value)
        {
            if (@this != null)
            {
                switch (@this.Type)
                {
                    case JTokenType.Array:
                    case JTokenType.Object:
                        value = @this.ToString(Newtonsoft.Json.Formatting.None);
                        break;

                    default:
                        value = @this.ToString();
                        break;
                }
            }
            else value = null;
            return value != null;
        }

        /// <summary>
        /// 確認該Value是否存在
        /// </summary>
        /// <param name="this"></param>
        /// <param name="path"></param>
        /// <param name="value">存在則返回value, 不存在返回null</param>
        /// <returns>存在返回true, 不存在返回false</returns>
        public static bool IsExist(this JToken @this, string path, out string value)
        {
            if (@this != null)
            {
                var token = @this.SelectToken(path);
                if (token != null)
                {
                    switch (token.Type)
                    {
                        case JTokenType.Array:
                        case JTokenType.Object:
                            value = token.ToString(Newtonsoft.Json.Formatting.None);
                            break;

                        default:
                            value = token.ToString();
                            break;
                    }
                    return true;
                }
            }
            value = null;
            return false;
        }

        /// <summary>
        /// 確認該Value是否存在
        /// </summary>
        /// <param name="this"></param>
        /// <param name="value">存在則返回value, 不存在返回null</param>
        /// <returns>存在返回true, 不存在返回false</returns>
        public static bool IsExist<T>(this JToken @this, string path, out T value)
        {
            var token = @this.SelectToken(path);
            if (token != null)
            {
                value = token.ToObject<T>();
                return true;
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// 將該JToken轉換為字串
        /// 若不存在則傳回replaceValue
        /// </summary>
        /// <param name="this"></param>
        /// <param name="replaceValue">若為null傳回值</param>
        /// <returns></returns>
        public static string NewValueIfNotExist(this JToken @this, string replaceValue)
        {
            return @this == null ? replaceValue : @this.ToString(Newtonsoft.Json.Formatting.None);
        }

        /// <summary>
        /// 若該index不存在於JToken裡, 則設置新值進去
        /// </summary>
        /// <param name="this"></param>
        /// <param name="path"></param>
        /// <param name="replaceValue">若不存在要設置的值</param>
        /// <returns></returns>
        public static JToken SetIfNotExist(this JToken @this, string path, JToken replaceValue)
        {
            if (@this == null) throw new ArgumentNullException(nameof(@this));
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (@this.SelectToken(path) == null) @this[path] = replaceValue;
            return @this;
        }

        /// <summary>
        /// 比對內容是否相同
        /// object 無視排列順序
        /// </summary>
        /// <param name="this"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool ContentEquals(this JToken @this, JToken @that)
        {
            // 不能為null
            // 型別要一致
            if (@this == null || that == null || @this.Type != that.Type)
                return false;

            switch (@this.Type)
            {
                case JTokenType.Array:
                case JTokenType.Object:
                    if (@this.Count() != @that.Count()) return false;
                    HashSet<string> fiter = new HashSet<string>();
                    foreach (var token in @this)
                    {
                        if (!@this.SelectToken(token.Path).ContentEquals(@that.SelectToken(token.Path)))
                            return false;
                        else
                            fiter.Add(token.Path);
                    }
                    return !@that.Where(i => !fiter.Contains(i.Path)).Any();

                default:
                    return JToken.DeepEquals(@this, @that);
            }
        }
    }

    public static class __
    {
        /// <summary>
        /// 移除該物件內某個事件所有監聽者
        /// </summary>
        /// <param name="this">要移除的物件</param>
        /// <param name="eventName">事件名稱</param>
        public static void ClearEvent(this object @this, string eventName)
        {
            FieldInfo _Field = @this.GetType().GetField(eventName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            if (_Field != null)
            {
                object _FieldValue = _Field.GetValue(@this);
                if (_FieldValue != null && _FieldValue is Delegate)
                {
                    Delegate _ObjectDelegate = (Delegate)_FieldValue;
                    foreach (Delegate del in _ObjectDelegate.GetInvocationList())
                    {
                        @this.GetType().GetEvent(eventName).RemoveEventHandler(@this, del);
                    }
                }
            }
        }

        /// <summary>
        /// 取得MemberInfo標籤屬性值
        /// ex: typeof(T).GetProperty("name").GetAttrValue((DefaultValueAttribute i) => i).Value
        /// ex: typeof(T).GetMethod("name").GetAttrValue((DefaultValueAttribute i) => i).Value
        /// ex: typeof(T).GetAttrValue((DefaultValueAttribute i) => i).Value
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="info"></param>
        /// <param name="valueSelector"></param>
        /// <returns></returns>
        public static TValue GetAttrValue<TAttribute, TValue>(this MemberInfo info, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            var att = info.GetCustomAttribute(typeof(TAttribute), true) as TAttribute;
            return att != null ? valueSelector(att) : default(TValue);
        }
    }

    public static class RandomExpansion
    {
        /// <summary>
        /// 隨機 大寫A-Z 或 0-9
        /// type null = 返回隨機大小寫字母加數字 (預設)
        /// type num = 返回隨機數字
        /// type char = 返回隨機大小寫字母
        /// type upperNum = 返回隨機大寫字母加數字
        /// type lowerNum = 返回隨機小寫字母加數字
        /// </summary>
        /// <param name="this"></param>
        /// <param name="types">模式</param>
        /// <returns></returns>
        public static string GetRandomChar(this Random @this, string types)
        {
            // 0-9
            // A-Z  ASCII值  65-90
            // a-z  ASCII值  97-122
            int i;
            switch (types)
            {
                case "num": // 返回隨機數字
                    i = @this.Next(0, 10);
                    break;

                case "char": // 返回隨機大小寫字母
                    do { i = @this.Next(65, 123); } while (i > 90 && i < 97);
                    break;

                case "upperNum": // 返回隨機大寫字母加數字
                    do { i = @this.Next(0, 91); } while ((i > 9 && i < 65));
                    break;

                case "lowerNum": // 返回隨機小寫字母加數字
                    do { i = @this.Next(0, 123); } while (i > 9 && i < 97);
                    break;

                default: // 返回隨機大小寫字母加數字 (預設)
                    do { i = @this.Next(0, 123); } while (i > 9 && i < 65);
                    break;
            }
            return i < 10 ? i.ToString() : ((char)i).ToString();
        }
    }
}
