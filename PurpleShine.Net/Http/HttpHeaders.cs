using System.Collections.Generic;

namespace PurpleShine.Net.Http
{
    public static class HttpHeaders
    {
        public static Dictionary<string, string> DEFAULT_HEADER = new Dictionary<string, string>()
        {
            { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8" },
            { "Accept-Encoding", "gzip, deflate, sdch" },
            { "Accept-Language", "zh-TW,zh;q=0.8,en-US;q=0.6,en;q=0.4" },
            { "Connection", "keep-alive" },
            { "KeepAlive", "false" },
            { "User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36" }
        };
    }
}
