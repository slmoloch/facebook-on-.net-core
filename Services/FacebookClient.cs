using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace facebook_demo.Services
{
    public class FacebookClient : IFacebookClient
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;

        public FacebookClient()
        {
            baseUrl = "https://graph.facebook.com/v2.8/";

            httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };

            httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public string GetLoginUrl(string appId, string redirectUrl, IList<string> permissions)
        {
            var scope = string.Join(",", permissions);

            return $"https://www.facebook.com/v2.8/dialog/oauth?client_id={appId}&redirect_uri={redirectUrl}&scope={scope}&response_type=code";
        }

        public async Task<T> GetPublicAsync<T>(string endpoint, string args = null)
        {
            var response = await httpClient.GetAsync($"{endpoint}?{args}");

            if (!response.IsSuccessStatusCode)
                return default(T);

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }

        public async Task<T> GetAsync<T>(string accessToken, string endpoint, string args = null)
        {
            var response = await httpClient.GetAsync($"{endpoint}?access_token={accessToken}&{args}");

            return await ProcessResponse<T>(response);
        }

        private static async Task<T> ProcessResponse<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                return default(T);

            var result = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(result))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(result);
        }

        public async Task<T> PostAsync<T>(string accessToken, string endpoint, object data, string args = null)
        {
            var payload = GetPayload(data);

            var response = await httpClient.PostAsync($"{endpoint}?access_token={accessToken}&{args}", payload);

            return await ProcessResponse<T>(response);
        }

        public async Task<IDictionary<string, T>> BatchAsync<T>(string accessToken, string query, int seq)
        {
            var paylod = GetPayload(new { batch = query });

            var response = await httpClient.PostAsync($"/?access_token={accessToken}", paylod);

            if (!response.IsSuccessStatusCode)
                return new Dictionary<string, T>();

            var result = await response.Content.ReadAsStringAsync();

            var wrapper = JsonConvert.DeserializeObject<dynamic>(result);
            var content = wrapper[seq].body;

            var r = JsonConvert.DeserializeObject(content);

            var dict = new Dictionary<string, T>();

            foreach (var x in r)
            {
                string name = x.Key;
                JToken value = x.Value;

                dict.Add(name, value.ToObject<dynamic>());
            }

            return dict;
        }

        private static StringContent GetPayload(object data)
        {
            var json = JsonConvert.SerializeObject(data);

            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
