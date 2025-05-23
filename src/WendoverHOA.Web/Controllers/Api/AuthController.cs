using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WendoverHOA.Application.Common.Security;
using WendoverHOA.Infrastructure.Identity;

namespace WendoverHOA.Web.Controllers.Api
{
    /// <summary>
    /// API controller for authentication operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class
        /// </summary>
        /// <param name="identityService">The identity service</param>
        /// <param name="logger">The logger</param>
        public AuthController(IIdentityService identityService, ILogger<AuthController> logger)
        {
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="request">The registration request</param>
        /// <returns>The result of the registration</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Sanitize user input before passing to service
            var sanitizedEmail = InputSanitizer.SanitizeEmail(request.Email);
            var sanitizedUsername = InputSanitizer.SanitizeUsername(request.Username);
            var sanitizedFirstName = InputSanitizer.SanitizeForXss(request.FirstName);
            var sanitizedLastName = InputSanitizer.SanitizeForXss(request.LastName);
            
            var result = await _identityService.RegisterUserAsync(
                sanitizedEmail,
                sanitizedUsername,
                request.Password,
                sanitizedFirstName,
                sanitizedLastName);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return BadRequest(ModelState);
            }

            _logger.LogInformation("User {Username} registered successfully", InputSanitizer.SanitizeForLogging(sanitizedUsername));

            return CreatedAtAction(nameof(Register), new { username = sanitizedUsername });
        }

        /// <summary>
        /// Authenticates a user
        /// </summary>
        /// <param name="request">The login request</param>
        /// <returns>The authentication result</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = Request.Headers["User-Agent"].ToString();

            // Sanitize user input before passing to service
            var sanitizedUsernameOrEmail = InputSanitizer.SanitizeUsernameOrEmail(request.UsernameOrEmail);
            
            var result = await _identityService.AuthenticateAsync(
                sanitizedUsernameOrEmail,
                request.Password,
                ipAddress,
                userAgent);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed login attempt for user {Username}", InputSanitizer.SanitizeForLogging(sanitizedUsernameOrEmail));
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var (accessToken, refreshToken) = await _identityService.GenerateAuthTokensAsync(
                result.User!,
                ipAddress,
                userAgent);

            _logger.LogInformation("User {Username} logged in successfully", InputSanitizer.SanitizeForLogging(sanitizedUsernameOrEmail));

            // Set refresh token in an HTTP-only cookie
            SetRefreshTokenCookie(refreshToken);

            return Ok(new AuthResponse
            {
                AccessToken = accessToken,
                Username = result.User!.UserName ?? string.Empty,
                Email = result.User.Email ?? string.Empty,
                FirstName = result.User.FirstName,
                LastName = result.User.LastName
            });
        }

        /// <summary>
        /// Refreshes the authentication tokens
        /// </summary>
        /// <returns>The new authentication tokens</returns>
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { message = "Refresh token is required" });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = Request.Headers["User-Agent"].ToString();

            var result = await _identityService.RefreshTokensAsync(
                refreshToken,
                userId,
                ipAddress,
                userAgent);

            if (!result.Succeeded)
            {
                return Unauthorized(new { message = result.Errors.FirstOrDefault() ?? "Invalid refresh token" });
            }

            _logger.LogInformation("Tokens refreshed for user ID {UserId}", InputSanitizer.SanitizeForLogging(userId.ToString()));

            // Set new refresh token in an HTTP-only cookie
            SetRefreshTokenCookie(result.RefreshToken!);

            return Ok(new { accessToken = result.AccessToken });
        }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        /// <returns>The result of the logout operation</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                await _identityService.LogoutAsync(userId, refreshToken);
                _logger.LogInformation("User ID {UserId} logged out successfully", InputSanitizer.SanitizeForLogging(userId.ToString()));
            }

            // Remove the refresh token cookie
            Response.Cookies.Delete("refreshToken");

            return Ok(new { message = "Logged out successfully" });
        }

        /// <summary>
        /// Sets the refresh token in an HTTP-only cookie
        /// </summary>
        /// <param name="refreshToken">The refresh token</param>
        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(14)
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }

    /// <summary>
    /// Registration request model
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Gets or sets the email
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email must be at most {1} characters")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between {2} and {1} characters")]
        [RegularExpression(@"^[a-zA-Z0-9._]+$", ErrorMessage = "Username can only contain letters, numbers, periods, and underscores")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least {2} characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
            ErrorMessage = "Password must include at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name must be at most {1} characters")]
        [RegularExpression(@"^[a-zA-Z\s-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name must be at most {1} characters")]
        [RegularExpression(@"^[a-zA-Z\s-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string LastName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Login request model
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Gets or sets the username or email
        /// </summary>
        [Required(ErrorMessage = "Username or email is required")]
        [StringLength(100, ErrorMessage = "Username or email must be at most {1} characters")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least {2} characters")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Authentication response model
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Gets or sets the access token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;
    }
}
