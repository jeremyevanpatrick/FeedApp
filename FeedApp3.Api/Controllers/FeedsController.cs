using FeedApp3.Api.Errors;
using FeedApp3.Api.Exceptions;
using FeedApp3.Api.Extensions;
using FeedApp3.Api.Services.Application;
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
        private readonly IFeedAppService _feedAppService;

        public FeedsController(
            ILogger<FeedsController> logger,
            IFeedAppService feedAppService)
        {
            _logger = logger;
            _feedAppService = feedAppService;
        }

        [HttpGet("getfeedlist")]
        public async Task<ActionResult<List<FeedDto>>> GetFeedList()
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                List<FeedDto> feedDtos = await _feedAppService.GetFeedListAsync(userId);

                return Ok(feedDtos);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(FeedErrorCodes.GetFeedListUnexpected, ex, "Unexpected error while getting feeds", new Dictionary<string, string> {
                    { "UserId", userId.ToString() }
                });
                return Problem500();
            }
        }

        [HttpGet("getfeedbyid")]
        public async Task<ActionResult<FeedDto>> GetFeedById([FromQuery] Guid feedId)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                FeedDto feedDto = await _feedAppService.GetFeedByIdAsync(userId, feedId);

                return Ok(feedDto);
            }
            catch (NotFoundException ex)
            {
                return Problem400(ex.Message, ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(FeedErrorCodes.GetFeedByIdUnexpected, ex, "Unexpected error while getting feed by id", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "FeedId", feedId.ToString() }
                });
                return Problem500();
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateFeedRequest request)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                await _feedAppService.CreateAsync(userId, request.FeedUrl);

                return Ok();
            }
            catch (NotFoundException ex)
            {
                return Problem400(ex.Message, ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(FeedErrorCodes.CreateFeedUnexpected, ex, "Unexpected error while creating feed", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "FeedUrl", request.FeedUrl }
                });
                return Problem500();
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UpdateFeedRequest request)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();
                
                await _feedAppService.UpdateAsync(userId, (Guid)request.FeedId);

                return Ok();
            }
            catch (NotFoundException ex)
            {
                return Problem400(ex.Message, ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(FeedErrorCodes.UpdateFeedUnexpected, ex, "Unexpected error while updating feed", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "FeedId", request.FeedId.ToString() }
                });
                return Problem500();
            }

        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteFeedRequest request)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                await _feedAppService.DeleteAsync(userId, (Guid)request.FeedId);

                return Ok();
            }
            catch (NotFoundException ex)
            {
                return Problem400(ex.Message, ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(FeedErrorCodes.DeleteFeedUnexpected, ex, "Unexpected error while deleting feed", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "FeedId", request.FeedId.ToString() }
                });
                return Problem500();
            }
        }

        [HttpPost("markfeedasread")]
        public async Task<IActionResult> MarkFeedAsRead([FromBody] MarkFeedAsReadRequest request)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();

                await _feedAppService.MarkFeedAsReadAsync(userId, (Guid)request.FeedId);

                return Ok();
            }
            catch (NotFoundException ex)
            {
                return Problem400(ex.Message, ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(FeedErrorCodes.MarkFeedAsReadUnexpected, ex, "Unexpected error while marking feed as read", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "FeedId", request.FeedId.ToString() }
                });
                return Problem500();
            }
        }

        [HttpPost("markarticleasread")]
        public async Task<IActionResult> MarkArticleAsRead([FromBody] MarkArticleAsReadRequest request)
        {
            Guid userId = Guid.Empty;

            try
            {
                userId = User.GetUserId();
                
                await _feedAppService.MarkArticleAsReadAsync(userId, (Guid)request.ArticleId);

                return Ok();
            }
            catch (NotFoundException ex)
            {
                return Problem400(ex.Message, ResponseErrorCodes.INVALID_REQUEST_PARAMETERS);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithDictionary(FeedErrorCodes.MarkArticleAsReadUnexpected, ex, "Unexpected error while marking article as read", new Dictionary<string, string> {
                    { "UserId", userId.ToString() },
                    { "ArticleId", request.ArticleId.ToString() }
                });
                return Problem500();
            }
        }

    }
}
