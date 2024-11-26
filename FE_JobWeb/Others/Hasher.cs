using Microsoft.AspNetCore.Identity;
using System.Text;
using System;

namespace BE_JobWeb.PasswordHasher
{
    public class Hasher
    {
        private readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();

        // Hàm băm mật khẩu
        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }

        // Hàm xác thực mật khẩu
        public bool VerifyPassword(string hashedPassword, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, password);
            return result == PasswordVerificationResult.Success;
        }
        // generate pass random
        public string GeneratePassword(int length = 12)
        {
            Random random = new Random();
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specialChars = "!@#$%^&*()-_+=<>?";

            // Ensure each required type is present at least once
            var password = new StringBuilder();
            password.Append(uppercase[random.Next(uppercase.Length)]);
            password.Append(lowercase[random.Next(lowercase.Length)]);
            password.Append(digits[random.Next(digits.Length)]);
            password.Append(specialChars[random.Next(specialChars.Length)]);

            // Fill the remaining characters randomly from all types
            string allChars = uppercase + lowercase + digits + specialChars;
            for (int i = password.Length; i < length; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle the characters in the password to randomize their positions
            return new string(password.ToString().ToCharArray().OrderBy(c => random.Next()).ToArray());
        }
    }
}
