using FeedApp3.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FeedApp3.Web.Services
{
    public class ModelStateResponseFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
            {
                return;
            }

            if (!HttpMethods.IsPost(context.HttpContext.Request.Method))
            {
                return;
            }

            if (context.Controller is not Controller controller)
            {
                return;
            }

            if (WebHelpers.IsAjaxRequest(context.HttpContext.Request))
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Detail = "One or more validation errors occurred."
                };

                context.Result = new ObjectResult(problemDetails)
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };

                return;
            }

            //Form POST request
            controller.Response.StatusCode = StatusCodes.Status400BadRequest;

            var model = context.ActionArguments.Values.FirstOrDefault();

            context.Result = controller.View(model);
        }

        public void OnActionExecuted(ActionExecutedContext context) { }

    }
}
