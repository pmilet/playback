// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using pmilet.Playback.Core;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace pmilet.Playback
{
    public abstract class FakeFactoryBase : IFakeFactory
    {
        protected bool GenerateFakeResponse<TRequest, TResponse>(HttpContext context, Func<TRequest, TResponse> func, Encoding encoding = null)
        {
            dynamic request = context.Request.Method != "GET" ? Deserialize<TRequest>(context.Request.Body) : GetFromQueryString(context, typeof(TRequest));
            var response = func(request);
            Stream fakeResponseStream = Serialize<TResponse>(response, encoding);
            fakeResponseStream.CopyToAsync(context.Response.Body);
            return true;
        }

        protected T Deserialize<T>(Stream body)
        {
            using (StreamReader sr = new StreamReader(body))
            {
                string bodyString = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(bodyString);
            }
        }

        protected Stream Serialize<T>(T body, Encoding encoding)
        {
            var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            string serializedBody = JsonConvert.SerializeObject(body, jsonSerializerSettings);
            encoding = encoding ?? Encoding.UTF8;
            var bytes = encoding.GetBytes(serializedBody);
            MemoryStream m = new MemoryStream(bytes);
            return m;
        }

        public object GetFromQueryString(HttpContext context, Type type)
        {
            if (type.Namespace == "System")
            {
                var key = context.Request.QueryString.HasValue ? context.Request.QueryString.Value.Replace("?","").Split('=')[0]  : "";
                return string.IsNullOrEmpty(key) ? type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null : context.Request.Query[key].ToString();
            }

            var obj = Activator.CreateInstance(type);
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var valueAsString = context.Request.Query[property.Name];
                object value = Parse(property.PropertyType, valueAsString);
                if (value == null)
                    continue;
                property.SetValue(obj, value, null);
            }
            return obj;
        }

        public object Parse(Type dataType, string ValueToConvert)
        {
            TypeConverter obj = TypeDescriptor.GetConverter(dataType);
            return obj.ConvertFromString(null, CultureInfo.InvariantCulture, ValueToConvert);
        }

        public abstract bool GenerateFakeResponse(HttpContext context);
    }
}
