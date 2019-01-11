using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace PurpleShine.Net.Http
{
    public class HttpOption
    {
        public HttpOption(string uri)
        {
            Uri = uri;
        }

        public static SecurityProtocolType SecurityProtocol
        {
            get => ServicePointManager.SecurityProtocol;
            set => ServicePointManager.SecurityProtocol = value;
        }

        public CookieContainer CookieContainer { get; set; } = new CookieContainer();

        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        public int Timeout { get; set; }

        public string Uri { get; private set; }

        public bool UseCookies { get; set; }

        public bool AllowAutoRedirect { get; set; }

        public bool UseDefaultCredentials { get; set; }

        public bool JsonUse { get; set; }

        public string CharSet { get; set; } = "UTF-8";

        public string Accept { get; set; } = "*/*";

        public void SetCookies(Dictionary<string, string> cookies)
        {
            foreach (var item in cookies)
            {
                SetCookie(item.Key, item.Value);
            }
        }

        public void SetCookie(string key, string value)
        {
            var u = new Uri(Uri);
            CookieContainer.Add(u, new Cookie(key, value, u.LocalPath, u.Host));   // ¥[¤JCookie
        }

        public void ResetCookie()
        {
            CookieContainer = new CookieContainer();
        }
    }

    public struct HttpResponse<T>
    {
        public HttpResponseMessage Response { get; set; }
        public bool IsSuccess { get; set; }
        public T Result { get; set; }
    }
}
