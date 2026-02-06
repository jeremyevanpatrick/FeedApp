using FeedApp3.Api.Services;
using FeedApp3.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace FeedApp3.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly LoggingDbService _loggingDbService;

        public LogsController(LoggingDbService loggingDbService)
        {
            _loggingDbService = loggingDbService;
        }

        [HttpPost("logerror")]
        public async Task<IActionResult> LogError([FromBody] Error error)
        {
            _loggingDbService.LogError(error);
            return Ok();
        }
    }
}
