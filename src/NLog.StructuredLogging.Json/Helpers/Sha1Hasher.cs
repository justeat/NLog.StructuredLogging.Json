using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace NLog.StructuredLogging.Json.Helpers
{
    internal static class Sha1Hasher
    {
#pragma warning disable CA5350
        public static string Hash(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            byte[] hash;

            using (var sha1 = SHA1.Create())
            {
                hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            }

            var sb = new StringBuilder(hash.Length * 2);

            foreach (var b in hash)
            {
                sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }
    }
}
