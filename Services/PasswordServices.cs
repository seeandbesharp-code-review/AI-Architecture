using Zxcvbn;

namespace Services
{
    public class PasswordServices : IPasswordServices
    {
        private const int MinimumPasswordScore = 2;

        public int PasswordScore(string password)
        {
            var result = Zxcvbn.Core.EvaluatePassword(password);
            return result.Score;
        }

        public bool IsPasswordStrong(string password)
        {
            return PasswordScore(password) >= MinimumPasswordScore;
        }

        public string HashPassword(string password)
        {
            // BCrypt automatically generates a salt and embeds it in the hash
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
