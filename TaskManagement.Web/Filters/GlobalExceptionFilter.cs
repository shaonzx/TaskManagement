using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace TaskManagement.Web.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "An unhandled exception occurred");

            if (context.Exception is DbUpdateException dbUpdateException)
            {
                context.Result = new ViewResult { ViewName = "Error" };
                context.ExceptionHandled = true;
            }
            else if (context.Exception is UnauthorizedAccessException)
            {
                context.Result = new RedirectToPageResult("/AccessDenied");
                context.ExceptionHandled = true;
            }

            if (!context.ExceptionHandled)
            {
                context.Result = new ViewResult { ViewName = "Error" };
                context.ExceptionHandled = true;
            }
        }
    }
}