using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using PurpleShine.Core.Expansions;

namespace PurpleShine.Core.Helpers
{

    public static class Objects
    {
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
        /// 匿名類轉換
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anonymous"></param>
        /// <param name="anonymousType"></param>
        /// <returns></returns>
        public static T CastAnonymous<T>(object anonymous, T anonymousType)
        {
            return (T)anonymous;
        }
    }

    public static class Dynamics
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

    public static class Strings
    {
        /// <summary>
        /// 判斷有無任何一個物件是null
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static bool AnyEmpty(params string[] objs) => objs.Any(i => string.IsNullOrEmpty(i));

        public static bool IsPositiveInteger(string strNumber)
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
}
