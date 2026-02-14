using FeedApp3.Shared.Helpers;
using FeedApp3.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Net;

namespace FeedApp3.Web.Services
{
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (WebHelpers.IsAjaxRequest(context.HttpContext.Request))
            {
                //ajax request
                if (context.Exception is HttpRecoverableError)
                {
                    bool isUnauthorized = false;
                    if (((HttpRecoverableError)context.Exception).IsUnauthorized)
                    {
                        isUnauthorized = true;
                    }

                    //user error
                    context.Result = new JsonResult(new { detail = context.Exception.Message, isUnauthorized = isUnauthorized })
                    {
                        StatusCode = (int)((HttpRecoverableError)context.Exception).HttpStatusCode
                    };
                }
                else
                {
                    //unexpected error
                    var controller = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
                    var action = context.RouteData.Values["action"]?.ToString() ?? "Unknown";
                    _logger.LogErrorWithDictionary(WebErrorCodes.ControllerUnexpected, context.Exception, $"Unexpected error in {controller}/{action}");
                    
                    context.Result = new JsonResult(new { detail = WebErrorMessages.UnknownError })
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    };
                }
            }
            else
            {
                //page navigation
                if (context.Exception is HttpRecoverableError)
                {
                    if (((HttpRecoverableError)context.Exception).IsUnauthorized)
                    {
                        context.Result = new RedirectToActionResult(
                            actionName: "Login",
                            controllerName: "Login",
                            routeValues: new { message = context.Exception.Message }
                        );
                        context.HttpContext.Response.StatusCode = (int)((HttpRecoverableError)context.Exception).HttpStatusCode;
                        context.ExceptionHandled = true;
                        return;
                    }

                    //user error
                    context.ModelState.AddModelError(string.Empty, context.Exception.Message);
                    context.HttpContext.Response.StatusCode = (int)((HttpRecoverableError)context.Exception).HttpStatusCode;
                }
                else
                {
                    //unexpected error
                    var controller = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
                    var action = context.RouteData.Values["action"]?.ToString() ?? "Unknown";
                    _logger.LogErrorWithDictionary(WebErrorCodes.ControllerUnexpected, context.Exception, $"Unexpected error in {controller}/{action}");

                    context.ModelState.AddModelError(string.Empty, WebErrorMessages.UnknownError);
                    context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }

                context.Result = new ViewResult
                {
                    ViewName = context.RouteData.Values["action"]?.ToString(),
                    ViewData = new ViewDataDictionary(
                        new EmptyModelMetadataProvider(),
                        context.ModelState)
                };

            }

            context.ExceptionHandled = true;

        }
    }

}
