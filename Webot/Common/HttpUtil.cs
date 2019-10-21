﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web;

namespace Webot.Common
{
    public static class HttpUtil
    {
        public static string Get(string url, int? timeOutSeconds, IDictionary<string, string> paramDic = null,
            string authToken = null, Action<string> logFn = null)
        {
            Guid requestId = Guid.Empty;
            string paramStr = SerializeDictionary(paramDic);
            if (!String.IsNullOrEmpty(paramStr))
            {
                url += "?" + paramStr;
            }
            using (var client = GetHttpClient(timeOutSeconds))
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get,
                };

                if (!String.IsNullOrEmpty(authToken))
                {
                    request.Headers.Add("Authorization", authToken);
                }

                if (logFn != null)
                {
                    requestId = Guid.NewGuid();
                    logFn(String.Format("[{0}][Get] {1}", requestId, url));
                }

                var sendTask = client.SendAsync(request);
                sendTask.Wait();
                var response = sendTask.Result.EnsureSuccessStatusCode();
                var responseContent = response.Content.ReadAsStringAsync().Result;
                if (logFn != null)
                {
                    logFn(String.Format("[{0}][Response] {1}", requestId, responseContent));
                }
                return responseContent;
            }
        }

        public static string SerializeDictionary(IDictionary<string, string> dic)
        {
            string str = null;
            if (dic != null && dic.Count > 0)
            {
                var parameters = new List<string>();
                foreach (var item in dic)
                {
                    parameters.Add(item.Key + "=" + HttpUtility.UrlEncode(item.Value, Encoding.UTF8));
                }
                str = string.Join("&", parameters.ToArray());
            }
            return str;
        }

        private static HttpClient GetHttpClient(int? timeOutSeconds = null)
        {
            var client = new HttpClient();
            // 没有设置超时时间，则用默认超时时间
            client.Timeout = new TimeSpan(0, 0, timeOutSeconds.HasValue ? timeOutSeconds.Value : 30);
            return client;
        }
    }
}
