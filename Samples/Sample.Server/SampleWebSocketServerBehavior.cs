using InterLinq.Communication;
using InterLinq.JsonNet;
using Newtonsoft.Json;
using Sample.Data;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Sample.Server
{
    class SampleWebSocketServerBehavior : WebSocketBehavior
    {
        private ServerQueryHandler _handler;

        private static readonly JsonSerializerSettings JsonNetSettingsSerializer = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, PreserveReferencesHandling = PreserveReferencesHandling.Objects };
        private static readonly JsonSerializerSettings JsonNetSettingsDeserializer = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, PreserveReferencesHandling = PreserveReferencesHandling.Objects };

        static SampleWebSocketServerBehavior()
        {
            JsonNetSettingsSerializer.Converters.Add(new InterlinqJsonConverter());
            JsonNetSettingsDeserializer.ContractResolver = new InterLinqQueryContractResolver();
        }

        public SampleWebSocketServerBehavior()
        {
            var sampleQueryHandler = new SampleQueryHandler();
            this._handler = new ServerQueryHandler(sampleQueryHandler);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (!e.IsPing)
            {
                var request = JsonConvert.DeserializeObject<WebsocketRequest>(e.Data, JsonNetSettingsDeserializer);
                var result = _handler.Retrieve(request.Expression);
                var response = new WebsocketResult() { Id = request.Id, Data = result };
                var serializedResponse = JsonConvert.SerializeObject(response, JsonNetSettingsSerializer);
                this.Send(serializedResponse);
            }
        }
    }
}
