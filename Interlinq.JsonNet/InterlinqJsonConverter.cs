using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InterLinq.JsonNet
{
    public class InterlinqJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var q = value as InterLinqQueryBase;
            writer.WriteStartObject();
            writer.WritePropertyName("$type");
            writer.WriteValue(q.GetType().FullName + ", InterLinq");
            writer.WritePropertyName("QueryName");
            writer.WriteValue(q.QueryName);

            if (q.AdditionalObject != null)
            {
                if (q.AdditionalObject is string)
                {
                    writer.WritePropertyName("AdditionalObject");
                    writer.WriteValue(q.AdditionalObject);
                }
                else
                {
                    writer.WritePropertyName("AdditionalObject");
                    var jObject = JObject.FromObject(q.AdditionalObject);
                    jObject.AddFirst(new JProperty("$type",
                        q.AdditionalObject.GetType().FullName + ", " +
                        q.AdditionalObject.GetType().Assembly.GetName().Name));
                    jObject.WriteTo(writer);
                }
            }

            writer.WritePropertyName("QueryParameters");
            serializer.Serialize(writer, q.QueryParameters);
            writer.WritePropertyName("Parameters");
            serializer.Serialize(writer, q.Parameters);
            writer.WritePropertyName("ElementInterLinqType");
            serializer.Serialize(writer, q.ElementInterLinqType);
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
}
