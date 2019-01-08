namespace FalseApple.Core.Data
{
    public class ProgressResult
    {
        public ProgressResult(string error, params object[] result)
        {
            Result = result;
            Error = error != null ? new ErrorException(error) : null;
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
