using System.Threading.Tasks;


namespace PurpleShine.Core.Data.Interfaces
{
    /// <summary>
    /// 重置界面
    /// </summary>
    public interface IResetHandler
    {
        /// <summary>
        /// 重置
        /// </summary>
        void Reset(params object[] parameters);

        /// <summary>
        ///異步重置
        /// </summary>
        Task ResetAsync(params object[] parameters);
    }
}
