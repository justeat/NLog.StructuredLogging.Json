using System.Security.Cryptography;
using System.Text;

namespace NLog.StructuredLogging.Json.Helpers
{
    internal static class Sha1Hasher
    {
        public static string Hash(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            byte[] hash;

            using (var sha1 = new SHA1Managed())
            {
                hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            }

            var sb = new StringBuilder(hash.Length * 2);

            foreach (var b in hash)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
