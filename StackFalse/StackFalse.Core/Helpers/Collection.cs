using System.Collections.Generic;

namespace StackFalse.Core.Helpers
{
    /// <summary>
    /// List的輔助
    /// </summary>
    public static class Lists
    {
        /// <summary>
        /// 產生一個元素集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static List<T> NewList<T>(params T[] items)
        {
            return new List<T>(items);
        }
    }

    /// <summary>
    /// Dicionary的輔助
    /// </summary>
    public static class Dicionarys
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static Dictionary<K, V> NewDicionary<K, V>(IDictionary<K, V> items)
        {
            return new Dictionary<K, V>(items);
        }
    }
}