//Copyright © DMPortella.  All Rights Reserved.
//This code released under the terms of the 
//Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)

//Copyright © DMPortella.  All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InterLinq.Tests.Server.Model
{
    internal sealed class MockService : IQueryRemoteHandler
    {
        private Communication.ServerQueryHandler serverQueryHandler;

        public MockService()
        {
            this.serverQueryHandler = new Communication.ServerQueryHandler(new MockQueryHandler());
        }

        class aa : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var q = value as InterLinqQueryBase;
                writer.WriteStartObject();
                writer.WritePropertyName("$type");
                writer.WriteValue(q.GetType().FullName);
                writer.WritePropertyName("A");
                writer.WriteValue(q.QueryName);
                writer.WritePropertyName("B");
               serializer.Serialize(writer,q.AdditionalObject);
                writer.WritePropertyName("C");
                serializer.Serialize(writer, q.QueryParameters);
                writer.WritePropertyName("D");
                serializer.Serialize(writer, q.Parameters);
                writer.WriteEndObject();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return null;
            }

            public override bool CanConvert(Type objectType)
            {
                if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(InterLinqQuery<>))
                {
                    return true;
                }
                return false;
            }
        }
        public object Retrieve(Expressions.SerializableExpression expression)
        {
            var s = new JsonSerializerSettings() {TypeNameHandling = TypeNameHandling.All};
            s.Converters.Add(new aa());
            var aa = JsonConvert.SerializeObject(expression, s);
            var q= JsonConvert.DeserializeObject(aa,s);
            return this.serverQueryHandler.Retrieve(expression);
        }
    }
}
