using System;
using System.Collections.Generic;
using System.Text;

namespace Spring.Framework
{
	public static class Base64Extensions
	{
		public static byte[] GetUTFBytes(this string utfString)
		{
			return Encoding.UTF8.GetBytes(utfString);
		}

		public static string ToUtfString(this byte[] bytes)
		{
			return Encoding.UTF8.GetString(bytes);
		}

		public static string ToBase64String(this byte[] bytes)
		{
			return Convert.ToBase64String(bytes);
		}

		public static byte[] ToBase64ByteArray(this string base64)
		{
			return Convert.FromBase64String(base64.PadRight(4 * ((base64.Length + 3) / 4), '='));
		}

		public static string ToBase64String(this string normalString)
		{
			return normalString.GetUTFBytes()
								.ToBase64String();
		}

		public static string ToUrlSafeBase64String(this string normalString)
		{
			return normalString
						.ToBase64String()
						.Replace("+", "-")
						.Replace("/", "_")
						.Replace("=", "");
		}

		public static string FromBase64String(this string encodedString)
		{
			return encodedString
					.ToBase64ByteArray()
					.ToUtfString();
		}

		public static string FromUrlSafeBase64String(this string encodedString)
		{
			return encodedString
					.Replace("-", "+")
					.Replace("_", "/")
					.PadRight(4 * ((encodedString.Length + 3) / 4), '=')
					.ToBase64ByteArray()
					.ToUtfString();
		}
	}
}
