using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNet.Identity;

namespace ToDoApp.Models.Enums
{
    public class OwnPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
#pragma warning disable SCS0006
            SHA1 hash = SHA1.Create();
#pragma warning restore SCS0006

            byte[] input = Encoding.Default.GetBytes(password);

            byte[] output = hash.ComputeHash(input);

            return Encoding.Default.GetString(output);
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if(hashedPassword == HashPassword(providedPassword))
                return PasswordVerificationResult.Success;

            return PasswordVerificationResult.Failed;
        }
    }
}