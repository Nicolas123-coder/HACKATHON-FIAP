using Microsoft.AspNetCore.Identity;

namespace Utils
{
    public class PasswordHasherService
    {
        private readonly IPasswordHasher<object> _passwordHasher;

        public PasswordHasherService()
        {
            _passwordHasher = new PasswordHasher<object>();
        }

        public string HashPassword(object user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }

        public PasswordVerificationResult VerifyHashedPassword(object user, string hashedPassword, string providedPassword)
        {
            return _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
        }
    }
}