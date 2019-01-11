using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace PurpleShine.Net.Http
{
    public static class HttpClientPool
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

        /// <summary>
        /// Create HttpClient
        /// </summary>
        /// <returns></returns>
        public static HttpClient CreateClient(HttpOption option)
        {
            HttpClientHandler clientHandler = new HttpClientHandler()
            {
                AllowAutoRedirect = option.AllowAutoRedirect,
                UseDefaultCredentials = option.UseDefaultCredentials,
            };

            if (option.UseCookies)
            {
                if (option.CookieContainer == null)
                {
                    option.CookieContainer = new CookieContainer();
                }

                clientHandler.CookieContainer = option.CookieContainer;
                clientHandler.UseCookies = true;
            }

            HttpClient http = new HttpClient(new CustomHandler("主處理器")
            {
                InnerHandler = new CustomHandler("輔助處理器")
                {
                    InnerHandler = clientHandler
                }
            });

            if (option.Uri != null)
            {
                http.BaseAddress = new Uri(option.Uri);
            }

            if (option.Headers != null)
            {
                foreach (var item in option.Headers)
                {
                    http.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }

            string mediaType = option.JsonUse ? "application/json" : option.Accept;
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType)
            {
                CharSet = option.CharSet
            });

            if (option.Timeout > 0)
                http.Timeout = new TimeSpan(option.Timeout);

            return http;
        }

        public static void DisposeClient(HttpClient http)
        {
            http?.Dispose();
        }
    }

    public class CustomHandler : DelegatingHandler
    {
        private string _name;

        public CustomHandler(string name)
        {
            _name = name;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //Console.WriteLine("Request path in handler {0}", _name); 
            return base.SendAsync(request, cancellationToken).ContinueWith(requestTask =>
            {
                //Console.WriteLine("Response path in handler {0}", _name);
                return requestTask.Result;
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}
