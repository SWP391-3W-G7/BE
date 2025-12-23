using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace LostFoundApi.Filters
{
    public class CheckUserStatusAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            var status = user.FindFirst("Status")?.Value;

            if (status == "Pending")
            {
                var allowedPaths = new[] { "/api/Users/upload-student-id-card", "/api/Users/login", "/api/auth/google-mobile-login", "/api/auth/google-login", "/api/auth/google-callback", "/api/Users/register" }; 
                var requestPath = context.HttpContext.Request.Path.Value;

                if (!allowedPaths.Any(p => requestPath.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                {
                    context.Result = new ForbidResult();
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
