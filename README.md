# grpc_ientityserver
.net core grpc+identity server4集成认证授权
#### 前言
随着.net core3.0的正式发布，gRPC服务被集成到了VS2019。本文主要演示如何对gRPC的服务进行认证授权。

#### 分析
目前.net core使用最广的认证授权组件是基于OAuth2.0协议的IdentityServer4。而gRPC可以与ASP.NET Core Authentication一起使用来实现认证授权功能。本文将创建3个应用程序来完成gRPC的认证授权演示过程。  
程序名称 | 类型 | 说明
---|---|---
Ids4.Server | webapi程序 | 提供基于OAuth2.0的认证授权服务
Grpc.Server | grpc程序 | 提供gRPC服务的服务端
Grpc.Client | 控制台程序 | 访问gRPC服务的客户端

#### 步骤
###### Ids4.Server
1. 创建一个.net core的webapi
2. nuget引用最新的IdentityServer4的包
```
<PackageReference Include="IdentityServer4" Version="3.0.1" />
```
3. IdentityServer4相关配置，因为是演示所以很简单，生产场景大家根据实际情况配置。
```
namespace Ids4.Server
{
    public class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
            };
        }
        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource("api", "Demo API")
                {
                    ApiSecrets = { new Secret("secret".Sha256()) }
                }
            };
        }
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
                {
                    new Client
                    {
                        ClientId = "client",
                        ClientSecrets = { new Secret("secret".Sha256()) },

                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        AllowedScopes = { "api" },
                    },
                };
        }
    }
}
```
4. startup.cs 注入服务
```
services.AddIdentityServer().AddInMemoryApiResources(Config.GetApis())
    .AddInMemoryIdentityResources(Config.GetIdentityResources())
    .AddInMemoryClients(Config.GetClients())
    .AddDeveloperSigningCredential(persistKey: false); 

```
5. startup.cs 配置http请求管道
```
app.UseIdentityServer();
```
6. 启动服务，使用PostMan进行调试，有返回结果表示服务创建成功
```
POST /connect/token HTTP/1.1
Host: localhost:5000
Content-Type: application/x-www-form-urlencoded
grant_type=client_credentials&client_id=client1&client_secret=secret
```
![img](https://note.youdao.com/yws/public/resource/51668182e267e23dbaecd82736071282/xmlnote/24E7E8F61B414172A28B0D2F45E24974/10592)

![img](https://note.youdao.com/yws/public/resource/51668182e267e23dbaecd82736071282/xmlnote/3DA6C340502145F5B68012603EB3D033/10596)
```
{
    "access_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6IlVyMmxuM2EwNGhWaGdDdWZTVTNtZVEiLCJ0eXAiOiJhdCtqd3QifQ.eyJuYmYiOjE1NzEzMDkwMTMsImV4cCI6MTU3MTMxMjYxMywiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo1MDAwIiwiYXVkIjoiYXBpIiwiY2xpZW50X2lkIjoiY2xpZW50Iiwic2NvcGUiOlsiYXBpIl19.X4pg9_FbPbWZl814XC0NYWTslfhMG4aXWEyXLrXhIojPJaL7Qvq9ieDF4S7x0psRcClwbwCg81hTrG3j2Cmcl0nzj_Ic7UY8MfN0dvAuy_fJdUf76TX0oOpir3SxgC8gnfaKyEoWmmbIyvwicWbKp9PP-EeTxG6-oMYn6PO22cwRVHDD28ZdEAq2DEkATOh9XPavoi9vGZhPQ1nviKL1K6tcYUGXSQbhWI9ISEqnTHqMX1xA_gcDIAplGvquXmtXdgyTsRoGolEtzDAYVH4sGUb1SpYx2nc8bgl6Qw27fhe0Uy9MR70kQMcEkCTdXLivjYjkuI9_quUyJHzdi5KgnQ",
    "expires_in": 3600,
    "token_type": "Bearer",
    "scope": "api"
}
```

> 本篇不对IdentityServer4做更多的讲解，大家可以参考官方文档了解更多。

###### Grpc.Server
1. 使用vs2019创建gRPC服务端。
![img](https://note.youdao.com/yws/public/resource/51668182e267e23dbaecd82736071282/xmlnote/4C82A22476F14C33951534383EDF8583/10578)
2. 不用做任何更改，直接使用默认创建的gRPC服务


###### Grpc.Client
1. 创建一个控制台程序
2. 引入nuget安装包
```
<PackageReference Include="Google.Protobuf" Version="3.10.0" />
<PackageReference Include="Grpc.Net.Client" Version="2.23.2" />
<PackageReference Include="Grpc.Tools" Version="2.24.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```
> 这3个核心包是客户端必备的，其中grpc.tools帮我们把proto文件转化成C#代码。

3. 创建Protos文件夹
4. 复制Grpc.Server项目Protos文件夹下的greet.proto文件到本项目的Protos文件夹
5. greet.proto文件右键设置gGRC Stub Classes为Client only。
![img](https://note.youdao.com/yws/public/resource/51668182e267e23dbaecd82736071282/xmlnote/87A15C108F4B4FAEB36D8FF4B3F28B2B/10583) 
也可以直接使用在项目文件里面代码设置如下：
```
<ItemGroup>
  <Protobuf Include="Protos\greet.proto" GrpcServices="Client" />
</ItemGroup>
```
6. gRPC客户端访问服务端代码
```
var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = new Greeter.GreeterClient(channel);
var response = client.SayHello(new HelloRequest { Name = "World" });
Console.WriteLine(response.Message);
```
启动gRPC服务端，在启动gRPC客户端控制台打印hello word表示成功。
![img](https://note.youdao.com/yws/public/resource/51668182e267e23dbaecd82736071282/xmlnote/A346B2DB163C4106B0EE1BA0CED648BE/10573)
identityServer接入gRPC是非常容易，和传统webapi差不多。

###### 改造Grpc.Server支持IdentitySserver4
1. 引入nuget包
```
<PackageReference Include="IdentityServer4.AccessTokenValidation" Version="3.0.1" />
```
2. startup.cs 注入服务，和IdentityServer4一样。
```
services.AddGrpc(x => x.EnableDetailedErrors = false);
services.AddAuthorization();
services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
    .AddIdentityServerAuthentication(options =>
    {
        options.Authority = "http://localhost:5000";
        options.RequireHttpsMetadata = false;
    });
```
3. startup.cs 配置http请求管道
```
if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<GreeterService>();

    endpoints.MapGet("/", async context =>
    {
        await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
    });
});
```
4. 对需要授权的服务打标签[Authorize]，可以打在类上也可以打在方法上
```
[Authorize]
public class GreeterService : Greeter.GreeterBase
{
}
```
这个时候我们启动Grpc.Client访问Grpc.Server服务
![image](https://note.youdao.com/yws/public/resource/51668182e267e23dbaecd82736071282/xmlnote/CE990D8652C34AFAA204866DA89BF1B8/10557)
发现报错401。说明此服务需要携带令牌才能访问。
###### 改造Grpc.Client携带令牌访问
```
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
```
执行程序返回hello world表示成功。
> 传统调用webapi把token放到Header头的Authorization属性里面，grpc是放到Metadata里面，调用方法的时候传入CallOptions。使用上也大同小异。

#### 后记
目前gRPC各个语言的支持都已经很完善，因为跨语言，性能更高的特性非常适合做内网的通信。笔者也将继续对gRPC进行跟进，会尝试将部分的内部服务暴露成gRPC供内网访问。  
本文项目地址：[github](https://github.com/longxianghui/grpc_ientityserver)

