using FeedApp3.Shared.Models;
using FeedApp3.Shared.Services.Queues;
using Microsoft.AspNetCore.Mvc;

namespace FeedApp3.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ILogProcessorQueue _logProcessorQueue;

        public LogsController(ILogProcessorQueue logProcessorQueue)
        {
            _logProcessorQueue = logProcessorQueue;
        }

        [HttpPost("log")]
        public async Task<IActionResult> Log([FromBody] ApplicationLog applicationLog)
        {
            _logProcessorQueue.Enqueue(applicationLog);
            return Ok();
        }

        [HttpPost("logbatch")]
        public async Task<IActionResult> LogBatch([FromBody] List<ApplicationLog> applicationLogs)
        {
            if (applicationLogs != null)
            {
                foreach (var applicationLog in applicationLogs)
                {
                    _logProcessorQueue.Enqueue(applicationLog);
                }
            }
            return Ok();
        }
    }
}
