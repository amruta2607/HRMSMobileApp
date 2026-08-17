using System;

namespace MobileWebApi.Helper
{
    /// <summary>
    /// Password helper - delegates to SerenityPasswordHasher for consistency
    /// Maintains backward compatibility with existing code
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Verifies a password against stored hash and salt
        /// Supports both PBKDF2 (new) and SHA512 (legacy) formats
        /// </summary>
        public static bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt)
        {
            return SerenityPasswordHasher.VerifyPassword(enteredPassword, storedHash, storedSalt);
        }

        /// <summary>
        /// Generates a cryptographically secure random salt
        /// Uses PBKDF2-compatible format (32 bytes, base64)
        /// </summary>
        public static string GenerateSalt()
        {
            return SerenityPasswordHasher.GenerateSalt();
        }

        /// <summary>
        /// Hashes a password using PBKDF2 (Serenity-compatible)
        /// This ensures passwords hashed via Web API match Web App format
        /// </summary>
        public static string HashPassword(string password, string salt)
        {
            return SerenityPasswordHasher.HashPassword(password, salt);
        }
    }
}
