using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PurpleShine.Core.Models;
using PurpleShine.Core.Models.Attributes;

namespace PurpleShine.Core.Expansions
{
    public static class EnumExpansion
    {
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
            return att.IsNonNull() ? valueSelector(att) : default(TValue);
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
            PlanetAttribute[] atts = (PlanetAttribute[])fieldInfo.GetCustomAttributes(typeof(PlanetAttribute), false);
            dynamic dync = atts.Length > 0 ? atts[0].value : null;
            if (dync.IsNonNull() && index >= 0)
            {
                return dync[index];
            }

            return dync;
        }
    }

    public static class ObjectExpansion
    {
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
        /// null則拋出NullReferenceException例外
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T RequeireNonNull<T>(this T @this)
        {
            if (@this.IsNull())
                throw new NullReferenceException();
            return @this;
        }

        /// <summary>
        /// 若例外不為null則拋出
        /// </summary>
        /// <param name="this"></param>
        public static void ThrowIfNonNull(this Exception @this)
        {
            if (@this.IsNonNull())
                throw @this;
        }

        /// <summary>
        /// 引用該實體
        /// 若實體為Null則拋出NullReferenceException例外
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static T RequeireNonNull<T>(this T @this, string message)
        {
            if (@this.IsNull())
                throw new NullReferenceException(message);
            return @this;
        }

        /// <summary>
        /// 判斷是否為Null
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsNull(this object @this)
        {
            return @this == null;
        }

        /// <summary>
        /// 判斷是否不為Null
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsNonNull(this object @this)
        {
            return @this != null;
        }

        /// <summary>
        /// object輸出string
        /// 若為null則輸出空字串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string NullToString(this object value)
        {
            return value.IsNull() ? "" : value.ToString();
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
    }

    public static class StringExpansion
    {
        public static string UcFirst(this string @this)
        {
            switch (@this)
            {
                case null: throw new ArgumentNullException(nameof(@this));
                case "": throw new ArgumentException($"{nameof(@this)} cannot be empty", nameof(@this));
                default: return @this.First().ToString().ToUpper() + @this.Substring(1);
            }
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
        /// 判斷該陣列中有無任何項目符合該字串
        /// </summary>
        /// <param name="this"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool EqualsAny(this string @this, params string[] args)
        {
            if (@this.IsNonNull())
            {
                foreach (var item in args)
                {
                    if (item.Equals(@this)) return true;
                }
            }
            return false;
        }
    }

    public static class TimeExpansion
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
    }

    public static class ConvertExpansion
    {
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
                    return o.IsNull() ? "" : o.ToString();
                });
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
        /// 轉換Decimal
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this object @this)
        {
            return Convert.ToDecimal(@this);

        }

        /// <summary>
        /// 轉換Double
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static double ToDouble(this object @this)
        {
            return Convert.ToDouble(@this);
        }

        public static long ToInt64(this object @this)
        {
             return Convert.ToInt64(@this);
        }

        public static ulong ToUInt64(this object @this)
        {
            return Convert.ToUInt64(@this);
        }

        public static long? ToInt64N(this object @this)
        {
            if (@this.IsNull()) return null;
            return ToInt64(@this);
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
            if (@this.IsNull()) return null;
            return ToInt32(@this);
        }

        public static short ToInt16(this object @this)
        {
            return Convert.ToInt16(@this);
        }

        public static short? ToInt16N(this object @this)
        {
            if (@this.IsNull()) return null;
            return ToInt16(@this);
        }

        public static byte ToInt8(this object @this)
        {
            return Convert.ToByte(@this);
        }

        public static byte? ToInt8N(this object @this)
        {
            if (@this.IsNull()) return null;
            return ToInt8(@this);
        }

        public static bool ToBool(this object @this)
        {
            return Convert.ToBoolean(@this);
        }
    }
}
