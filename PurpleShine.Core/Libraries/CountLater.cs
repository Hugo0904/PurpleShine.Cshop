using System;
using System.Threading;
using System.Threading.Tasks;
using PurpleShine.Core.Models.Interfaces;

namespace PurpleShine.Core.Libraries
{
    /// <summary>
    /// 記數器
    /// </summary>
    public sealed class CountLater : IResetHandler
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private long _max;
        private long _min;
        private double _avg;
        private long _total;
        private long _last;

        /// <summary>
        /// 最大值
        /// </summary>
        public long MaxValue
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _max;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// 最小值
        /// </summary>
        public long MinValue
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _min;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// 平均值
        /// </summary>
        public long AvgValue
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return Convert.ToInt64(_avg);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// 總值
        /// </summary>
        public long TotalValue
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _total;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// 最後一次輸入的值
        /// </summary>
        public long LastValue
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _last;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        private void Update(long value)
        {
            if (value > _max)
                _max = value;

            if (value < _min)
                _min = value;

            _last = value;

            try
            {
                _avg = checked(((_avg * checked(_total++)) + value) / _total);
            }
            catch (Exception)
            {
                _avg = 0;
                _total = 0;
                Update(value);
            }
        }

        /// <summary>
        /// 設置一個值至計數器
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(long value)
        {
            _lock.EnterWriteLock();
            try
            {
                Update(value);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 重置計數器
        /// </summary>
        /// <param name="parameters"></param>
        public void Reset(params object[] parameters)
        {
            _lock.EnterWriteLock();
            try
            {
                _max = 0;
                _min = 0;
                _avg = 0;
                _total = 0;
                _last = 0;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 重置計數器
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Task ResetAsync(object[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
