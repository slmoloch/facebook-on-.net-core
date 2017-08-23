using System.Collections.Generic;
using System.Threading.Tasks;

namespace facebook_demo.Services
{
    public interface IFacebookService
    {
        Task<AccountDto> GetAccountAsync(string accessToken);

        Task PostOnPage(string accessToken, string message, string pageId, bool published, int publishOn);

        Task PublishPostOnPage(string accessToken, string postId);

        Task<List<PostDto>> GetPagePostsAsync(string accessToken, string pageId);

        Task<IList<PageMetricDto>> GetPostImpressions(string accessToken, string pageId);

        Task<string> AddVideoObject(string accessToken, string videoUrl, string imageUrl, string title);

        Task<string> AddWatchAction(string accessToken, string objectId);
        Task<string> AddLikeAction(string accessToken, string objectId);
    }
}