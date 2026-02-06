using FeedApp3.Shared.Data;
using FeedApp3.Shared.Helpers;
using FeedApp3.Shared.Services.DTOs;
using FeedApp3.Shared.Services.Requests;
using FeedApp3.Web.Services;
using FeedApp3.Web.Settings;
using Microsoft.Extensions.Options;

namespace FeedApp3.Web.Data
{
    public class FeedClient : IFeedClient
    {
        private readonly RequestHelper _requestHelperAuthenticated;
        private readonly string _apiBaseUrl;

        public FeedClient(
            AuthenticatedHttpClient authenticatedHttpClient,
            IOptions<ApplicationSettings> applicationSettings)
        {
            _requestHelperAuthenticated = new RequestHelper(authenticatedHttpClient.Client);
            _apiBaseUrl = applicationSettings.Value.ApiBaseUrl;
        }

        public async Task<List<FeedDto>> GetFeedList()
        {
            string requestUrl = $"{_apiBaseUrl}/feeds/getfeedlist";

            return await _requestHelperAuthenticated.GetAsync<List<FeedDto>>(requestUrl, null, 9000);
        }

        public async Task<FeedDto> GetFeedById(Guid feedId)
        {
            string requestUrl = $"{_apiBaseUrl}/feeds/getfeedbyid?feedId={feedId}";

            return await _requestHelperAuthenticated.GetAsync<FeedDto>(requestUrl, null, 9000);
        }

        public async Task Create(string feedUrl)
        {
            string requestUrl = $"{_apiBaseUrl}/feeds/create";

            CreateFeedRequest request = new CreateFeedRequest(feedUrl);
            await _requestHelperAuthenticated.PostAsync<CreateFeedRequest>(requestUrl, request, null, 30000);
        }

        public async Task Update(Guid feedId)
        {
            string requestUrl = $"{_apiBaseUrl}/feeds/update";

            UpdateFeedRequest request = new UpdateFeedRequest(feedId);
            await _requestHelperAuthenticated.PostAsync<UpdateFeedRequest>(requestUrl, request, null, 30000);
        }

        public async Task Delete(Guid feedId)
        {
            string requestUrl = $"{_apiBaseUrl}/feeds/delete";

            DeleteFeedRequest request = new DeleteFeedRequest(feedId);
            await _requestHelperAuthenticated.PostAsync<DeleteFeedRequest>(requestUrl, request, null, 9000);
        }

        public async Task MarkFeedAsRead(Guid feedId)
        {
            string requestUrl = $"{_apiBaseUrl}/feeds/markfeedasread";

            MarkFeedAsReadRequest request = new MarkFeedAsReadRequest(feedId);
            await _requestHelperAuthenticated.PostAsync<MarkFeedAsReadRequest>(requestUrl, request, null, 9000);
        }

        public async Task MarkArticleAsRead(Guid articleId)
        {
            string requestUrl = $"{_apiBaseUrl}/feeds/markarticleasread";

            MarkArticleAsReadRequest request = new MarkArticleAsReadRequest(articleId);
            await _requestHelperAuthenticated.PostAsync<MarkArticleAsReadRequest>(requestUrl, request, null, 9000);
        }
    }
}
