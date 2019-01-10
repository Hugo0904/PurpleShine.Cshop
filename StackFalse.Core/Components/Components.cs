using System;
using System.Collections.Generic;
using StackFalse.Core.Expansion;

namespace StackFalse.Core.Components
{
    public class ClassQueue<T> : List<T>
    {
        /// <summary>
        /// 將元素新增至List最尾端
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            Add(item);
        }
        /// <summary>
        /// 將List第一個元素取出並刪除
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            if (Count > 0)
            {
                var t = base[0];
                RemoveAt(0);
                return t;
            }
            else
                return default(T);
        }
        /// <summary>
        /// 將List第一個元素取出並刪除
        /// </summary>
        /// <returns></returns>
        public T Dequeue(int index)
        {
            if (Count > index)
            {
                var t = base[index];
                RemoveAt(index);
                return t;
            }
            else
                return default(T);
        }
        /// <summary>
        /// 將List的第一個元素取出不刪除
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            return base[0];
        }
        public T FindDequeue(Predicate<T> match)
        {
            var value = Find(match);
            if (value.IsNonNull())
                Remove(value);
            return value;
        }
        public List<T> FindAllDequeue(Predicate<T> match)
        {
            var value = FindAll(match);
            foreach (var single_value in value)
            {
                if (value.IsNonNull())
                {
                    Remove(single_value);
                }
            }
            return value;
        }
    }
}
