# ASP.NET Core Playback

An ASP.NET Core middleware library for recording and replaying HTTP requests (inbound and outbound).

## Requirements
- .NET 9.0 or later

## Purpose
Record your Web API incoming (your API receives calls) and outgoing (calls from your API to external dependencies) HTTP requests for later replay in a dev/testing environment.
Useful for unit testing, integration testing, and regression testing your API.

## How to Setup

### For .NET 9+ (Minimal Hosting Model)

In your `Program.cs`:

```csharp
using pmilet.Playback;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddPlayback(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UsePlayback();
app.MapControllers();

app.Run();
```

### For Legacy Startup Class (compatibility)

In your Startup class:

```csharp
using pmilet.Playback;

public void ConfigureServices(IServiceCollection services)
{
    services.AddPlayback(Configuration);
}
 
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    app.UsePlayback();
    // ... other middleware
}
```

## Configuration

In your `appsettings.json` file:

Add a playback storage section that points to the storage (blob or file) where the requests and responses will be saved:

### Sample configuration for using Azure Blob Storage:
```json
{
  "PlaybackStorage": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "ContainerName": "playback"
  },
```

Sample configuration for using local file system:
```json
  
   "PlaybackStorage": {
    "ConnectionString": "Local",
    "ContainerName": "PlaybackFiles" 
  },
```

In your controllers:

if using swagger, decorate your controller for swagger to generate playback headers in swagger UI  

```cs
using pmilet.Playback;

  ...

  [HttpGet]
  [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
  public async Task<string> Get()
  
  ...
  
```

###  How to record and replay incoming Api requests?

Once your Asp.NetCore Api is configured for playback ( see above section or refer to sample in github repo ) you can start recording your api requests by setting the client request X-Playback-Mode header value to Record. 

```javascript
curl -X GET --header 'Accept: text/plain' --header 'X-Playback-Mode: Record' 'http://apigatewaysample.azurewebsites.net/api/Hello/hello'
```

then a  x-playback-id response header will be returned. 

```javascript
Response Headers
{
  "date": "Wed, 25 Oct 2017 21:05:46 GMT",
  "content-encoding": "gzip",
  "server": "Kestrel",
  "x-powered-by": "ASP.NET",
  "vary": "Accept-Encoding",
  "content-type": "text/plain; charset=utf-8",
  "transfer-encoding": "chunked",
  "x-playback-id": "_ApiGateway+Sample_v1.0_Hello%252Fhello_GET_757602046"
}
```

To replay a previously recorded request, set the client request `X-Playback-Mode` header to `Playback` and the `X-Playback-Id` header with the playback ID value received from the recording response.

```javascript
curl -X GET --header 'Accept: text/plain' --header 'X-Playback-Id: _ApiGateway+Sample_v1.0_Hello%252Fhello_GET_757602046' --header 'X-Playback-Mode: Playback' 'http://apigatewaysample.azurewebsites.net/api/Hello/bye'
```

When setting the `X-Playback-Mode` to `None`, the playback functionality is bypassed.

## PlaybackId Format

The returned playback ID header is composed of different parts, each carrying important context information. 
Each playback ID part is separated by an underscore: 

PlaybackContextInfo_ApiName_PlaybackVersion_RequestPath_RequestMethod_RequestContextHash
  
  - The PlayContextInfo comes from the X-Playback-RequestContext header.
  - The ApiName is the web api Name. 
  - The PlaybackVersion comes from the X-Playback-Version header.
  - The RequestPath is the request path url encoded
  - The RequestMethod is the request http verb
  - The RequestContextHash is a hash of the request payload in order to univoquely indentify each different request.
  
For example this playbackid  DemoUser_ApiGateway+Sample_v1.0_Hello%252Fhello_GET_757602046 can be descompsed as:
  - PlayContextInfo = DemoUser
  - AssemblyName = ApiGateway+Sample
  - PlaybackVersion = v1.0
  - RequestPath = Hello%252Fhello
  - RequestMethod = GET
  - RequestContextHash = 757602046

## How to Record Responses from Outbound Requests

For replaying responses from outgoing requests, you should use the `HttpClientFactory`.

### Example Service Proxy

Imagine you have a service proxy to call an external HTTP service (postman-echo):

```csharp
public class MyServiceProxy : IServiceProxy
{
    public HttpClient HttpClient { get; protected set; }

    public MyServiceProxy(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }
    
    public async Task<MyServiceResponse> Execute(MyServiceRequest command)
    {
        var requestUri = $"https://postman-echo.com/get?foo1={command.Input}&foo2={command.Input}";
        var r = await HttpClient.GetAsync(requestUri);
        var content = await r.Content.ReadAsStringAsync();
        return new MyServiceResponse() { Output = content };
    }
}
```

By overriding this proxy, you can inject a playback-specific handler that will be able to record and replay outgoing calls by referring to the playback context and playback storage service:

```csharp
public class MyPlaybackProxy : MyServiceProxy, IServiceProxy
{
    private const string PROXY_NAME = "MyServiceProxy";
    readonly IPlaybackContext _playbackContext;
    readonly IPlaybackStorageService _playbackStorageService;
    private readonly IHttpClientPlaybackErrorSimulationService _configService;

    public MyPlaybackProxy(
        IPlaybackContext playbackContext, 
        IPlaybackStorageService playbackStorageService, 
        IHttpClientPlaybackErrorSimulationService configService) 
        : base(new HttpClient())
    {
        _playbackContext = playbackContext;
        _playbackStorageService = playbackStorageService;
        _configService = configService;
        base.HttpClient = HttpClientFactory.WithPlaybackContext(
            playbackContext, 
            playbackStorageService, 
            PROXY_NAME, 
            configService);
    }
}
```

## Migration from .NET 6 to .NET 9

This library has been modernized to .NET 9 with the following changes:
- Upgraded from WindowsAzure.Storage to Azure.Storage.Blobs (v12+)
- Removed obsolete API usage
- Fixed nullable reference type warnings
- Improved error handling

If you're upgrading from an older version, ensure your Azure Storage connection strings are compatible with the new Azure.Storage.Blobs SDK.
 




