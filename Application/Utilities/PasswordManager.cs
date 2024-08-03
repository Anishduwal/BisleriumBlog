using System.Security.Cryptography;

namespace BisleriumBlog.Application.Utilities
{
    public class PasswordManager
    {
        private const char Separator = ':';

        public static string GenerateHash(string password)
        {
            const int keyLength = 16;
            const int saltLength = 8;
            const int iterationCount = 100_000;

            // Using DESCryptoServiceProvider to ensure encryption is available
            using DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();

            var hashAlgorithm = HashAlgorithmName.SHA256;
            var saltBytes = RandomNumberGenerator.GetBytes(saltLength);
            var hashedPassword = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, iterationCount, hashAlgorithm, keyLength);

            // Combine hash, salt, iterations, and algorithm into a single string
            var hashedResult = string.Join(
                Separator,
                Convert.ToHexString(hashedPassword),
                Convert.ToHexString(saltBytes),
                iterationCount,
                hashAlgorithm
            );

            return hashedResult;
        }

        public static bool ValidatePassword(string password, string storedHash)
        {
            var hashParts = storedHash.Split(Separator);
            var storedPasswordHash = Convert.FromHexString(hashParts[0]);
            var saltBytes = Convert.FromHexString(hashParts[1]);
            var iterationCount = int.Parse(hashParts[2]);
            var hashAlgorithm = new HashAlgorithmName(hashParts[3]);

            var providedPasswordHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                saltBytes,
                iterationCount,
                hashAlgorithm,
                storedPasswordHash.Length
            );

            return CryptographicOperations.FixedTimeEquals(providedPasswordHash, storedPasswordHash);
        }
    }

}
