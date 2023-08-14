using System;
using System.Collections.Generic;
using System.Text;

namespace Spring.Framework
{
	public static class DictionaryExtensions
	{
		public static void Upsert<TValue>(this IDictionary<string, TValue> dict, [System.Runtime.CompilerServices.CallerMemberName] string key = "", TValue value = default(TValue)) => 
			dict.Upsert<string, TValue>(key, value);

		public static void Upsert<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
		{
			if (dict.ContainsKey(key))
			{
				dict[key] = value;
			}
			else
			{
				dict.Add(key, value);
			}
		}

		public static TValue GetOrDefault<TValue>(this IDictionary<string, TValue> dict, [System.Runtime.CompilerServices.CallerMemberName] string key = "") => 
			dict.GetOrDefault<string, TValue>(key);

		public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            if (dict.ContainsKey(key))
            {
				return dict[key];
            }
            else
            {
				return default(TValue);
            }
        }
	}
}
