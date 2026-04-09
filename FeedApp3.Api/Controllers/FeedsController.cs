using FeedApp3.Api.Exceptions;
using FeedApp3.Api.Extensions;
using FeedApp3.Api.Services.Application;
using FeedApp3.Shared.Errors;
using FeedApp3.Shared.Services.DTOs;
using FeedApp3.Shared.Services.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FeedApp3.Shared.Extensions;

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
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(FeedsController), nameof(GetFeedList), correlationId))
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
                    _logger.LogError(
                        ex,
                        "Unexpected error while getting feeds. ErrorCode: {ErrorCode}, UserId: {UserId}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId.ToString());
                    return Problem500();
                }
            }
        }

        [HttpGet("getfeedbyid")]
        public async Task<ActionResult<FeedDto>> GetFeedById([FromQuery] Guid feedId)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(FeedsController), nameof(GetFeedById), correlationId))
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
                    return Problem400(ex.Message, ApiErrorCodes.INVALID_REQUEST_PARAMETERS);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while getting feed by id. ErrorCode: {ErrorCode}, UserId: {UserId}, FeedId: {FeedId}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId.ToString(),
                        feedId.ToString());
                    return Problem500();
                }
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateFeedRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(FeedsController), nameof(Create), correlationId))
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
                    return Problem400(ex.Message, ApiErrorCodes.INVALID_REQUEST_PARAMETERS);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while creating feed. ErrorCode: {ErrorCode}, UserId: {UserId}, FeedUrl: {FeedUrl}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId.ToString(),
                        request.FeedUrl);
                    return Problem500();
                }
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UpdateFeedRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(FeedsController), nameof(Update), correlationId))
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
                    return Problem400(ex.Message, ApiErrorCodes.INVALID_REQUEST_PARAMETERS);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while updating feed. ErrorCode: {ErrorCode}, UserId: {UserId}, FeedId: {FeedId}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId.ToString(),
                        request.FeedId.ToString());
                    return Problem500();
                }
            }

        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteFeedRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(FeedsController), nameof(Delete), correlationId))
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
                    return Problem400(ex.Message, ApiErrorCodes.INVALID_REQUEST_PARAMETERS);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while deleting feed. ErrorCode: {ErrorCode}, UserId: {UserId}, FeedId: {FeedId}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId.ToString(),
                        request.FeedId.ToString());
                    return Problem500();
                }
            }
        }

        [HttpPost("markfeedasread")]
        public async Task<IActionResult> MarkFeedAsRead([FromBody] MarkFeedAsReadRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(FeedsController), nameof(MarkFeedAsRead), correlationId))
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
                    return Problem400(ex.Message, ApiErrorCodes.INVALID_REQUEST_PARAMETERS);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while marking feed as read. ErrorCode: {ErrorCode}, UserId: {UserId}, FeedId: {FeedId}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId.ToString(),
                        request.FeedId.ToString());
                    return Problem500();
                }
            }
        }

        [HttpPost("markarticleasread")]
        public async Task<IActionResult> MarkArticleAsRead([FromBody] MarkArticleAsReadRequest request)
        {
            string? correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            using (_logger.BeginLoggingScope(nameof(FeedsController), nameof(MarkArticleAsRead), correlationId))
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
                    return Problem400(ex.Message, ApiErrorCodes.INVALID_REQUEST_PARAMETERS);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while marking feed as read. ErrorCode: {ErrorCode}, UserId: {UserId}, ArticleId: {ArticleId}",
                        ApiErrorCodes.INTERNAL_SERVER_ERROR,
                        userId.ToString(),
                        request.ArticleId.ToString());
                    return Problem500();
                }
            }
        }

    }
}
