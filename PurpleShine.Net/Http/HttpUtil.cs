using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace PurpleShine.Net.Http
{
    /// <summary>
    /// 
    /// </summary>
    public static class HttpUtil
    {
        /// <summary>
        /// 將Collection建立Get請求用格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="addQuestionMark"></param>
        /// <returns></returns>
        public static string BuildGetQuery<T>(Dictionary<string, T> parameters, bool addQuestionMark = true)
        {
            var result = from a in parameters
                         select $"{a.Key}={a.Value.ToString()}";

            return (addQuestionMark ? "?" : "") + string.Join("&", result);
        }

        /// <summary>
        /// 將Collection建立Get請求用格式
        /// 會做 URLEncoding
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="addQuestionMark"></param>
        /// <param name="encodeUpdatCase"></param>
        /// <returns></returns>
        public static string BuildGetEncodeQuery<T>(Dictionary<string, T> parameters, bool addQuestionMark = true, bool encodeUpdatCase = true)
        {
            var reg = new Regex(@"%[a-f0-9]{2}");
            var result = from a in parameters
                         select $"{a.Key}={reg.Replace(HttpUtility.UrlEncode(a.Value.ToString()), m => m.Value.ToUpperInvariant())}";

            return (addQuestionMark ? "?" : "") + string.Join("&", result);
        }
    }
}
