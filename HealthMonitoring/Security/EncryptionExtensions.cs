using System;
using System.Security.Cryptography;
using System.Text;

namespace HealthMonitoring.Security
{
    public static class EncryptionExtensions
    {
        private static readonly Encoding Encoding = Encoding.ASCII;

        public static string ToSha256Hash(this string value)
        {
            var algorithm = SHA256.Create();
            var hash = new StringBuilder();

            var crypto = algorithm.ComputeHash(Encoding.GetBytes(value), 0, Encoding.GetByteCount(value));
            foreach (byte _byte in crypto)
            {
                hash.Append(_byte.ToString("x2"));
            }
            return hash.ToString();
        }

        public static string ToBase64String(this string value)
        {
            return Convert.ToBase64String(Encoding.GetBytes(value));
        }

        public static string FromBase64String(this string value)
        {
            return Encoding.GetString(Convert.FromBase64String(value));
        }
    }
}
