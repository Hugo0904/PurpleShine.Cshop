using System;
using System.Collections.Generic;
using System.Linq;

namespace PurpleShine.Core.Expansions
{
    /// <summary>
    /// 
    /// </summary>
    public static class IListExpansion
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IList<T> ListAdd<T>(this IList<T> @this, T item)
        {
            @this.Add(item);
            return @this;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class IDictionaryExpansion
    {
        /// <summary>
        /// 合併另一個字典
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="this"></param>
        /// <param name="dict"></param>
        /// <param name="forceCover"></param>
        /// <returns></returns>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> dict, bool forceCover = false) where TValue : class
        {
            return @this.Keys.Union(dict.Keys).ToDictionary(k => k, k => (@this.ContainsKey(k) && !forceCover) ? @this[k] : dict[k]);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class IEnumerableExpansion
    {
        /// <summary>
        /// 過濾元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="fiter"></param>
        /// <param name="handleItem"></param>
        /// <param name="handleFiltItem"></param>
        /// <returns></returns>
        public static IEnumerable<T> FilterElements<T>(this IEnumerable<T> @this, Predicate<T> fiter, Action<T> handleItem = null, Action<T> handleFiltItem = null)
        {
            foreach (var e in @this)
            {
                if (fiter(e))
                {
                    yield return e;
                    handleFiltItem?.Invoke(e);
                }
                else
                    handleItem?.Invoke(e);
            }
        }

        /// <summary>
        /// 對集合做一次迭帶動做
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerable<T> Peek<T>(this IEnumerable<T> @this, Action<T> action)
        {
            if (@this.IsNull()) throw new ArgumentNullException(nameof(@this));
            if (action.IsNull()) throw new ArgumentNullException(nameof(action));

            return Iterator();

            IEnumerable<T> Iterator() // C# 7 Local Function
            {
                foreach (var item in @this)
                {
                    action(item);
                    yield return item;
                }
            }
        }
    }
}