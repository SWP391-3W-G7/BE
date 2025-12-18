using BLL.IServices;
using DAL.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LostFoundApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin([FromQuery] int? campusId)
        {
            _logger.LogInformation("GoogleLogin initiated with CampusId: {CampusId}", campusId);

            var properties = new AuthenticationProperties
            {
                RedirectUri = "/api/auth/google-login-callback"
            };

            // Store campusId in the authentication properties state
            if (campusId.HasValue)
            {
                properties.Items["campusId"] = campusId.Value.ToString();
                _logger.LogInformation("CampusId {CampusId} stored in authentication state.", campusId.Value);
            }

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-login-callback")]
        public async Task<IActionResult> GoogleLoginCallback()
        {
            _logger.LogInformation("GoogleLoginCallback started.");

            try
            {
                var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

                if (!authenticateResult.Succeeded)
                {
                    _logger.LogWarning("Google authentication failed. Reason: {ErrorDescription}",
                        authenticateResult.Failure?.Message);
                    return BadRequest("Google authentication failed.");
                }

                var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
                var name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name);

                _logger.LogInformation("Authenticated Google user: Email = {Email}, Name = {Name}", email, name);

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogError("Email not found in Google account claims for authenticated user.");
                    return BadRequest("Email not found in Google account.");
                }

                // Retrieve campusId from authentication properties
                int? campusId = null;
                if (authenticateResult.Properties?.Items.TryGetValue("campusId", out var campusIdString) == true)
                {
                    if (int.TryParse(campusIdString, out var parsedCampusId))
                    {
                        campusId = parsedCampusId;
                        _logger.LogInformation("Retrieved CampusId {CampusId} from authentication state.", campusId);
                    }
                }

                var token = await _userService.LoginWithGoogleAsync(email, name, campusId);

                _logger.LogInformation("GoogleLoginCallback completed successfully for user {Email}.", email);

                // Return HTML page that sends data to the parent window and closes itself
                var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Login Successful</title>
    <script>
        window.opener.postMessage({{
            type: 'GOOGLE_LOGIN_SUCCESS',
            data: {{
                token: '{token.Token}',
                email: '{token.Email}',
                fullName: '{token.FullName?.Replace("'", "\\'")}',
                roleName: '{token.RoleName}',
                campusName: '{token.CampusName?.Replace("'", "\\'")}',
                campusId: {token.CampusId?.ToString() ?? "null"}
            }}
        }}, '*');
        window.close();
    </script>
</head>
<body>
    <p>Login successful! This window will close automatically...</p>
    <p>If it doesn't close, you can close it manually.</p>
</body>
</html>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during GoogleLoginCallback.");

                var errorHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Login Failed</title>
    <script>
        window.opener.postMessage({{
            type: 'GOOGLE_LOGIN_ERROR',
            error: 'Internal server error during Google login.'
        }}, '*');
        window.close();
    </script>
</head>
<body>
    <p>Login failed. This window will close automatically...</p>
</body>
</html>";

                return Content(errorHtml, "text/html");
            }
        }
    }
}