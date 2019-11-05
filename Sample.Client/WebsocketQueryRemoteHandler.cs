using InterLinq;
using InterLinq.Expressions;
using InterLinq.JsonNet;
using Newtonsoft.Json;
using Sample.Data;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Sample.Client
{
    public class WebsocketQueryRemoteHandler : IQueryRemoteHandler
    {
        private WebSocket _webSocket;

        private ConcurrentDictionary<Guid, TaskCompletionSource<object>> _requests = new ConcurrentDictionary<Guid, TaskCompletionSource<object>>();

        private static readonly JsonSerializerSettings JsonNetSettingsSerializer = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, PreserveReferencesHandling = PreserveReferencesHandling.Objects };
        private static readonly JsonSerializerSettings JsonNetSettingsDeserializer = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, PreserveReferencesHandling = PreserveReferencesHandling.Objects };

        static WebsocketQueryRemoteHandler()
        {
            JsonNetSettingsSerializer.Converters.Add(new InterlinqJsonConverter());
            JsonNetSettingsDeserializer.ContractResolver = new InterLinqQueryContractResolver();
        }

        public WebsocketQueryRemoteHandler(string url)
        {
            _webSocket = new WebSocket(url);
            _webSocket.WaitTime = TimeSpan.FromSeconds(60);
            _webSocket.WriteTimeout = TimeSpan.FromMinutes(5).Milliseconds;
            _webSocket.ReadTimeout = TimeSpan.FromMinutes(5).Milliseconds;
            _webSocket.OnMessage += _webSocket_OnMessage;
            _webSocket.EmitOnPing = true;
            _webSocket.Connect();
        }

        private void _webSocket_OnMessage(object sender, MessageEventArgs e)
        {
            if (!e.IsPing)
            {
                var result = JsonConvert.DeserializeObject<WebsocketResult>(e.Data, JsonNetSettingsDeserializer);
                if (_requests.TryRemove(result.Id, out var tcs))
                {
                    tcs.SetResult(result.Data);
                }
            }
        }

        public object Retrieve(SerializableExpression expression)
        {
            var request = new WebsocketRequest() { Id = Guid.NewGuid(), Expression = expression };
            var tcs = new TaskCompletionSource<object>();
            _requests.TryAdd(request.Id, tcs);
            var serializedRequest = JsonConvert.SerializeObject(request, JsonNetSettingsSerializer);
            _webSocket.Send(serializedRequest);
            return tcs.Task.Result;
        }
    }
}
