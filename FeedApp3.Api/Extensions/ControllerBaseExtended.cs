using FeedApp3.Shared.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Shared.Services.Responses;

namespace FeedApp3.Api.Controllers
{
    public abstract class ControllerBaseExtended : ControllerBase
    {
        protected ObjectResult Problem400(string detail, string errorCode)
        {
            return ProblemWithErrorCode(StatusCodes.Status400BadRequest, detail, errorCode);
        }

        protected ObjectResult Problem401(string detail, string errorCode)
        {
            return ProblemWithErrorCode(StatusCodes.Status401Unauthorized, detail, errorCode);
        }

        protected ObjectResult Problem404(string detail, string errorCode)
        {
            return ProblemWithErrorCode(StatusCodes.Status404NotFound, detail, errorCode);
        }

        protected ObjectResult Problem409(string detail, string errorCode)
        {
            return ProblemWithErrorCode(StatusCodes.Status409Conflict, detail, errorCode);
        }

        protected ObjectResult Problem500()
        {
            return ProblemWithErrorCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.", ApiErrorCodes.INTERNAL_SERVER_ERROR);
        }

        protected ObjectResult ProblemWithErrorCode(int statusCode, string detail, string errorCode)
        {
            string title = ReasonPhrases.GetReasonPhrase(statusCode);
            string path = HttpContext.Request.Path;

            var apiErrorResponse = new ApiErrorResponse
            {
                Title = title,
                Status = statusCode,
                Detail = detail,
                ErrorCode = errorCode,
                Instance = path
            };

            return StatusCode(statusCode, apiErrorResponse);
        }
    }
}
