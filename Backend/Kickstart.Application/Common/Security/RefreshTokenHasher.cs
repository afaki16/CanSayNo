using System;
using System.Security.Cryptography;
using System.Text;

namespace Kickstart.Application.Common.Security
{
    /// <summary>
    /// SHA-256 over UTF-8 bytes of the plaintext token, stored as Base64.
    /// Shared by refresh tokens and password-reset tokens — both store only the hash.
    /// </summary>
    public static class RefreshTokenHasher
    {
        public static string Hash(string plainToken)
        {
            if (plainToken is null)
                throw new ArgumentNullException(nameof(plainToken));

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainToken));
            return Convert.ToBase64String(bytes);
        }
    }
}
