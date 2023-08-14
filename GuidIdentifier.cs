using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace Spring.Framework
{
	public static class GuidIdentifier
	{
        public static Guid GuidFromStrings(params string[] strings)
        {
            byte[] inputBytes = Encoding.Default.GetBytes(string.Join("|", strings));
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            byte[] hashBytes = provider.ComputeHash(inputBytes);
            return new Guid(hashBytes);
        }
    }

    public static class GuidExtensions
    {
        private static Guid GenerateGuid(this IEnumerable<string> orderedList)
        {
            return GuidFromStrings('|', orderedList.ToArray());
        }

        public static Guid GuidFromStrings(char delimitor = '|', params string[] strings)
        {
            byte[] inputBytes = Encoding.Default.GetBytes(string.Join(delimitor.ToString(), strings));
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            byte[] hashBytes = provider.ComputeHash(inputBytes);
            return new Guid(hashBytes);
        }
    }
}
