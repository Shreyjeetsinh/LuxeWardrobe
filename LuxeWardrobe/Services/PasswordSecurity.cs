using System;
using System.Security.Cryptography;

namespace LuxeWardrobe.Services
{
    public static class PasswordSecurity
    {
        public static string HashPassword(string password, out string salt)
        {
            var saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            salt = Convert.ToBase64String(saltBytes);
            return HashPassword(password, salt);
        }

        public static bool Verify(string password, string salt, string expectedHash)
        {
            var actual = HashPassword(password, salt);
            return SlowEquals(Convert.FromBase64String(actual), Convert.FromBase64String(expectedHash));
        }

        private static string HashPassword(string password, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000))
            {
                return Convert.ToBase64String(pbkdf2.GetBytes(32));
            }
        }

        private static bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (var i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }
    }
}
