using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PurpleShine.Core.Libraries
{
    /// <summary>
    /// 時間鎖, 用來防止時間內某個物件被操作二次操作
    /// 搭配try finally 來釋放鎖
    /// </summary>
    public sealed class TimeLock : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly Dictionary<object, LockItem> _lockList = new Dictionary<object, LockItem>();
        private bool disposed = false;

        ~TimeLock()
        {
            Dispose(false);
        }

        #region Function
        /// <summary>
        /// 檢查該競爭對象是否正被鎖定
        /// </summary>
        /// <param name="lockObj"></param>
        /// <returns></returns>
        public bool IsLock(object lockObj)
        {
            try
            {
                _lock.EnterReadLock();
                if (_lockList.TryGetValue(lockObj, out LockItem _item)) return _item.InLockTime;
                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 釋放該競爭的鎖 最好要與 TryRelease 包在 try-finally裡面
        /// </summary>
        /// <param name="lockObj">競爭的物件對象</param>
        /// <returns>True if release success, false is fail maybe is released.</returns>
        public bool TryRelease(object lockObj)
        {
            if (disposed) throw new ObjectDisposedException("TimeLock disposed");

            try
            {
                _lock.EnterWriteLock();
                if (_lockList.TryGetValue(lockObj, out LockItem _item)) //&& _item.thread == Thread.CurrentThread
                {
                    return _lockList.Remove(lockObj);
                }
                return false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 取得該競爭對象的鎖 最好要與 TryRelease 包在 try-finally裡面
        /// </summary>
        /// <param name="lockObk">要競爭的物件對象</param>
        /// <param name="lockTime">需要用多久</param>
        /// <returns>True if get lock success</returns>
        public bool TryLock(object lockObk, int lockTime)
        {
            if (disposed) throw new ObjectDisposedException("TimeLock disposed");
            try
            {
                _lock.EnterUpgradeableReadLock();

                // 檢查當前清單
                try
                {
                    var delete = (from a in _lockList
                                  where !a.Value.InLockTime
                                  select a.Key).ToList();

                    if (delete.Any())
                    {
                        try
                        {
                            _lock.EnterWriteLock();
                            delete.ForEach(i => _lockList.Remove(i));
                        }
                        finally
                        {
                            _lock.ExitWriteLock();
                        }
                    }
                }
                catch (Exception) {}

                if (!_lockList.ContainsKey(lockObk))
                {
                    try
                    {
                        _lock.EnterWriteLock();
                        _lockList.Add(lockObk, new LockItem(lockTime, DateTime.Now));
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                    return true;
                }
                else
                    return false;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// 取得鎖定直到成功為止
        /// 會阻塞當前執行緒
        /// </summary>
        /// <param name="lockObk"></param>
        /// <param name="lockTime"></param>
        /// <param name="timeout">取得超時</param>
        public void Lock(object lockObk, int lockTime, int timeout = 0)
        {
            if (disposed) throw new ObjectDisposedException("TimeLock disposed");
            if (timeout > 0)
                SpinWait.SpinUntil(() => TryLock(lockObk, lockTime), timeout);
            else
                SpinWait.SpinUntil(() => TryLock(lockObk, lockTime));
        }

        /// <summary>
        /// 重置鎖定的物件開始時間(即重新計算時間)
        /// </summary>
        /// <param name="lockObk">要競爭的物件對象</param>
        /// <returns></returns>
        public bool ResetLockTime(object lockObk)
        {
            if (disposed) throw new ObjectDisposedException("TimeLock disposed");

            try
            {
                _lock.EnterReadLock();
                if (_lockList.TryGetValue(lockObk, out LockItem _item))
                {
                    _item.StartLockTime = DateTime.Now;
                    return true;
                }
                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        #endregion Function

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _lockList.Clear();
                    _lock.Dispose();
                }
                disposed = true;
            }
        }

        sealed class LockItem
        {
            public LockItem(int time, DateTime start)
            {
                Thread = Thread.CurrentThread;
                LockTime = time;
                StartLockTime = start;
            }

            internal Thread Thread { get; private set; }
            internal int LockTime { get; set; }
            internal DateTime StartLockTime { get; set; }
            internal bool InLockTime => LockTime == -1 || (DateTime.Now - StartLockTime).TotalMilliseconds <= LockTime;
        }
    }
}
