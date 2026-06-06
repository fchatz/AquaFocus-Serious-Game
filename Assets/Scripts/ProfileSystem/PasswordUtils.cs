using System.Security.Cryptography;
using System.Text;

public static class PasswordUtils
{
    public static string HashPassword(string input)
    {
        using (SHA256 sha = SHA256.Create())
        {
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return System.BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }

    public static bool CheckPassword(string input, string hash)
    {
        return HashPassword(input) == hash;
    }
}
