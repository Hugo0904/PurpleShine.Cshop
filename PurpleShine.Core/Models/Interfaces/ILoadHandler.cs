
using System.Threading.Tasks;

namespace PurpleShine.Core.Models.Interfaces
{
    /// <summary>
    /// 初始化界面
    /// </summary>
    public interface ILoadHandler
    {
        /// <summary>
        /// 初始化前準備
        /// </summary>
        /// <returns></returns>
        bool PrepareInit(params object[] parameters);

        /// <summary>
        /// 異步初始化前準備
        /// </summary>
        /// <returns></returns>
        Task<bool> PrepareInitAsync(params object[] parameters);

        /// <summary>
        /// 初始化實體
        /// </summary>
        void Initialize(params object[] parameters);

        /// <summary>
        /// 異步初始化實體
        /// </summary>
        Task InitializeAsync(params object[] parameters);
    }
}
