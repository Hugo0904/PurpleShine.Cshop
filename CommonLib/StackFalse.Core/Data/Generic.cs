using System.Collections;
using System.Collections.Generic;

namespace StackFalse.Core.Data
{
    public struct MultPair<TSource> : IEnumerable<TSource>
    {
        public MultPair(params TSource[] value)
        {
            Values = value.Length != 0 ? value : null;
        }

        public TSource[] Values { get; private set; }

        public IEnumerator<TSource> GetEnumerator()
        {
            foreach (var value in Values)
            {
                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
