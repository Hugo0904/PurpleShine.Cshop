using System;

namespace PurpleShine.Core.Expansions
{
    public static class RandomExpansion
    {
        /// <summary>
        /// 隨機 大寫A-Z 或 0-9
        /// type null = 返回隨機大小寫字母加數字 (預設)
        /// type num = 返回隨機數字
        /// type char = 返回隨機大小寫字母
        /// type upperNum = 返回隨機大寫字母加數字
        /// type lowerNum = 返回隨機小寫字母加數字
        /// </summary>
        /// <param name="this"></param>
        /// <param name="types">模式</param>
        /// <returns></returns>
        public static string GetRandomChar(this Random @this, string types)
        {
            // 0-9
            // A-Z  ASCII值  65-90
            // a-z  ASCII值  97-122
            int i;
            switch (types)
            {
                case "num": // 返回隨機數字
                    i = @this.Next(0, 10);
                    break;

                case "char": // 返回隨機大小寫字母
                    do { i = @this.Next(65, 123); } while (i > 90 && i < 97);
                    break;

                case "upperNum": // 返回隨機大寫字母加數字
                    do { i = @this.Next(0, 91); } while ((i > 9 && i < 65));
                    break;

                case "lowerNum": // 返回隨機小寫字母加數字
                    do { i = @this.Next(0, 123); } while (i > 9 && i < 97);
                    break;

                default: // 返回隨機大小寫字母加數字 (預設)
                    do { i = @this.Next(0, 123); } while (i > 9 && i < 65);
                    break;
            }
            return i < 10 ? i.ToString() : ((char)i).ToString();
        }
    }
}
