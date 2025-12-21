using BLL.IServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LostFoundApi.Controllers
{
    /// <summary>
    /// This is a temporary controller for testing purposes. It should be removed after testing is complete.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public TestController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public record TestEmailRequest(string To, string Subject, string Body);

        /// <summary>
        /// Sends a test email using the configured email service.
        /// </summary>
        /// <param name="request">The email details.</param>
        /// <returns>A log of the email sending attempt.</returns>
        [HttpPost("send-email")]
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.Subject) || string.IsNullOrEmpty(request.Body))
            {
                return BadRequest("Request body must include 'to', 'subject', and 'body'.");
            }

            try
            {
                Console.WriteLine($"[Test Endpoint] Attempting to send email to: {request.To}");
                await _emailService.SendEmailAsync(request.To, request.Subject, request.Body);
                Console.WriteLine("[Test Endpoint] Call to EmailService completed.");
                
                // The EmailService itself logs success or failure. This response confirms the endpoint was called.
                return Ok(new 
                { 
                    message = "Test email request processed. Check the application console logs for detailed output from the EmailService (success or failure).",
                    requestData = request 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Test Endpoint] An unexpected error occurred: {ex}");
                return StatusCode(500, new 
                { 
                    message = "An unexpected error occurred in the test endpoint.",
                    error = ex.ToString() 
                });
            }
        }
    }
}
