using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;

namespace InterLinq.JsonNet
{
    public class InterLinqQueryContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(InterLinqQuery<>))
            {
                var contract = this.CreateObjectContract(objectType);

                contract.OverrideCreator = args => FormatterServices.GetUninitializedObject(objectType);

                return contract;
            }

            return base.CreateContract(objectType);
        }
    }
}
