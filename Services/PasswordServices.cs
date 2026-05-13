using Zxcvbn;

namespace Services
{
    public class PasswordServices : IPasswordServices
    {
        public int PasswordScore(string password)
        {
            var result = Zxcvbn.Core.EvaluatePassword(password);
            return result.Score;
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
