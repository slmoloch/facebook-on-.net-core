using System.Collections.Generic;
using System.Threading.Tasks;

namespace facebook_demo.Services
{
    public interface IFacebookClient
    {
        Task<T> GetPublicAsync<T>(string endpoint, string args = null);

        Task<T> GetAsync<T>(string accessToken, string endpoint, string args = null);

        Task<IDictionary<string, T>> BatchAsync<T>(string accessToken, string query, int seq);

        Task PostAsync(string accessToken, string endpoint, object data, string args = null);
    }
}