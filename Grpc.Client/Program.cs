using System;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Server;
using IdentityModel.Client;

namespace Grpc.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //获取token可以直接使用HttpClient来获取，这里使用IdentityModel来获取token
            var httpClient = new HttpClient();
            var disco = await httpClient.GetDiscoveryDocumentAsync("http://localhost:5000");
            if (!disco.IsError)
            {
                var token = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest()
                {
                    Address = disco.TokenEndpoint,
                    ClientId = "client",
                    ClientSecret = "secret"
                });
                var tokenValue = "Bearer " + token.AccessToken;
                var metadata = new Metadata
                {
                    { "Authorization", tokenValue }
                };
                var callOptions = new CallOptions(metadata);
                var channel = GrpcChannel.ForAddress("https://localhost:5001");
                var client = new Greeter.GreeterClient(channel);
                var response = client.SayHello(new HelloRequest { Name = "World" }, callOptions);
                Console.WriteLine(response.Message);
            }
        }
    }
}
