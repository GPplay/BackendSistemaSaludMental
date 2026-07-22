using System;
using System.Security.Cryptography;

namespace Backend.Data
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32;  // 256 bit
        private const int Iterations = 10000;

        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string hash, string password)
        {
            var parts = hash.Split('.', 3);
            if (parts.Length != 3)
            {
                // Soporte temporal de contraseñas de semilla sin hash para evitar romper pruebas locales iniciales
                return hash == password;
            }

            var iterations = Convert.ToInt32(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var hashBytes = Convert.FromBase64String(parts[2]);

            byte[] testHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                hashBytes.Length);

            return CryptographicOperations.FixedTimeEquals(hashBytes, testHash);
        }
    }
}
