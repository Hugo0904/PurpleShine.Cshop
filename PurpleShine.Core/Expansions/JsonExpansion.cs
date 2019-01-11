using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PurpleShine.Core.Expansions
{
    /// <summary>
    /// Json擴充
    /// </summary>
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
            return @this[property].IsNonNull() && @this[property].ToString().Equals(preItem);
        }

        /// <summary>
        /// 確認該Value是否存在
        /// </summary>
        /// <param name="this"></param>
        /// <param name="value">存在則返回value, 不存在返回null</param>
        /// <returns>存在返回true, 不存在返回false</returns>
        public static bool IsExist(this JToken @this, out string value)
        {
            if (@this.IsNonNull())
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
            return value.IsNonNull();
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
            if (@this.IsNonNull())
            {
                var token = @this.SelectToken(path);
                if (token.IsNonNull())
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
        /// <param name="path"></param>
        /// <param name="value">存在則返回value, 不存在返回null</param>
        /// <returns>存在返回true, 不存在返回false</returns>
        public static bool IsExist<T>(this JToken @this, string path, out T value)
        {
            var token = @this.SelectToken(path);
            if (token.IsNonNull())
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
            return @this.IsNull() ? replaceValue : @this.ToString(Newtonsoft.Json.Formatting.None);
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
            if (@this.IsNull()) throw new ArgumentNullException(nameof(@this));
            if (path.IsNull()) throw new ArgumentNullException(nameof(path));
            if (@this.SelectToken(path).IsNull()) @this[path] = replaceValue;
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
            if (@this.IsNull() || that.IsNull() || @this.Type != that.Type)
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
}
