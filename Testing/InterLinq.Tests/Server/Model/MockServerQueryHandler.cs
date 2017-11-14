//Copyright © DMPortella.  All Rights Reserved.
//This code released under the terms of the 
//Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)

//Copyright © DMPortella.  All Rights Reserved.
using System;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using InterLinq;
using InterLinq.Expressions;
using InterLinq.JsonNet;

namespace InterLinq.Tests.Server.Model
{
    internal sealed class MockService : IQueryRemoteHandler
    {
        private Communication.ServerQueryHandler serverQueryHandler;

        public MockService()
        {
            this.serverQueryHandler = new Communication.ServerQueryHandler(new MockQueryHandler());
        }

        class test
        {
            public Object Expression { get; set; }
        }
        public object Retrieve(Expressions.SerializableExpression expression)
        {
            var s = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, PreserveReferencesHandling = PreserveReferencesHandling.Objects };
            s.Converters.Add(new InterlinqJsonConverter());
            s.ContractResolver = new InterLinqQueryContractResolver();
            var aa = JsonConvert.SerializeObject(new test {Expression = expression}, s);
            var q = JsonConvert.DeserializeObject<test>(aa, s);
            return this.serverQueryHandler.Retrieve((SerializableExpression)q.Expression);
        }
    }
}
