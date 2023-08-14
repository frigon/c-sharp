using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Spring.Json
{
	static class Utilities
	{
		public static JObject PatchObjectFromJObject<T>(JObject patchJSON, T POCObject)
		{

			// TODO: Implmement Array Handling
			Type targetType = POCObject.GetType();
			var sourcePOCO = patchJSON.ToObject(targetType);
			var targetPropInfos = targetType.GetProperties().Where(prop => prop.CanWrite).ToList();
			patchJSON.Properties().ToList().ForEach(jsonProp =>
			{
				var targetPropInfo = targetPropInfos.SingleOrDefault(pocoProp => pocoProp.Name == jsonProp.Name);
				if (targetPropInfo == null)
				{
					targetPropInfo = targetPropInfos.Where(pocoProp => pocoProp.GetCustomAttribute<JsonPropertyAttribute>() != null).SingleOrDefault(pocoProp => pocoProp.GetCustomAttribute<JsonPropertyAttribute>().PropertyName == jsonProp.Name);
				}
				if (targetPropInfo != null)
				{
					var updatedValue = targetPropInfo.GetValue(sourcePOCO);
					var existingValue = targetPropInfo.GetValue(POCObject);

					if (jsonProp.Value is JValue || jsonProp.Value.Type == JTokenType.Array)
					{
						if (updatedValue != existingValue)
						{
							targetPropInfo.SetValue(POCObject, updatedValue);
						}
						else
						{
							patchJSON.Remove(jsonProp.Name);
						}
					}
					else
					{
						if (jsonProp.Value is JObject jobj)
						{
							if(jobj.Count > 0)
							{
								if (existingValue == null)
								{
									var defaultInstance = Activator.CreateInstance(targetPropInfo.PropertyType);
									existingValue = defaultInstance;
									targetPropInfo.SetValue(POCObject, existingValue);
								}
								var results = PatchObjectFromJObject(jobj, existingValue);
								patchJSON[jsonProp.Name] = results;
							}
						}
						else
						{
							throw new NotImplementedException(jsonProp.Value.GetType().Name + " not implemented.");
						}
					}
				}
			});
			return patchJSON;
		}

		public static List<string> CopyProperties(object source, object destination, List<string> propNames)
		{
			var changes = new List<string>();
			propNames.ForEach(propName => {
				var targetPropInfo = destination.GetType().GetProperty(propName);
				var sourcePropInfo = source.GetType().GetProperty(propName);
				if(targetPropInfo != null && sourcePropInfo != null)
				{
					if (sourcePropInfo.PropertyType == targetPropInfo.PropertyType)
					{
						if (targetPropInfo.CanWrite && sourcePropInfo.CanRead)
						{
							var updatedValue = sourcePropInfo.GetValue(source);
							var targetValue = targetPropInfo.GetValue(destination);
							if (targetPropInfo.PropertyType != typeof(string) && targetPropInfo.PropertyType.IsClass || targetPropInfo.PropertyType.IsInterface)
							{
								var childProps = CopyProperties(targetValue, updatedValue, targetValue.GetType().GetProperties().Where(p => p.CanWrite).Select(p => p.Name).ToList());
								changes.AddRange(childProps);
							}
							if(!Object.Equals(updatedValue,targetValue))
							{
								changes.Add(targetPropInfo.Name);
								targetPropInfo.SetValue(destination, updatedValue);
							}
						}
					}
				}
			});
			return changes;
		}

		public static IEnumerable<JObject> ReduceJsonByIdentifier(this IEnumerable<JObject> list, string identifier)
		{
			return list.ToList()
				.GroupBy(json => json.Property(identifier).Value)
				.Select(kvp => kvp.Aggregate((jbo, next) => {
					jbo.Merge(next);
					return jbo;
				}));
		}
	}
}
