using StackFalse.Core.Expansion;

namespace StackFalse.Core.Data
{
    public class ProgressResult
    {
        public ProgressResult(string error, params object[] result)
        {
            Result = result;
            Error = error.IsNonNull() ? new ErrorException(error) : null;
        }

        /// <summary>
        /// 結果
        /// </summary>
        public object[] Result { get; private set; }

        /// <summary>
        /// 錯誤資訊
        /// </summary>
        public ErrorException Error { get; private set; }
    }
}
