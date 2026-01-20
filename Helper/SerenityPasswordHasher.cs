using System;
using System.Security.Cryptography;
using System.Text;

namespace MobileWebApi.Helper
{
    /// <summary>
    /// Serenity-compatible password hasher matching HRMS Indotalent Web Application
    /// Uses SHA512(password + salt) algorithm with 5-character random salt
    /// This matches the exact format used by UserRepository.GenerateHash in HRMS web app
    /// </summary>
    public static class SerenityPasswordHasher
    {
        // Salt settings matching HRMS web application
        private const int SaltLength = 5; // 5-character random salt like HRMS RandomFileCode().Substring(0, 5)

        /// <summary>
        /// Generates a random salt matching HRMS web application format
        /// Returns a 5-character random string (like Serenity.IO.TemporaryFileHelper.RandomFileCode().Substring(0, 5))
        /// RandomFileCode generates Base64 characters, so we use full Base64 alphabet: A-Z, a-z, 0-9, +, /
        /// Uses cryptographically secure random number generation
        /// </summary>
        /// <returns>5-character random salt string</returns>
        public static string GenerateSalt()
        {
            // Serenity's RandomFileCode generates full Base64 alphabet characters
            // Base64 alphabet: A-Z, a-z, 0-9, +, / (64 characters total)
            // This matches what Serenity.IO.TemporaryFileHelper.RandomFileCode() generates
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            var salt = new StringBuilder(SaltLength);
            var randomBytes = new byte[SaltLength];
            
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            
            // Use modulo to ensure uniform distribution across character set
            for (int i = 0; i < SaltLength; i++)
            {
                salt.Append(chars[randomBytes[i] % chars.Length]);
            }
            
            return salt.ToString();
        }

        /// <summary>
        /// Hashes a password using SHA512(password + salt) matching HRMS web application
        /// This matches UserRepository.GenerateHash and UserRepository.CalculateHash exactly
        /// Returns hash WITHOUT Base64 padding ('=') to match HRMS storage format
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="salt">5-character salt string (matches HRMS format)</param>
        /// <returns>Base64-encoded SHA512 hash (without padding, matches HRMS format)</returns>
        public static string HashPassword(string password, string salt)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            
            if (string.IsNullOrEmpty(salt))
                throw new ArgumentException("Salt cannot be null or empty", nameof(salt));

            // Hash using SHA512(password + salt) - matches HRMS UserRepository.CalculateHash
            string base64Hash = ComputeSHA512Base64(password + salt);
            
            // Remove Base64 padding to match HRMS storage format
            // HRMS stores hash without trailing '==' padding (e.g., 84 chars instead of 88)
            return base64Hash.TrimEnd('=');
        }

        /// <summary>
        /// Verifies a password against a stored hash and salt
        /// Matches HRMS UserRepository.CalculateHash validation exactly
        /// Uses case-insensitive comparison like: CalculateHash(password, salt).Equals(storedHash, StringComparison.OrdinalIgnoreCase)
        /// IMPORTANT: This must match Serenity's validation exactly:
        /// - CalculateHash returns full Base64 string (with padding)
        /// - Stored hash may or may not have padding (depends on how it was saved)
        /// - Comparison is case-insensitive and handles padding differences
        /// </summary>
        /// <param name="enteredPassword">Password entered by user</param>
        /// <param name="storedHash">Stored password hash (Base64 SHA512, with or without padding)</param>
        /// <param name="storedSalt">Stored password salt (5-character for new, or legacy format)</param>
        /// <returns>True if password matches, false otherwise</returns>
        public static bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt)
        {
            if (string.IsNullOrEmpty(enteredPassword) || string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
                return false;

            // Normalize stored hash and salt (remove any trailing dots or spaces, but preserve padding for now)
            // This handles cases where database columns might have length limits or truncation
            string normalizedHash = storedHash.TrimEnd('.', ' ', '\t', '\r', '\n');
            string normalizedSalt = storedSalt.TrimEnd('.', ' ', '\t', '\r', '\n');

            try
            {
                // Compute hash using SHA512(password + salt) - matches HRMS UserRepository.CalculateHash exactly
                // This returns full Base64 string with padding (e.g., 88 characters ending with ==)
                string computedHash = ComputeSHA512Base64(enteredPassword + normalizedSalt);
                
                // Serenity's comparison: CalculateHash(password, salt).Equals(storedHash, StringComparison.OrdinalIgnoreCase)
                // The computed hash has padding, stored hash may or may not have padding
                // We need to compare both with and without padding to match Serenity behavior
                
                // Try exact match first (case-insensitive)
                if (string.Equals(computedHash, normalizedHash, StringComparison.OrdinalIgnoreCase))
                    return true;
                
                // Try match without padding on computed hash (in case stored hash doesn't have padding)
                string computedWithoutPadding = computedHash.TrimEnd('=');
                if (string.Equals(computedWithoutPadding, normalizedHash, StringComparison.OrdinalIgnoreCase))
                    return true;
                
                // Try match without padding on stored hash (in case stored hash has padding but computed doesn't match exactly)
                string storedWithoutPadding = normalizedHash.TrimEnd('=');
                if (string.Equals(computedHash, storedWithoutPadding, StringComparison.OrdinalIgnoreCase))
                    return true;
                
                // Try match both without padding
                if (string.Equals(computedWithoutPadding, storedWithoutPadding, StringComparison.OrdinalIgnoreCase))
                    return true;
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Computes SHA512 hash (matches HRMS UserRepository.CalculateHash and SiteMembershipProvider.ComputeSHA512)
        /// Uses SHA512 of (password + salt) and returns Base64-encoded result
        /// This matches Serenity's SiteMembershipProvider.ComputeSHA512 implementation exactly
        /// </summary>
        private static string ComputeSHA512Base64(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input cannot be null or empty", nameof(input));

            using var sha = SHA512.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }

    }
}

