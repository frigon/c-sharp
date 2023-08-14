using Spring.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;
using Spring.Interfaces;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace Spring.Framework
{
	public class ModelHistory<T> : System.Collections.ObjectModel.ObservableCollection<T> where T : PartialModel
	{
		public ModelHistory(){

		}

		public ModelHistory(params T[] items) : base(items.ToList()) { }

		public ModelHistory(IEnumerable<T> items) : base(items) { 
		}

		public T GetCurrentState()
		{
			return Enumerable.Range(0, this.Count).ToList().Aggregate(new JObject(), (parent, i) => {
				var current = this.ElementAt(i);
				parent.Merge(JObject.FromObject(current));
				return parent;
			}).ToObject<T>();
		}

		public PropertyType PropertyValueInitial<PropertyType>(string PropertyName)
		{
			var version = this.FirstOrDefault(item => item.ContainsKey(PropertyName));
			if(version != null)
			{
				return version.GetValue<PropertyType>(PropertyName);
			}
			else
			{
				return default(PropertyType);
			}
		}

		public List<(KeyType value, DateTimeOffset? modified)> PropertyValueHistory<KeyType>(string KeyName)
		{
			return this.Where(item => item.ContainsKey(KeyName))
						.Select(item => (
							value: item.GetValue<KeyType>(KeyName), 
							modified: item.GetValue<DateTimeOffset?>(nameof(ITimestamps.modified)))
						).ToList();
		}

		public KeyType PropertyValueCurrent<KeyType>(string KeyName)
		{
			var version = this.LastOrDefault(item => item.ContainsKey(KeyName));
			if (version != null)
			{
				return version.GetValue<KeyType>(KeyName);
			}
			else
			{
				return default(KeyType);
			}
		}

		protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
		{
			base.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(propertyName));
		}

	}
}
