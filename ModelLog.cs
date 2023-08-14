using Spring.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Spring.Framework
{
	public class ModelLog<T> : List<T> where T : PartialModel
	{
		public T GetCurrentState()
		{
			return Enumerable.Range(0, this.Count).ToList().Aggregate(new JObject(), (parent, i) => {
				var current = this.ElementAt(i);
				parent.Merge(JObject.FromObject(current));
				return parent;
			}).ToObject<T>();
		}

		public KeyType FirstValue<KeyType>(string KeyName)
		{
			var version = this.FirstOrDefault(item => item.ContainsKey(KeyName));
			if(version != null)
			{
				return version.GetValue<KeyType>(KeyName);
			}
			else
			{
				return default(KeyType);
			}
		}

		public KeyType LastValue<KeyType>(string KeyName)
		{
			var version = this.LastOrDefault(item => item.ContainsKey(KeyName));
			if (version != null)
			{
				return (KeyType)version.GetValue<KeyType>(KeyName);
			}
			else
			{
				return default(KeyType);
			}
		}
	}
}
