using System.Security.Cryptography;
using System.Text;

namespace LiveStock.Core.Services
{
    public class PasswordHashingService
    {
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                // Generate a random salt
                byte[] salt = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                // Combine password and salt
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];
                Array.Copy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
                Array.Copy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

                // Hash the salted password
                byte[] hashedBytes = sha256.ComputeHash(saltedPassword);

                // Combine salt and hash for storage
                byte[] result = new byte[salt.Length + hashedBytes.Length];
                Array.Copy(salt, 0, result, 0, salt.Length);
                Array.Copy(hashedBytes, 0, result, salt.Length, hashedBytes.Length);

                return Convert.ToBase64String(result);
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                byte[] storedBytes = Convert.FromBase64String(hashedPassword);
                
                // Extract salt (first 16 bytes)
                byte[] salt = new byte[16];
                Array.Copy(storedBytes, 0, salt, 0, 16);

                // Extract hash (remaining bytes)
                byte[] storedHash = new byte[storedBytes.Length - 16];
                Array.Copy(storedBytes, 16, storedHash, 0, storedHash.Length);

                // Hash the provided password with the extracted salt
                using (var sha256 = SHA256.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];
                    Array.Copy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
                    Array.Copy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

                    byte[] computedHash = sha256.ComputeHash(saltedPassword);

                    // Compare hashes
                    return storedHash.SequenceEqual(computedHash);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}