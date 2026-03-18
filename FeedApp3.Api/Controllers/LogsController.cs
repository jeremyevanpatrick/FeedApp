using FeedApp3.Api.Services;
using FeedApp3.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace FeedApp3.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ErrorLogQueue _errorLogQueue;

        public LogsController(ErrorLogQueue errorLogQueue)
        {
            _errorLogQueue = errorLogQueue;
        }

        [HttpPost("logerror")]
        public async Task<IActionResult> LogError([FromBody] Error error)
        {
            _errorLogQueue.Enqueue(error);
            return Ok();
        }
    }
}
