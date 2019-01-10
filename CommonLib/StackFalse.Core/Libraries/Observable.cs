using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace StackFalse.Core.Libraries
{
    public struct Message
    {
        public string Text { get; set; }
        public object[] Datas { get; set; }

        private Message(string newText, object[] o)
        {
            Text = newText;
            Datas = o;
        }

        public static implicit operator Message(string text)
        {
            return new Message(text, null);
        }

        public static Message Build(string newText, params object[] datas)
        {
            return new Message(newText, datas);
        }
    }

    /// <summary>
    /// 監聽者模式
    /// </summary>
    public class Observable : IEnumerable<KeyValuePair<IObserver, string>>
    {
        private readonly ConcurrentDictionary<IObserver, string> _observers = new ConcurrentDictionary<IObserver, string>();
        private bool disposed = false;

        /// <summary>
        /// 新增監聽者
        /// </summary>
        /// <param name="observer">監聽者</param>
        /// <returns></returns>
        public bool Subscribe(IObserver observer)
        {
            string observerNum = null;
            try { observerNum = observer.ObserverNum(); }
            catch (NotImplementedException) { /* 未實作 */ }
            return _observers.TryAdd(observer, observerNum);
        }

        /// <summary>
        /// 若移除中呼叫OnCompleted發生例外時
        /// 必定回傳false, 但仍然會移除監聽
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        public bool UnSubscribe(IObserver observer)
        {
            if (_observers.TryRemove(observer, out string num))
            {
                try { observer.OnCompleted(); }
                catch (NotImplementedException) { /* 未實作 */ }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 發送訊息至監聽者
        /// </summary>
        /// <param name="loc">訊息</param>
        /// <param name="observerNum">若有指定條件, 則只會通知相關ObserverNum()返回結果之Observer</param>
        public void Notify(Message? message, Predicate<string> observerNum = null)
        {
            foreach (var observer in _observers)
            {
                try
                {
                    if (observerNum == null || observerNum(observer.Value))
                    {
                        if (!message.HasValue)
                            observer.Key.OnError(new MessageUnknownException());
                        else
                            observer.Key.OnNext(this, message.Value);
                    }
                }
                catch (NotImplementedException)
                {
                    // 防止observer.ObserverNum()沒實做會拋 NotImplementedException
                }
            }
        }

        /// <summary>
        /// 取消監聽者模式, 通知目前全部監聽對象
        /// </summary>
        public void EndTransmission()
        {
            foreach (var observer in _observers.Keys)
            {
                observer.OnCompleted();
            }
            _observers.Clear();
        }

        public IEnumerator<KeyValuePair<IObserver, string>> GetEnumerator()
        {
            foreach (var observer in _observers)
            {
                yield return observer;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    [Serializable]
    public class MessageUnknownException : Exception
    {
        internal MessageUnknownException()
        {
            //
        }
    }

    public interface IObserver
    {
        string ObserverNum();
        void OnCompleted();
        void OnError(Exception error);
        void OnNext(Observable headquarters, Message value);
    }
}