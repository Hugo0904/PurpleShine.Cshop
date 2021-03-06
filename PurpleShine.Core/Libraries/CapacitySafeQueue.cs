﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Libraries
{
    public class CapacitySafeQueue<T> : ConcurrentQueue<T>
    {
        private int _maxCapacity = 0;
        public CapacitySafeQueue(int maxCapacity)
        {
            _maxCapacity = maxCapacity;
        }

        /// <summary>
        /// 嘗試將元素加入queue裡面
        /// </summary>
        /// <param name="t">元素</param>
        /// <param name="major">是否重要元素? 重要元素無視上限加入</param>
        /// <returns>false 表示queue已達上限</returns>
        public bool TryEnqueue(T t, bool major = false)
        {
            if (!major && base.Count >= _maxCapacity) return false;
            base.Enqueue(t);
            return true;
        }
    }
}
