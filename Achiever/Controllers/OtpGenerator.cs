using System.Security.Cryptography;

namespace Achiever.Controllers
{
    public static class OtpGenerator
	{
		public static string GenerateNumericOTP(int length)
		{
			using (var rng = RandomNumberGenerator.Create())
			{
				var bytes = new byte[length];
				rng.GetBytes(bytes);

				// Convert the random bytes into a numeric string
				var result = new char[length];
				for (int i = 0; i < length; i++)
				{
					// Use modulo 10 to ensure the result is a digit (0-9)
					result[i] = (char)('0' + (bytes[i] % 10));
				}
				return new string(result);
			}
		}
	}
}
