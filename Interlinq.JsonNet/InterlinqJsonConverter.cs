using System;
using Newtonsoft.Json;

namespace InterLinq.JsonNet
{
    public class InterlinqJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var q = value as InterLinq.InterLinqQueryBase;
            writer.WriteStartObject();
            writer.WritePropertyName("$type");
            writer.WriteValue(q.GetType().FullName);
            writer.WritePropertyName("QueryName");
            writer.WriteValue(q.QueryName);
            writer.WritePropertyName("AdditionalObject");
            serializer.Serialize(writer, q.AdditionalObject);
            writer.WritePropertyName("QueryParameters");
            serializer.Serialize(writer, q.QueryParameters);
            writer.WritePropertyName("Parameters");
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
}
