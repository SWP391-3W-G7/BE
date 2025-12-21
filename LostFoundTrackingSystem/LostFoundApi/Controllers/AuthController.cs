using BLL.DTOs.UserDTO;
using BLL.IServices;
using DAL.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;

        public AuthController(IUserService userService, ILogger<AuthController> logger, IMemoryCache cache)
        {
            _userService = userService;
            _logger = logger;
            _cache = cache;
        }

        [HttpPost("google-mobile-login")]
        public async Task<IActionResult> GoogleMobileLogin([FromBody] GoogleTokenRequestDto request)
        {
            _logger.LogInformation("GoogleMobileLogin initiated.");
            try
            {
                var tokenResponse = await _userService.LoginWithGoogleMobileAsync(request);
                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during GoogleMobileLogin.");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin([FromQuery] int? campusId)
        {
            _logger.LogInformation("GoogleLogin initiated with CampusId: {CampusId}", campusId);

            // Generate a unique state parameter
            var stateId = Guid.NewGuid().ToString();

            // Store campusId in memory cache with the stateId as key
            if (campusId.HasValue)
            {
                _cache.Set($"campus_{stateId}", campusId.Value, TimeSpan.FromMinutes(10));
                _logger.LogInformation("CampusId {CampusId} stored in cache with state {StateId}", campusId.Value, stateId);
            }
            else
            {
                // Still store the stateId even without campus for consistency
                _cache.Set($"campus_{stateId}", 0, TimeSpan.FromMinutes(10));
                _logger.LogInformation("No CampusId provided, stored 0 with state {StateId}", stateId);
            }

            var properties = new AuthenticationProperties
            {
                RedirectUri = $"/api/auth/google-callback?stateId={stateId}",
                Items = { ["stateId"] = stateId }
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback([FromQuery] string stateId)
        {
            _logger.LogInformation("GoogleLoginCallback started with StateId: {StateId}", stateId);

            try
            {
                var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

                if (!authenticateResult.Succeeded)
                {
                    _logger.LogWarning("Google authentication failed. Reason: {ErrorDescription}",
                        authenticateResult.Failure?.Message);
                    return ReturnErrorHtml("Google authentication failed.");
                }

                var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
                var name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name);

                _logger.LogInformation("Authenticated Google user: Email = {Email}, Name = {Name}", email, name);

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogError("Email not found in Google account claims for authenticated user.");
                    return ReturnErrorHtml("Email not found in Google account.");
                }

                // Retrieve campusId from cache
                int? campusId = null;
                if (!string.IsNullOrEmpty(stateId) && _cache.TryGetValue($"campus_{stateId}", out int cachedCampusId))
                {
                    if (cachedCampusId > 0)
                    {
                        campusId = cachedCampusId;
                        _logger.LogInformation("Retrieved CampusId {CampusId} from cache for state {StateId}", campusId, stateId);
                    }
                    else
                    {
                        _logger.LogInformation("No CampusId was provided (value was 0) for state {StateId}", stateId);
                    }

                    // Remove from cache after retrieval
                    _cache.Remove($"campus_{stateId}");
                }
                else
                {
                    _logger.LogWarning("CampusId not found in cache for state {StateId}", stateId);
                }

                var tokenResponse = await _userService.LoginWithGoogleAsync(email, name, campusId);

                _logger.LogInformation("GoogleLoginCallback completed successfully for user {Email}.", email);

                return ReturnSuccessHtml(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during GoogleLoginCallback.");
                return ReturnErrorHtml($"Internal server error: {ex.Message}");
            }
        }

        private IActionResult ReturnSuccessHtml(BLL.DTOs.UserLoginResponseDto token)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Login Successful</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }}
        .container {{
            background: white;
            padding: 2rem;
            border-radius: 10px;
            box-shadow: 0 10px 25px rgba(0,0,0,0.2);
            text-align: center;
        }}
        .success-icon {{
            color: #10b981;
            font-size: 3rem;
            margin-bottom: 1rem;
        }}
        h2 {{
            color: #1f2937;
            margin: 0 0 0.5rem 0;
        }}
        p {{
            color: #6b7280;
            margin: 0.5rem 0;
        }}
    </style>
    <script>
        window.opener.postMessage({{
            type: 'GOOGLE_LOGIN_SUCCESS',
            data: {{
                token: '{token.Token}',
                email: '{token.Email}',
                fullName: '{token.FullName?.Replace("'", "\\'")}',
                roleName: '{token.RoleName}',
                campusName: '{token.CampusName?.Replace("'", "\\'")}',
                campusId: {token.CampusId?.ToString() ?? "null"},
                status: '{token.Status}'
            }}
        }}, '*');
        setTimeout(() => window.close(), 2000);
    </script>
</head>
<body>
    <div class='container'>
        <div class='success-icon'>✓</div>
        <h2>Login Successful!</h2>
        <p>Welcome, {token.FullName}</p>
        <p style='font-size: 0.875rem; margin-top: 1rem;'>This window will close automatically...</p>
    </div>
</body>
</html>";

            return Content(html, "text/html");
        }

        private IActionResult ReturnErrorHtml(string errorMessage)
        {
            var errorHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Login Failed</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
        }}
        .container {{
            background: white;
            padding: 2rem;
            border-radius: 10px;
            box-shadow: 0 10px 25px rgba(0,0,0,0.2);
            text-align: center;
        }}
        .error-icon {{
            color: #ef4444;
            font-size: 3rem;
            margin-bottom: 1rem;
        }}
        h2 {{
            color: #1f2937;
            margin: 0 0 0.5rem 0;
        }}
        p {{
            color: #6b7280;
            margin: 0.5rem 0;
        }}
    </style>
    <script>
        window.opener.postMessage({{
            type: 'GOOGLE_LOGIN_ERROR',
            error: '{errorMessage.Replace("'", "\\'")}'
        }}, '*');
        setTimeout(() => window.close(), 3000);
    </script>
</head>
<body>
    <div class='container'>
        <div class='error-icon'>✗</div>
        <h2>Login Failed</h2>
        <p>{errorMessage}</p>
        <p style='font-size: 0.875rem; margin-top: 1rem;'>This window will close automatically...</p>
    </div>
</body>
</html>";

            return Content(errorHtml, "text/html");
        }
    }
}