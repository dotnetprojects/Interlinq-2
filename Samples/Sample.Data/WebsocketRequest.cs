using InterLinq.Expressions;
using System;

namespace Sample.Data
{
    public class WebsocketRequest
    {
        public Guid Id { get; set; }

        public SerializableExpression Expression { get; set; }
    }
}
