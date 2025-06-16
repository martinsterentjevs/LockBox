using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LockBox
{
    public static class PasswordGenerator
    {
        // Possibly safe to use for web charset
        private const string CharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-_=+[]";

        public static string GeneratePassword(int length)
        {
            if (length <= 0) throw new ArgumentException("Password length must be greater than 0", nameof(length));

            var password = new char[length];
            var characterSetBytes = CharacterSet.ToCharArray();

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomNumbers = new byte[length];

                rng.GetBytes(randomNumbers); // Fill with cryptographically secure random bytes

                for (int i = 0; i < length; i++)
                {
                    password[i] = characterSetBytes[randomNumbers[i] % CharacterSet.Length];
                }
            }

            return new string(password);
        }
    }
}
