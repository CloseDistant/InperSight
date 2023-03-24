using InperStudio.Lib.Data.Monitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace InperStudio.Lib.Helper
{
    public class HttpClientHelper
    {
        private static readonly string baseurl = "http://cat.inper.com:4646/";
        //private static readonly string baseurl = "https://localhost:44307/";
        public static ApiResult Get(string url, string token = null)
        {
            var res = new ApiResult();
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(baseurl);
                    //client.Timeout = TimeSpan.FromSeconds(10);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    if (token != null)
                    {
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer " + token);
                    }
                    HttpResponseMessage response = client.GetAsync("api/" + url).Result;
                    string json = response.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(json))
                    {
                        res.Message = response.StatusCode.ToString();
                        if (res.Message.Equals("Unauthorized"))
                        {
                            res.Code = 401;
                        }
                        return res;
                    }
                    var apir = JsonConvert.DeserializeObject<ApiResult>(json);
                    apir.Success = apir.Code == 200 ? true : false;
                    return apir;
                }
                catch (Exception ex)
                {
                    App.Log.Error(ex.ToString());
                }
            }
            return res;
        }
        /// <summary>
        /// Post （带参）
        /// </summary>
        /// <param name="url">路径</param>
        /// <param name="content">参数</param>
        /// <returns></returns>
        public static ApiResult Post(string url, Dictionary<string, object> content, string token = null)
        {
            return Post(url, JsonConvert.SerializeObject(content), token);
        }
        public static ApiResult Post(string url, string jsonContent, string token = null)
        {
            var res = new ApiResult();
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(baseurl);
                    client.Timeout = TimeSpan.FromSeconds(10);
                    client.DefaultRequestHeaders.ExpectContinue = false;
                    if (token != null)
                    {
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer " + token);
                    }
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = client.PostAsync("api/" + url, new StringContent(jsonContent,
                                        Encoding.UTF8,
                                        "application/json")).Result;
                    string json = response.Content.ReadAsStringAsync().Result;

                    if (string.IsNullOrEmpty(json))
                    {
                        res.Message = response.StatusCode.ToString();
                        if (res.Message.Equals("Unauthorized"))
                        {
                            res.Code = 401;
                        }
                        return res;
                    }

                    return JsonConvert.DeserializeObject<ApiResult>(json);
                }
                catch (Exception ex)
                {
                    res.Message = ex.ToString();
                    App.Log.Error(ex.ToString());
                }
            }
            return res ?? new ApiResult();
        }
    }
}
