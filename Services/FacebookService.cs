using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace facebook_demo.Services
{
    public class FacebookService : IFacebookService
    {
        private readonly IFacebookClient facebookClient;
        private readonly IConfiguration configuration;

        public FacebookService(IFacebookClient facebookClient, IConfiguration configuration)
        {
            this.facebookClient = facebookClient;
            this.configuration = configuration;
        }

        public async Task<string> GetToken(string code, string redirectUrl)
        {
            var appId = configuration["Authentication:Facebook:AppId"];
            var appSecret = configuration["Authentication:Facebook:AppSecret"];

            var result = await facebookClient.GetPublicAsync<dynamic>(
                "oauth/access_token", 
                $"client_id={appId}&redirect_uri={redirectUrl}&client_secret={appSecret}&code={code}");

            if (result == null)
            {
                return null;
            }

            return result.access_token;
        }

        public async Task<AccountDto> GetAccountAsync(string accessToken)
        {
            var result = await facebookClient.GetAsync<dynamic>(
                accessToken, "me", "fields=id,name,email,first_name,last_name,age_range,birthday,gender,locale");

            if (result == null)
            {
                return new AccountDto();
            }

            var account = new AccountDto
            {
                Id = result.id,
                Email = result.email,
                Name = result.name,
                UserName = result.username,
                FirstName = result.first_name,
                LastName = result.last_name,
                Locale = result.locale
            };

            return account;
        }

        public async Task<List<PostDto>> GetPagePostsAsync(string accessToken, string pageId)
        {
            var result = await facebookClient.GetAsync<dynamic>(accessToken, pageId + "/promotable_posts", "fields=id,message,is_published");

            if (result == null)
            {
                return new List<PostDto>();
            }

            var posts = new List<PostDto>();

            foreach (var e in result.data)
            {
                var post = new PostDto
                {
                    Id = e.id,
                    Message = e.message,
                    Published = e.is_published
                };

                posts.Add(post);
            }

            return posts;
        }

        public async Task<string> GetPageToken(string accessToken, string pageId)
        {
            var result = await facebookClient.GetAsync<dynamic>(accessToken, pageId, "fields=access_token");

            if (result == null)
            {
                return null;
            }

            return result.access_token;
        }

        public async Task<IList<PageMetricDto>> GetPostImpressions(string accessToken, string pageId)
        {
            var query = @"[{ ""method"":""GET"",""name"":""get-posts"",""relative_url"":""" + pageId + @"/posts"",},{ ""method"":""GET"", ""relative_url"":""/insights/post_impressions_unique/lifetime?ids={result=get-posts:$.data.*.id}""}]";

            var result = await facebookClient.BatchAsync<dynamic>(accessToken, query, 1);

            IList<PageMetricDto> r = new List<PageMetricDto>();

            foreach (var o in result)
            {
                r.Add(new PageMetricDto()
                {
                    Id = o.Key,
                    Value = o.Value.data.values[0].value
                });
            }

            return r;
        }

        public async Task PostOnPage(string accessToken, string pageId, string message, bool published, int publishOn)
            => await facebookClient.PostAsync(accessToken, $"{pageId}/feed", new { message, published, scheduled_publish_time = publishOn });

        public async Task PublishPostOnPage(string accessToken, string postId)
            => await facebookClient.PostAsync(accessToken, $"{postId}", new { is_published = true });
    }
}