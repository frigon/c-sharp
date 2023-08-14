using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Spring.Authorization
{
    public static class Extensions
    {
        public static string GetUserId(this ClaimsIdentity user)
        {
            var sub = user.FindFirst("sub");
            if(sub != null)
            {
                return sub.Value;
            }
            sub = user.FindFirst(ClaimTypes.NameIdentifier);
            if (sub != null)
            {
                return sub.Value;
            }
            return user.Name;
        }        

        public static string GetUserId(this ClaimsPrincipal user)
        {
            return (user.Identity as ClaimsIdentity).GetUserId();
        }
    }
}

namespace Spring.Serialization
{

    public static class Extensions
    {


        public static JObject ToJson(this Exception ex)
        {
            var jobject = new Newtonsoft.Json.Linq.JObject();
            jobject.Add("message", ex.Message);
            var stack = ex.StackTrace.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Select(str => str.Replace("at", "").Trim()).Reverse();
            jobject.Add("stack", new JArray(stack));
            return jobject;
        }

        public static string ToJson<T>(this T obj, JsonSerializerSettings settings = null) where T : class
        {
            if(settings != null)
			{
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, settings);
			}
			else
			{
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            }
        }

        public static string ToBase64Json<T>(this T obj) where T : class
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(obj.ToJson()));
        }

        public static T FromBase64Json<T>(this string Base64String)
        {
            var bytes = Convert.FromBase64String(Base64String);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(System.Text.Encoding.UTF8.GetString(bytes));
        }

        public static StringContent ToJsonStringContent(this object obj, JsonSerializerSettings settings = null)
        {
            var content = new StringContent(obj.ToJson(settings), System.Text.Encoding.UTF8, "application/json");
            return content;
        }

        public static HttpResponseMessage ToJsonHttpResponseMessage(this object obj, JsonSerializerSettings setting = null)
        {
			try
			{
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = obj.ToJsonStringContent(setting)
                };
            }catch(Exception ex)
			{
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Text.Plain)
                };
            }
        }

        public static HttpResponseMessage ToJsonHttpResponseMessage(this object obj, JsonConverter converter)
		{
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(converter);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = obj.ToJsonStringContent(settings)
            };
        }

        public static HttpResponseMessage ToHttpResponseMessage(this HttpListenerException ex)
        {
            var response = new HttpResponseMessage((HttpStatusCode)ex.ErrorCode);
            response.Headers.Add("X-Error", ex.Message.Replace(Environment.NewLine, " "));
            return response;
        }

        public static HttpResponseMessage ToHttpResponseMessage(this Exception ex)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            response.Headers.Add("X-Error", ex.Message.Replace(Environment.NewLine, " "));
            return response;
        }

        public static TSubclass CastToSubClass<TBaseClass, TSubclass>(object parentItem) where TSubclass : TBaseClass
        {
            return JsonConvert.DeserializeObject<TSubclass>(JsonConvert.SerializeObject(parentItem));
        }

        public static TBaseClass CastToParentClass<TSubclass, TBaseClass>(object subClassItem) where TSubclass : TBaseClass
        {
            return JsonConvert.DeserializeObject<TBaseClass>(JsonConvert.SerializeObject(subClassItem));
        }

    }
}

namespace Spring.Identifiers
{

}
