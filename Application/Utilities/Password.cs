using System.Security.Cryptography;

namespace BisleriumBlog.Application.Utilities
{
    public class Password
    {
        private const char Colon = ':';

        public static string HashSecret(string input)
        {
            const int keySize = 16;
            const int saltSize = 8;
            const int iterations = 100_000;

            DESCryptoServiceProvider provider = new DESCryptoServiceProvider();

            var algorithm = HashAlgorithmName.SHA256;
            var salt = RandomNumberGenerator.GetBytes(saltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(input, salt, iterations, algorithm, keySize);

            var result = string.Join(
                Colon,
                Convert.ToHexString(hash),
                Convert.ToHexString(salt),
                iterations,
                algorithm
            );

            return result;
        }

        public static bool VerifyHash(string input, string hashString)
        {
            var segments = hashString.Split(Colon);
            var hash = Convert.FromHexString(segments[0]);
            var salt = Convert.FromHexString(segments[1]);
            var iterations = int.Parse(segments[2]);
            var algorithm = new HashAlgorithmName(segments[3]);

            var inputHash = Rfc2898DeriveBytes.Pbkdf2(
                input,
                salt,
                iterations,
                algorithm,
                hash.Length
            );

            return CryptographicOperations.FixedTimeEquals(inputHash, hash);
        }
    }
}
