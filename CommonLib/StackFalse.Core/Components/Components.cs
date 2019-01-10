using System;
using System.Collections.Generic;

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
            base.Add(item);
        }
        /// <summary>
        /// 將List第一個元素取出並刪除
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            if (base.Count > 0)
            {
                var t = base[0];
                base.RemoveAt(0);
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
            if (base.Count > index)
            {
                var t = base[index];
                base.RemoveAt(index);
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
            var value = base.Find(match);
            if (value != null)
                base.Remove(value);
            return value;
        }
        public List<T> FindAllDequeue(Predicate<T> match)
        {
            var value = base.FindAll(match);
            foreach (var single_value in value)
                if (value != null)
                    base.Remove(single_value);
            return value;
        }
    }
}
