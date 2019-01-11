using System;
using PurpleShine.Core.Expansions;

namespace PurpleShine.Core.Data
{
    /// <summary>
    /// 成功失敗結果事件組
    /// </summary>
    public class EventResultArgs : EventArgs
    {
        public static new readonly EventResultArgs Empty = new EventResultArgs(true);

        public EventResultArgs(bool _suc, string err = null, object data = null)
        {
            Success = _suc;
            Data = data;
            if (err.IsNonNull())
                Error = new ErrorException(err);
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// 資料
        /// </summary>
        public object Data { get; private set; }

        /// <summary>
        /// 錯誤原因
        /// </summary>
        public ErrorException Error { get; private set; }

        public static implicit operator EventResultArgs(bool state)
        {
            return new EventResultArgs(state, null);
        }

        public static implicit operator EventResultArgs(string error)
        {
            return new EventResultArgs(false, error);
        }
    }
}
