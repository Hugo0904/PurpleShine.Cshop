using System;
using System.Threading.Tasks;
using PurpleShine.Core.Delegates.Args;

namespace PurpleShine.Core.Models.Interfaces
{
    /// <summary>
    /// 流程確認介面
    /// </summary>
    public interface IProgressHandler
    {
        event EventHandler<EventResultArgs> OnProgressUpdated;

        /// <summary>
        /// 更新當前進展, 藉由OnProgressChecked事件觸發回傳訊息
        /// </summary>
        Task UpdateProgressAsync();
    }
}
