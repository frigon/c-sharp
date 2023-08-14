using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Spring.Interfaces;
using System.Diagnostics.CodeAnalysis;
using Spring.Models;

namespace Spring.Framework.Comparisons
{
	public class ListChangeLog<T> where T : IdPartialModel<T>
	{
		public Dictionary<string, T> Inserts { get; set; } = new Dictionary<string, T>();
		public Dictionary<string, T> Deletes { get; set; } = new Dictionary<string, T>();
		public Dictionary<string, T> Updates { get; set; } = new Dictionary<string, T>();
	}

	public class IdEqualityCompare<T> : IEqualityComparer<T> where T : IIdVersionable<T>
	{
		public bool Equals(T x, T y)
		{
			if (x == null || y == null) return false;
			if (string.IsNullOrEmpty(x.id) || string.IsNullOrEmpty(y.id)) return false;
			return x.id == y.id;
		}

		public int GetHashCode(T obj)
		{
			if (obj == null || string.IsNullOrEmpty(obj.id)) return obj.GetHashCode();
			return obj.id.GetHashCode();
		}
	}

	public static class ListComparisons
	{
		public static ListChangeLog<T> Merge<T>(this IEnumerable<ListChangeLog<T>> items) where T : IdPartialModel<T>
		{
			return items.Aggregate(new ListChangeLog<T>(), (parent, current) => {
				current.Inserts.ToList().ForEach(kvp => parent.Inserts.Upsert(kvp.Key, kvp.Value));
				current.Updates.ToList().ForEach(kvp => parent.Updates.Upsert(kvp.Key, kvp.Value));
				current.Deletes.ToList().ForEach(kvp => parent.Deletes.Upsert(kvp.Key, kvp.Value));
				return parent;
			});
		}

		public static ListChangeLog<T> GetListChanges<T>(this List<T> updatedList, List<T> cachedList) where T : IdPartialModel<T>
		{
			try
			{
				var comparer = new IdEqualityCompare<T>();
				var liveMatches = updatedList.Intersect(cachedList, comparer).ToList();
				var cachedMatches = cachedList.Intersect(updatedList, comparer).ToList();
				var matchGroups = liveMatches.Union(cachedMatches)
										//NEWER ITEMS FIRST, OLDER ITEMS SECOND
										.OrderByDescending(item => item)
										.GroupBy(item => item.id)
										.ToDictionary(
												kvp => kvp.Key,
												kvp => kvp.First().GetObjectDeltas(kvp.Last())
															.ToObject<T>()
										);

				var chnages = new ListChangeLog<T>
				{
					Deletes = cachedList.Except(updatedList, comparer).ToDictionary(kvp => kvp.id, kvp => kvp),
					Inserts = updatedList.Except(cachedList, comparer).ToDictionary(kvp => kvp.id, kvp => kvp),
					Updates = matchGroups
				};
				return chnages;
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

	}

	public class JsonPathObject
	{
		public string path { get; set; }
		public JTokenType type { get; set; }
		public object value { get; set; }
	}

	public static class JsonUtilities
	{



		public static List<JsonPathObject> ToJsonPath(this JContainer jContainer)
		{
			return jContainer
						.Descendants().OfType<JValue>()
						.Select(jval => new JsonPathObject
						{
							path = jval.Path,
							type = jval.Type,
							value = jval.Value
						})
						.ToList();
		}

		public static JObject MergeAllObjects(this IEnumerable<JObject> jObjects) => jObjects.Aggregate(new JObject(), (parent, current) =>
		{
			parent.Merge(current);
			return parent;
		});

		public static JToken BuildTokenFromPath(this JsonPathObject path)
		{
			try
			{
				var dotPath = path.path
									//remove property bracket notation
									.Replace("['", ".")
									.Replace("']", ".")
									//remote array bracket notation
									.Replace("[", ".")
									.Replace("]", ".")
									//Remove duplicate dot
									.Split('.', StringSplitOptions.RemoveEmptyEntries).ToList();
				return BuildTokenFromPath(dotPath, path.value);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		public static JToken BuildTokenFromPath(List<string> pathParts, object value = null)
		{
			try
			{

				JToken parent = null;
				if (value == null)
				{
					parent = JValue.CreateNull();
				}
				else
				{
					parent = JToken.FromObject(value);
				}

				for (var i = pathParts.Count - 1; i >= 0; i--)
				{
					if (string.IsNullOrEmpty(pathParts[i]))
					{
						break;
					}
					else if (char.IsDigit(pathParts[i].First()))
					{
						//Array
						var intArray = int.Parse(pathParts[i]);
						var elements = Enumerable.Range(0, intArray + 1).Select(i => JValue.CreateNull()).Cast<JToken>().ToList();
						elements[elements.Count - 1] = parent;
						var array = new JArray(elements);
						parent = array;
					}
					else
					{
						//Property
						var job = new JObject();
						job.Add(pathParts[i], parent);
						parent = job;
					}
				}

				return parent;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}

	public static class ObjectComparisons
	{

		public class JsonPathIdentityComparer : IEqualityComparer<JsonPathObject>
		{
			public bool Equals(JsonPathObject x, JsonPathObject y)
			{
				if (x == null || y == null) return false;
				if (string.IsNullOrEmpty(x.path) || string.IsNullOrEmpty(y.path)) return false;
				return x.path == y.path;
			}

			public int GetHashCode([DisallowNull] JsonPathObject obj)
			{
				if (obj == null || string.IsNullOrEmpty(obj.path)) return base.GetHashCode();
				return obj.path.GetHashCode();
			}
		}

		public static JObject GetObjectDeltas<T>(this T left, T right) where T : IIdVersionable<T>
		{
			var comparer = new JsonPathIdentityComparer();

			var newer = left.CompareTo(right) >= 0 ? left : right;
			var older = left.CompareTo(right) > 0 ? right : left;
			
			var newerObject = JObject.FromObject(left).ToJsonPath();
			var olderObject = JObject.FromObject(right).ToJsonPath();

			var added = newerObject.Except(olderObject, comparer).ToList();
			//NEEDED UNCLEAR IF MISSING OBJECTS ARE DELETED IF PARTIAL OBJECTS
			var removed = olderObject.Except(newerObject, comparer).ToList();

			var updates = newerObject.Intersect(olderObject, comparer);

			return added.Union(updates).Select(item => item.BuildTokenFromPath())
						  .Aggregate(new JObject(), (parent, current) => {
							  parent.Merge(current);
							  return parent;
					});
		}
	}
}
