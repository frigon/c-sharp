using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spring.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Spring.Models
{

	public interface IPartialModel : INotifyPropertyChanged
	{
		bool ContainsKey(string field);
		int Count();
		T GetValue<T>(string key);
		List<string> Keys();
		List<string> Patch(IPartialModel patch);
		void RemoveKey(string field);
	}

	public class PartialModelConverter : Newtonsoft.Json.JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return true;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var token = JToken.Load(reader);
			var target = Activator.CreateInstance(objectType);
			serializer.Error += (sender, args) => {
				args.ErrorContext.Handled = true;
			};
			serializer.Populate(token.CreateReader(), target);
			return target;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var partial = value as PartialModel;
			var keys = partial.GetKeyValuePairs().ToList().Union(partial.ExtensionData.ToList()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			var jbo = new JObject();
			keys.ToList().ForEach(kvp => {
				if (kvp.Value == null)
				{
					jbo.Add(kvp.Key, JValue.CreateNull());
				}
				else
				{
					jbo.Add(kvp.Key, JToken.FromObject(kvp.Value));
				}
			});
			jbo.WriteTo(writer);
		}

	}

	[JsonConverter(typeof(PartialModelConverter))]
	public class PartialModel : INotifyPropertyChanged, IPartialModel
	{
		private Dictionary<string, object> _fields = new Dictionary<string, object>();
		private Dictionary<string, object> _extensionData = new Dictionary<string, object>();
		[JsonExtensionData]
		public Dictionary<string, object> ExtensionData { get => _extensionData; set => _extensionData = value; }

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string PropertyName = null, bool notifyChange = true)
		{
			if (!string.IsNullOrEmpty(PropertyName) && this.PropertyChanged != null && notifyChange)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
			}
		}

		public int Count()
		{
			return _fields.Count;
		}


		List<string> _patchChanges = new List<string>();
		private void _patchItemProperty(object sender, PropertyChangedEventArgs args)
		{
			_patchChanges.Add(args.PropertyName);
		}

		public List<string> Patch(IPartialModel patch)
		{
			var changes = new List<string>();
			if (patch.Count() > 0)
			{
				var thisInfo = this.GetType();
				var patchInfo = patch.GetType();
				this.PropertyChanged += _patchItemProperty;
				foreach (var prop in patch.Keys())
				{
					var thisProp = thisInfo.GetProperty(prop);
					var patchProp = patchInfo.GetProperty(prop);
					if (thisProp != null && patchProp != null)
					{
						thisProp.SetValue(this, patchProp.GetValue(patch));
					}
				}
				changes.AddRange(_patchChanges);
				_patchChanges.Clear();
				this.PropertyChanged -= _patchItemProperty;
			}
			return changes;
		}

		public bool ContainsKey(string field)
		{
			return _fields.ContainsKey(field);
		}

		public void RemoveKey(string field)
		{
			if (ContainsKey(field))
			{
				_fields.Remove(field);
			}
		}

		public List<string> Keys()
		{
			return _fields.Keys.ToList();
		}

		public T GetValue<T>(string key)
		{
			if (ContainsKey(key))
			{
				return (T)_fields[key];
			}
			else
			{
				return default(T);
			}
		}

		protected T Get<T>([CallerMemberName] string field = null)
		{
			if (!string.IsNullOrEmpty(field))
			{
				if (this.ContainsKey(field) && _fields[field] != null)
				{
					return (T)_fields[field];
				}
				else
				{
					return default(T);
				}
			}
			else
			{
				return default(T);
			}
		}

		protected void Set<T>(T value, bool notifyChange = true, [CallerMemberName] string field = null)
		{
			if (!string.IsNullOrEmpty(field))
			{
				if (this.ContainsKey(field))
				{
					var existingValue = this.Get<T>(field);
					if (!Object.Equals(existingValue, value))
					{
						_fields[field] = value;
						OnPropertyChanged(field, notifyChange);
					}
				}
				else
				{
					_fields.Add(field, value);
					OnPropertyChanged(field, notifyChange);
				}
			}
		}

		public Dictionary<string, object> GetKeyValuePairs()
		{
			return new Dictionary<string, object>(_fields);
		}
	}

	public abstract class IdPartialModel<T> : PartialModel, IIdVersionable<T> where T : IdPartialModel<T>, IIdVersionable<T>
	{
		public string id { get => Get<string>(); set => Set(value); }

		public abstract int CompareTo(T other);

	}

	public class AgendoBase<T> : IdPartialModel<T>, IAccountObject where T : AgendoBase<T>
	{
		public string accountId { get => Get<string>(); set => Set(value); }
		public Providers source { get => Get<Providers>(); set => Set(value); }
		public SourceType sourceType { get => Get<SourceType>(); set => Set(value); }
		public DateTimeOffset modified { get => Get<DateTimeOffset>(); set => Set(value); }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public TimeSpan? expiresIn { get => Get<TimeSpan?>(); set => Set(value); }
		public bool deleted { get => Get<bool>(); set => Set(value); }
		public override int CompareTo(T other)
		{
			if (other == null) return 1;
			return modified.CompareTo(other.modified);
		}

		public int CompareTo([AllowNull] ITimestamps other)
		{
			if (other == null) return 1;
			return modified.CompareTo(other.modified);
		}
	}
}
