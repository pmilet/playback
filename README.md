# Asp.Net Core Playback
An Asp.Net Core middleware library for recording and replay http requests (inbound and outbound).

### Purpose
Record your Web api incoming (your API received calls) and outgoing (calls from your API to external dependencies) http requests for later replay in a dev / testing environment.
Useful for unit testing, and or regresion testing your API.

### How to Setup

In your Startup class:

```cs
using pmilet.Playback;

...

public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ...
            
            services.AddPlayback(Configuration);
            
            ...
            
            //don't forget to return the service provider
            return services.BuildServiceProvider();

         }
 
 public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ...
            
            app.UsePlayback();
          
            ...
        }
      
 ...
            
```

In your appsetings.json file:

Add playback storage section that points to the storage (blob, or file) where the requests and responses will be saved:

Sample configuration for using a blob storage:
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

To replay a previously recorded request, set the client request X-Playback-Mode header to Playback and the X-Playback-Id header with the playbackid value received from the recording response.

```javascript
curl -X GET --header 'Accept: text/plain' --header 'X-Playback-Id: _ApiGateway+Sample_v1.0_Hello%252Fhello_GET_757602046' --header 'X-Playback-Mode: Playback' 'http://apigatewaysample.azurewebsites.net/api/Hello/bye'
```

When setting the x-playback-mode to None the playback functionality is bypassed. 

### PlaybackId format
The returned playbackid header is composed of differents parts each one carrying important context information. 
Each playbackid part is separated by an underscore : 

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

### How to record responses received from outbound requests

For replaying responses from outgoing requests you should use the HttpClientFactory.
this code excerpt show how you:

imagine you have a service proxy to call to an external http service (postman-echo) :
```cs
 public class MyServiceProxy : IServiceProxy
    {
        public HttpClient HttpClient { get; protected set; }

        public MyServiceProxy(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }
        public async Task<MyServiceResponse> Execute( MyServiceRequest command)
        {
            var requestUri = $"https://postman-echo.com/get?foo1={command.Input}&foo2={command.Input}";
            var r = await HttpClient.GetAsync(requestUri);
            var content = await r.Content.ReadAsStringAsync();
            return new MyServiceResponse() { Output = content };
        }
    }
''
by overriding this proxy, you can inject a playback specific handler that will be able to record and replay outgoing calls by refering to the playback context and playbackstorage service

```cs
public class MyPlaybackProxy : MyServiceProxy, IServiceProxy
    {
        private const string PROXY_NAME = "MyServiceProxy";
        readonly IPlaybackContext _playbackContext;
        readonly IPlaybackStorageService _playbackStorageService;
        private readonly IHttpClientPlaybackErrorSimulationService _configService;

        public MyPlaybackProxy(IPlaybackContext playbackContext, IPlaybackStorageService playbackStorageService, IHttpClientPlaybackErrorSimulationService configService) :
            base(new System.Net.Http.HttpClient())
        {
            _playbackContext = playbackContext;
            _playbackStorageService = playbackStorageService;
            _configService = configService;
            base.HttpClient = HttpClientFactory.WithPlaybackContext(playbackContext, playbackStorageService, PROXY_NAME, configService);
        }
    }
''
 




