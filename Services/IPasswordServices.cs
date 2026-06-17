namespace Services
{
    public interface IPasswordServices
    {
        int PasswordScore(string password);
        bool IsPasswordStrong(string password);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}
