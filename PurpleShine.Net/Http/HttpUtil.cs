using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurpleShine.Net.Http
{
    public static class HttpUtil
    {
        /// <summary>
        /// 將Collection建立Get請求用格式
        /// </summary>
        /// <param name="parameters">容器</param>
        /// <returns></returns>
        public static string BuildGetQuery<T>(Dictionary<string, T> parameters)
        {
            var result = from a in parameters
                         select $"{a.Key}={a.Value.ToString()}";
            return "?" + string.Join("&", result);
        }
    }
}
