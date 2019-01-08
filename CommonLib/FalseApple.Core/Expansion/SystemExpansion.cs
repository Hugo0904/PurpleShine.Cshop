using System;
using System.Diagnostics;
using System.Reflection;

namespace FalseApple.Core.Expansion
{
    public static class SystemExpansion
    {
        /// <summary>
        /// 元件版本
        /// Assembly.GetExecutingAssembly()
        /// </summary>
        public static string GetVersion(this Assembly @this)
        {
            Assembly assembly = @this;
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        /// <summary>
        /// null則拋出NullReferenceException例外
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T RequeireNonNull<T>(this T @this)
        {
            if (@this == null)
                throw new NullReferenceException();
            return @this;
        }

        /// <summary>
        /// 若例外不為null則拋出
        /// </summary>
        /// <param name="this"></param>
        public static void ThrowIfNonNull(this Exception @this)
        {
            if (@this != null)
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
            if (@this == null)
                throw new NullReferenceException(message);
            return @this;
        }

        public static bool IsNull(this object @this)
        {
            return @this == null;
        }

        public static bool IsNonNull(this object @this)
        {
            return @this != null;
        }
    }
}
