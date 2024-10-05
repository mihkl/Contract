using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public interface IHMACService
    {
        string GenerateSignature(DateTime startDate, DateTime endDate, string templateId);
        bool IsValidSignature(DateTime startDate, DateTime endDate, string templateId, string signature);
    }

    public class HMACService : IHMACService
    {
        private byte[] _hmacSecretKey = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("HMACSecretKey") ?? "");
        public string GenerateSignature(DateTime startDate, DateTime endDate, string templateId)
        {
            var message = generateMessage(startDate, endDate, templateId);
            return GenerateHmac(message);
        }

        public bool IsValidSignature(DateTime startDate, DateTime endDate, string templateId, string signature)
        {
            var message = generateMessage(startDate, endDate, templateId);
            var expectedSignature = GenerateHmac(message);
            return expectedSignature == signature;
        }

        private string generateMessage(DateTime startDate, DateTime endDate, string templateId)
        {
            return $"{startDate}|{endDate}|{templateId}";
        }

        private string GenerateHmac(string message)
        {
            var encoding = Encoding.UTF8;
            var messageBytes = encoding.GetBytes(message);

            using (var hmac = new HMACSHA256(_hmacSecretKey))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }

}