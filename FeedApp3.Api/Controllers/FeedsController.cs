using FeedApp3.Api.Extensions;
using FeedApp3.Api.Helpers;
using FeedApp3.Api.Models;
using FeedApp3.Api.Services;
using FeedApp3.Shared.Helpers;
using FeedApp3.Shared.Services.DTOs;
using FeedApp3.Shared.Services.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Helpers;

namespace FeedApp3.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class FeedsController : ControllerBaseExtended
    {
        private readonly ILogger<FeedsController> _logger;
        private readonly IFeedDbService _feedDbService;
        private readonly RssHttpClient _rssClient;

        public FeedsController(
            ILogger<FeedsController> logger,
            IFeedDbService feedDbService,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _feedDbService = feedDbService;
            _rssClient = new RssHttpClient(httpClientFactory);
        }

        [HttpGet("getfeedlist")]
        public async Task<ActionResult<List<FeedDto>>> GetFeedList()
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                List<Feed> feeds = await _feedDbService.GetListByUserId(userId);
                List<FeedDto?> feedDtos = feeds.Select(f => FeedMapper.ToDto(f)).ToList();

                await _feedDbService.CreateFeedUpdateAsync(userId);

                return Ok(feedDtos);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(FeedErrorCodes.GetFeedListUnexpected, ex, "Unexpected error while getting feeds", new Dictionary<string, string> {
                    { "UserId", userId.ToString() }
                });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpGet("getfeedbyid")]
        public async Task<ActionResult<FeedDto>> GetFeedById([FromQuery] Guid feedId)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                Feed? feed = await _feedDbService.GetByFeedId(userId, feedId);
                if (feed == null)
                {
                    return Problem400("Invalid feedId. Please check your information and try again.", ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
                }

                FeedDto feedDto = FeedMapper.ToDto(feed);
                return Ok(feedDto);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(FeedErrorCodes.GetFeedByIdUnexpected, ex, "Unexpected error while getting feed by id", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "FeedId", feedId.ToString() }
                });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateFeedRequest request)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                bool isDuplicateUrl = await _feedDbService.IsDuplicateFeedUrl(userId, request.FeedUrl);
                if (isDuplicateUrl)
                {
                    return Ok();
                }

                FeedDto? feedDto = null;

                try
                {
                    feedDto = await _rssClient.ImportFeedFromUrl(request.FeedUrl);
                }
                catch (Exception ex)
                {
                    return Problem400("Invalid feed url. Please check your information and try again.", ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
                }

                Feed feed = FeedMapper.ToEntity(feedDto);
                feed.UserId = userId;
                await _feedDbService.CreateAsync(feed);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(FeedErrorCodes.CreateFeedUnexpected, ex, "Unexpected error while creating feed", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "FeedUrl", request.FeedUrl }
                });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UpdateFeedRequest request)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                Feed? existingFeed = await _feedDbService.GetByFeedId(userId, (Guid)request.FeedId);
                if (existingFeed == null)
                {
                    return Problem400("Invalid feedId. Please check your information and try again.", ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
                }

                FeedDto latestFeedDto = await _rssClient.ImportFeedFromUrl(existingFeed.FeedUrl, existingFeed.LastModified);
                Feed latestFeed = FeedMapper.ToEntity(latestFeedDto);
                await _feedDbService.UpdateAsync(existingFeed, latestFeed);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(FeedErrorCodes.UpdateFeedUnexpected, ex, "Unexpected error while updating feed", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "FeedId", request.FeedId.ToString() }
                });
                return Problem("An unexpected error occurred. Please try again later.");
            }

        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteFeedRequest request)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                Feed? existingFeed = await _feedDbService.GetByFeedId(userId, (Guid)request.FeedId);
                if (existingFeed == null)
                {
                    return Problem400("Invalid feedId. Please check your information and try again.", ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
                }

                await _feedDbService.DeleteAsync(existingFeed);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(FeedErrorCodes.DeleteFeedUnexpected, ex, "Unexpected error while deleting feed", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "FeedId", request.FeedId.ToString() }
                });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("markfeedasread")]
        public async Task<IActionResult> MarkFeedAsRead([FromBody] MarkFeedAsReadRequest request)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                Feed? existingFeed = await _feedDbService.GetByFeedId(userId, (Guid)request.FeedId);
                if (existingFeed == null)
                {
                    return Problem400("Invalid feedId. Please check your information and try again.", ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
                }

                await _feedDbService.MarkFeedAsReadAsync(existingFeed);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(FeedErrorCodes.MarkFeedAsReadUnexpected, ex, "Unexpected error while marking feed as read", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "FeedId", request.FeedId.ToString() }
                });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("markarticleasread")]
        public async Task<IActionResult> MarkArticleAsRead([FromBody] MarkArticleAsReadRequest request)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                Article? existingArticle = await _feedDbService.GetByArticleId((Guid)request.ArticleId);
                if (existingArticle == null)
                {
                    return Problem400("Invalid articleId. Please check your information and try again.", ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
                }

                await _feedDbService.MarkArticleAsReadAsync(existingArticle);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(FeedErrorCodes.MarkArticleAsReadUnexpected, ex, "Unexpected error while marking article as read", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "ArticleId", request.ArticleId.ToString() }
                });
                return Problem("An unexpected error occurred. Please try again later.");
            }
        }

    }
}
