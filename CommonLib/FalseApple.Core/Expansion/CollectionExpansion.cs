using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FalseApple.Core.Expansion
{
    public static class IListExpansion
    {
        public static IList<T> ListAdd<T>(this IList<T> @this, T item)
        {
            @this.Add(item);
            return @this;
        }
    }

    public static class IEnumerableExpansion
    {
        /// <summary>
        /// 過濾元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itor"></param>
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
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerable<T> Peek<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            return Iterator();

            IEnumerable<T> Iterator() // C# 7 Local Function
            {
                foreach (var item in source)
                {
                    action(item);
                    yield return item;
                }
            }
        }
    }
}