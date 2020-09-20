using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PurpleShine.Net.Http
{
    /// <summary>
    /// 
    /// </summary>
    public static class HttpExpansion
    {
        /// <summary>
        /// Get
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path"></param>
        /// <param name="timeout"></param>
        /// <param name="OnResponse"></param>
        /// <returns></returns>
        public static async Task<HttpResponse<string>> SendGetAsync(this HttpClient client, string path, int timeout = Timeout.Infinite, Action<HttpResponseMessage> OnResponse = null)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                using (HttpResponseMessage response = await client.GetAsync(path, cts.Token))
                {
                    OnResponse?.Invoke(response);

                    var httpResponse = new HttpResponse<string>
                    {
                        Response = response,
                        IsSuccess = response.IsSuccessStatusCode
                    };

                    if (response.Content != null)
                    {
                        using (var responseContent = response.Content)
                        {
                            httpResponse.Result = await responseContent.ReadAsStringAsync();
                        }
                    }

                    return httpResponse;
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
        /// <param name="OnResponse"></param>
        /// <returns></returns>
        public static async Task<HttpResponse<T>> SendGetAsync<T>(this HttpClient client, string path, int timeout = Timeout.Infinite, Action<HttpResponseMessage> OnResponse = null) where T : class
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                using (HttpResponseMessage response = await client.GetAsync(path, cts.Token))
                {
                    OnResponse?.Invoke(response);

                    var httpResponse = new HttpResponse<T>
                    {
                        Response = response,
                        IsSuccess = response.IsSuccessStatusCode
                    };

                    if (httpResponse.IsSuccess)
                    {
                        using (var responseContent = response.Content)
                        {
                            httpResponse.Result = await responseContent.ReadAsAsync<T>();
                        }
                    }

                    return httpResponse;
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
        /// <param name="OnRequestContent"></param>
        /// <param name="OnResponse"></param>
        /// <returns></returns>
        public static async Task<HttpResponse<string>> SendPostAsync(this HttpClient client, string url, Dictionary<string, string> data, int timeout = Timeout.Infinite, Action<HttpContent> OnRequestContent = null, Action<HttpResponseMessage> OnResponse = null)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                HttpContent postContent = new FormUrlEncodedContent(data);
                OnRequestContent?.Invoke(postContent);

                using (HttpResponseMessage response = await client.PostAsync(url, postContent, cts.Token))
                {
                    OnResponse?.Invoke(response);

                    var httpResponse = new HttpResponse<string>
                    {
                        Response = response,
                        IsSuccess = response.IsSuccessStatusCode
                    };

                    if (response.Content != null)
                    {
                        using (var responseContent = response.Content)
                        {
                            httpResponse.Result = await responseContent.ReadAsStringAsync();
                        }
                    }

                    return httpResponse;
                }
            }
        }

        /// <summary>
        /// Post
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <param name="timeout"></param>
        /// <param name="OnRequestContent"></param>
        /// <param name="OnResponse"></param>
        /// <returns></returns>
        public static async Task<HttpResponse<string>> SendPostAsync(this HttpClient client, string url, HttpContent content, int timeout = Timeout.Infinite, Action<HttpContent> OnRequestContent = null, Action<HttpResponseMessage> OnResponse = null)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                OnRequestContent?.Invoke(content);

                using (HttpResponseMessage response = await client.PostAsync(url, content, cts.Token))
                {
                    OnResponse?.Invoke(response);

                    var httpResponse = new HttpResponse<string>
                    {
                        Response = response,
                        IsSuccess = response.IsSuccessStatusCode
                    };

                    if (response.Content != null)
                    {
                        ;
                        using (var responseContent = response.Content)
                        {
                            httpResponse.Result = await responseContent.ReadAsStringAsync();
                        }
                    }

                    return httpResponse;
                }
            }
        }

        /// <summary>
        /// Post
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <param name="timeout"></param>
        /// <param name="OnRequestContent"></param>
        /// <param name="OnResponse"></param>
        /// <returns></returns>
        public static async Task<HttpResponse<T>> SendPostAsync<T>(this HttpClient client, string url, HttpContent content, int timeout = Timeout.Infinite, Action<HttpContent> OnRequestContent = null, Action<HttpResponseMessage> OnResponse = null)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                OnRequestContent?.Invoke(content);

                using (HttpResponseMessage response = await client.PostAsync(url, content, cts.Token))
                {
                    OnResponse?.Invoke(response);

                    var httpResponse = new HttpResponse<T>
                    {
                        Response = response,
                        IsSuccess = response.IsSuccessStatusCode
                    };

                    if (httpResponse.IsSuccess)
                    {
                        using (var responseContent = response.Content)
                        {
                            httpResponse.Result = await responseContent.ReadAsAsync<T>();
                        }
                    }

                    return httpResponse;
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
        /// <param name="OnRequestContent"></param>
        /// <param name="OnResponse"></param>
        /// <returns></returns>
        public static async Task<HttpResponse<T>> SendPostAsync<T>(this HttpClient client, string url, Dictionary<string, string> data, int timeout = Timeout.Infinite, Action<HttpContent> OnRequestContent = null, Action<HttpResponseMessage> OnResponse = null)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                HttpContent postContent = new FormUrlEncodedContent(data);
                OnRequestContent?.Invoke(postContent);

                using (HttpResponseMessage response = await client.PostAsync(url, postContent, cts.Token))
                {
                    OnResponse?.Invoke(response);

                    var httpResponse = new HttpResponse<T>
                    {
                        Response = response,
                        IsSuccess = response.IsSuccessStatusCode
                    };

                    if (httpResponse.IsSuccess)
                    {
                        using (var responseContent = response.Content)
                        {
                            httpResponse.Result = await responseContent.ReadAsAsync<T>();
                        }
                    }

                    return httpResponse;
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
        /// <param name="OnResponse"></param>
        /// <returns></returns>
        public static async Task<HttpResponse<string>> SendPostAsJsonAsync(this HttpClient client, string url, object data, int timeout = Timeout.Infinite, Action<HttpResponseMessage> OnResponse = null)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                using (HttpResponseMessage response = await client.PostAsJsonAsync(url, data, cts.Token))
                {
                    OnResponse?.Invoke(response);

                    var httpResponse = new HttpResponse<string>
                    {
                        Response = response,
                        IsSuccess = response.IsSuccessStatusCode
                    };

                    if (response.Content != null)
                    {
                        using (var responseContent = response.Content)
                        {
                            httpResponse.Result = await responseContent.ReadAsStringAsync();
                        }
                    }

                    return httpResponse;
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
        /// <param name="OnResponse"></param>
        /// <returns></returns>
        public static async Task<HttpResponse<T>> SendPostAsJsonAsync<T>(this HttpClient client, string url, object data, int timeout = Timeout.Infinite, Action<HttpResponseMessage> OnResponse = null)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                using (HttpResponseMessage response = await client.PostAsJsonAsync(url, data, cts.Token))
                {
                    OnResponse?.Invoke(response);

                    var httpResponse = new HttpResponse<T>
                    {
                        Response = response,
                        IsSuccess = response.IsSuccessStatusCode
                    };

                    if (httpResponse.IsSuccess)
                    {
                        using (var responseContent = response.Content)
                        {
                            httpResponse.Result = await responseContent.ReadAsAsync<T>();
                        }
                    }

                    return httpResponse;
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
        /// <param name="OnContent"></param>
        /// <param name="OnResponse"></param>
        /// <returns></returns>
        public static async Task<HttpResponse<string>> SendPutAsync(this HttpClient client, string url, Dictionary<string, string> data, int timeout = Timeout.Infinite, Action<HttpContent> OnContent = null, Action<HttpResponseMessage> OnResponse = null)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                HttpContent putContent = new FormUrlEncodedContent(data);
                OnContent?.Invoke(putContent);

                using (HttpResponseMessage response = await client.PutAsync(url, putContent, cts.Token))
                {
                    OnResponse?.Invoke(response);

                    var httpResponse = new HttpResponse<string>
                    {
                        Response = response,
                        IsSuccess = response.IsSuccessStatusCode
                    };

                    if (response.Content != null)
                    {
                        using (var responseContent = response.Content)
                        {
                            httpResponse.Result = await responseContent.ReadAsStringAsync();
                        }
                    }

                    return httpResponse;
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
        /// <param name="OnContent"></param>
        /// <param name="OnResponse"></param>
        /// <returns></returns>
        public static async Task<HttpResponse<T>> SendPutAsync<T>(this HttpClient client, string url, Dictionary<string, string> data, int timeout = Timeout.Infinite, Action<HttpContent> OnContent = null, Action<HttpResponseMessage> OnResponse = null)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                HttpContent putContent = new FormUrlEncodedContent(data);
                OnContent?.Invoke(putContent);

                using (HttpResponseMessage response = await client.PutAsync(url, putContent, cts.Token))
                {
                    OnResponse?.Invoke(response);

                    var httpResponse = new HttpResponse<T>
                    {
                        Response = response,
                        IsSuccess = response.IsSuccessStatusCode
                    };

                    if (httpResponse.IsSuccess)
                    {
                        using (var responseContent = response.Content)
                        {
                            httpResponse.Result = await responseContent.ReadAsAsync<T>();
                        }
                    }

                    return httpResponse;
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
        /// <param name="OnResponse"></param>
        /// <returns></returns>
        public static async Task<HttpResponse<string>> SendPutAsJsonAsync(this HttpClient client, string url, object data, int timeout = Timeout.Infinite, Action<HttpResponseMessage> OnResponse = null)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                using (HttpResponseMessage response = await client.PutAsJsonAsync(url, data, cts.Token))
                {
                    OnResponse?.Invoke(response);

                    var httpResponse = new HttpResponse<string>
                    {
                        Response = response,
                        IsSuccess = response.IsSuccessStatusCode
                    };

                    if (response.Content != null)
                    {
                        using (var responseContent = response.Content)
                        {
                            httpResponse.Result = await responseContent.ReadAsStringAsync();
                        }
                    }

                    return httpResponse;
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
        /// <param name="OnResponse"></param>
        /// <returns></returns>
        public static async Task<HttpResponse<T>> SendPutAsJsonAsync<T>(this HttpClient client, string url, object data, int timeout = Timeout.Infinite, Action<HttpResponseMessage> OnResponse = null)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                using (HttpResponseMessage response = await client.PutAsJsonAsync(url, data, cts.Token))
                {
                    OnResponse?.Invoke(response);

                    var httpResponse = new HttpResponse<T>
                    {
                        Response = response,
                        IsSuccess = response.IsSuccessStatusCode
                    };

                    if (httpResponse.IsSuccess)
                    {
                        using (var responseContent = response.Content)
                        {
                            httpResponse.Result = await responseContent.ReadAsAsync<T>();
                        }
                    }

                    return httpResponse;
                }
            }
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path"></param>
        /// <param name="timeout"></param>
        /// <param name="OnResponse"></param>
        /// <returns></returns>
        public static async Task<HttpResponse<string>> SendDeleteAsync(this HttpClient client, string path, int timeout = Timeout.Infinite, Action<HttpResponseMessage> OnResponse = null)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                using (HttpResponseMessage response = await client.DeleteAsync(path, cts.Token))
                {
                    OnResponse?.Invoke(response);

                    var httpResponse = new HttpResponse<string>
                    {
                        Response = response,
                        IsSuccess = response.IsSuccessStatusCode
                    };

                    if (response.Content != null)
                    {
                        using (var responseContent = response.Content)
                        {
                            httpResponse.Result = await responseContent.ReadAsStringAsync();
                        }
                    }

                    return httpResponse;
                }
            }
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="client"></param>
        /// <param name="path"></param>
        /// <param name="timeout"></param>
        /// <param name="OnResponse"></param>
        /// <returns></returns>
        public static async Task<HttpResponse<T>> SendDeleteAsync<T>(this HttpClient client, string path, int timeout = Timeout.Infinite, Action<HttpResponseMessage> OnResponse = null)
        {
            using (var cts = new CancellationTokenSource())
            {
                if (timeout != Timeout.Infinite)
                    cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                using (HttpResponseMessage response = await client.DeleteAsync(path, cts.Token))
                {
                    OnResponse?.Invoke(response);

                    var httpResponse = new HttpResponse<T>
                    {
                        Response = response,
                        IsSuccess = response.IsSuccessStatusCode
                    };

                    if (httpResponse.IsSuccess)
                    {
                        using (var responseContent = response.Content)
                        {
                            httpResponse.Result = await responseContent.ReadAsAsync<T>();
                        }
                    }

                    return httpResponse;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ConvertGetQueryToDictionary(this string @this)
        {
            if (@this.StartsWith("&"))
                @this = @this.Substring(1);
            var collection = HttpUtility.ParseQueryString(@this);
            return collection.AllKeys.ToDictionary(k => k, v => collection[v]);
        }
    }
}
