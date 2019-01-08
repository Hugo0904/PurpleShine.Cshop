using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MegaMedia.Net.Http
{
    public static class HttpClientUtil
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
        public static HttpClient CreateClient(FactoryOption option)
        {
            HttpClientHandler clientHandler = new HttpClientHandler()
            {
                AllowAutoRedirect = option.AllowAutoRedirect,
                UseDefaultCredentials = option.UseDefaultCredentials,
            };

            if (option.Cookies != null)
            {
                CookieContainer cookieContainer = new CookieContainer();
                foreach (var item in option.Cookies)
                {
                    cookieContainer.Add(new Cookie(item.Key, item.Value));   // 加入Cookie
                }
                clientHandler.CookieContainer = cookieContainer;
                clientHandler.UseCookies = true;
            }

            HttpClient http = new HttpClient(new CustomHandler("主處理器")
            {
                InnerHandler = new CustomHandler("輔助處理器")
                {
                    InnerHandler = clientHandler
                }
            })
            {
                BaseAddress = new Uri(option.Uri)
            };

            if (option.Headers != null)
            {
                foreach (var item in option.Headers)
                {
                    http.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }

            if (option.JsonUse)
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (option.Timeout > 0)
                http.Timeout = new TimeSpan(option.Timeout);

            return http;
        }

        public static void DisposeClient(HttpClient http)
        {
            http?.Dispose();
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> SendGetAsync(this HttpClient client, string path, int timeout = Timeout.Infinite)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                using (HttpResponseMessage response = await client.GetAsync(path, cts.Token))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (var responseContent = response.Content)
                        {
                            return await responseContent.ReadAsStringAsync();
                        }
                    }
                    throw new Exception($"Response is fail status code " + response.StatusCode);
                }
            }
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="path"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<T> SendGetAsync<T>(this HttpClient client, string path, int timeout = Timeout.Infinite) where T : class
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));
          
                using (HttpResponseMessage response = await client.GetAsync(path, cts.Token))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (var responseContent = response.Content)
                        {
                            return await responseContent.ReadAsAsync<T>();
                        }
                    }
                    throw new Exception($"Response is fail status code " + response.StatusCode);
                }
            }
        }

        /// <summary>
        /// Post
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> SendPostAsync(this HttpClient client, string url, Dictionary<string, string> data, int timeout = Timeout.Infinite, Action<HttpContent> OnContent = null)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                HttpContent postContent = new FormUrlEncodedContent(data);
                OnContent?.Invoke(postContent);

                using (HttpResponseMessage response = await client.PostAsync(url, postContent, cts.Token))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (var responseContent = response.Content)
                        {
                            return await responseContent.ReadAsStringAsync();
                        }
                    }
                    throw new Exception($"Response is fail status code " + response.StatusCode);
                }
            }
        }

        /// <summary>
        /// Post Json
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> SendPostAsJsonAsync(this HttpClient client, string url, object data, int timeout = Timeout.Infinite)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                using (HttpResponseMessage response = await client.PostAsJsonAsync(url, data, cts.Token))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (var responseContent = response.Content)
                        {
                            return await responseContent.ReadAsStringAsync();
                        }
                    }
                    throw new Exception($"Response is fail status code " + response.StatusCode);
                }
            }
        }

        /// <summary>
        /// Put
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> SendPutAsync(this HttpClient client, string url, Dictionary<string, string> data, int timeout = Timeout.Infinite, Action<HttpContent> OnContent = null)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                HttpContent putContent = new FormUrlEncodedContent(data);
                OnContent?.Invoke(putContent);

                using (HttpResponseMessage response = await client.PutAsync(url, putContent, cts.Token))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (var responseContent = response.Content)
                        {
                            return await responseContent.ReadAsStringAsync();
                        }
                    }
                    throw new Exception($"Response is fail status code " + response.StatusCode);
                }
            }
        }

        /// <summary>
        /// Put Json
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> SendPutAsJsonAsync(this HttpClient client, string url, object data, int timeout = Timeout.Infinite)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                using (HttpResponseMessage response = await client.PutAsJsonAsync(url, data, cts.Token))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (var responseContent = response.Content)
                        {
                            return await responseContent.ReadAsStringAsync();
                        }
                    }
                    throw new Exception($"Response is fail status code " + response.StatusCode);
                }
            }
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> SendDeleteAsync(this HttpClient client, string path, int timeout = Timeout.Infinite)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                using (HttpResponseMessage response = await client.DeleteAsync(path, cts.Token))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (var responseContent = response.Content)
                        {
                            return await responseContent.ReadAsStringAsync();
                        }
                    }
                    throw new Exception($"Response is fail status code " + response.StatusCode);
                }
            }
        }

        /// <summary>
        /// 將Collection建立Get請求用格式
        /// </summary>
        /// <param name="parameters">容器</param>
        /// <returns></returns>
        public static string BuildGetQuery(Dictionary<string, object> parameters)
        {
            var result = from a in parameters
                         select $"{a.Key}={a.Value.ToString()}";
            return "?" + string.Join("&", result);
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
