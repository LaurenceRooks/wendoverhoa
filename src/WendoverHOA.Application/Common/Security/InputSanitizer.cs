using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace WendoverHOA.Application.Common.Security
{
    /// <summary>
    /// Utility class for sanitizing user input to prevent security vulnerabilities
    /// </summary>
    public static class InputSanitizer
    {
        /// <summary>
        /// Sanitizes a string to prevent XSS attacks
        /// </summary>
        /// <param name="input">The input string to sanitize</param>
        /// <returns>A sanitized version of the input string</returns>
        public static string SanitizeForXss(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // Replace potentially dangerous characters with their HTML encoded equivalents
            var sanitized = input
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#x27;")
                .Replace("/", "&#x2F;");

            // Remove any script tags that might have been encoded
            sanitized = Regex.Replace(sanitized, @"javascript:", "", RegexOptions.IgnoreCase);
            
            return sanitized;
        }

        /// <summary>
        /// Sanitizes a string for safe logging
        /// </summary>
        /// <param name="input">The input string to sanitize</param>
        /// <returns>A sanitized version of the input string safe for logging</returns>
        public static string SanitizeForLogging(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // Remove control characters and non-printable characters
            var sanitized = Regex.Replace(input, @"[\p{C}]", string.Empty);
            
            // Limit the length to prevent log injection
            if (sanitized.Length > 100)
            {
                sanitized = sanitized.Substring(0, 100) + "...";
            }
            
            return sanitized;
        }

        /// <summary>
        /// Anonymizes sensitive data for logging purposes
        /// </summary>
        /// <param name="input">The sensitive data to anonymize</param>
        /// <returns>An anonymized version of the input that's safe for logging</returns>
        public static string AnonymizeForLogging(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // Create a hash of the input
            using var sha256 = SHA256.Create();
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(inputBytes);
            
            // Convert to Base64 string and take first 8 characters
            var hash = Convert.ToBase64String(hashBytes);
            var shortHash = hash.Substring(0, Math.Min(8, hash.Length));
            
            // For emails, preserve the domain part for troubleshooting
            if (input.Contains('@'))
            {
                var parts = input.Split('@');
                if (parts.Length == 2)
                {
                    return $"[REDACTED-{shortHash}]@{parts[1]}";
                }
            }
            
            // For usernames or other identifiers, just return the hash
            return $"[REDACTED-{shortHash}]";
        }

        /// <summary>
        /// Anonymizes a user identifier (username, email, or ID) for logging purposes
        /// </summary>
        /// <param name="input">The user identifier to anonymize</param>
        /// <returns>An anonymized version of the user identifier that's safe for logging</returns>
        public static string AnonymizeUserIdentifier(string? input)
        {
            return AnonymizeForLogging(input);
        }

        /// <summary>
        /// Sanitizes a username or email for safe use in the application
        /// </summary>
        /// <param name="input">The username or email to sanitize</param>
        /// <returns>A sanitized version of the username or email</returns>
        public static string SanitizeUsernameOrEmail(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // For usernames, only allow alphanumeric characters, underscores, and periods
            // For emails, apply standard email validation and sanitization
            if (input.Contains('@'))
            {
                // This is likely an email, so sanitize it as such
                return SanitizeEmail(input);
            }
            else
            {
                // This is likely a username, so sanitize it as such
                return SanitizeUsername(input);
            }
        }

        /// <summary>
        /// Sanitizes an email address
        /// </summary>
        /// <param name="email">The email to sanitize</param>
        /// <returns>A sanitized version of the email</returns>
        public static string SanitizeEmail(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return string.Empty;
            }

            // Basic email validation pattern
            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            
            // If it doesn't match the pattern, return an empty string
            if (!Regex.IsMatch(email, emailPattern))
            {
                return string.Empty;
            }
            
            // Otherwise, return the sanitized email
            return SanitizeForXss(email);
        }

        /// <summary>
        /// Sanitizes a username
        /// </summary>
        /// <param name="username">The username to sanitize</param>
        /// <returns>A sanitized version of the username</returns>
        public static string SanitizeUsername(string? username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return string.Empty;
            }

            // Only allow alphanumeric characters, underscores, and periods in usernames
            return Regex.Replace(username, @"[^a-zA-Z0-9._]", string.Empty);
        }
    }
}
