# Asp.Net Core Playback
An Asp.Net Core middleware library that simplifies the recording and playback of WebApi incoming requests and outgoing requests responses. Suitable for saving user interactions in production to be replayed in development.

## Two usage scenarios
1. Record incoming Api requests and outgoing Api responses in order to reproduce for testing or troubleshooting. 
2. Fake Api responses in order to quickly design your rest api interface . 

When the X-Playback-Mode header is set to Record the request is saved to a remote storage (remote blob or local file storage available for the moment) and then a X-Playback-Id reponse header is returned that should be used for replay.

To replay a recorded request set the X-Playback-Mode request header to Playback mode and X.Playback-Id request header to the value returned in the previous step.

When the X-Playback-Mode is set to Fake fake responses will be returned. The faked responses are codified in a fake factory class you should implement and explicitly register.

There is also the possibility to capture the responses of any outgoing Api request in order to test the Api in total isolation.
Use the IPlaybackContext interface into your outgoing service proxies ( by injecting the IPlaybackContext into the  constructors ). This interface provides methods for saving and replaying the responses from outgoing calls ( and correlate to the Api playback-id). 

## Use case 1 : has a developer I want to record api requests in order to be able to replay them.
 
 In you web api project install pmilet.Playback package: Install-Package pmilet.Playback -Version 1.0.6
 
 Configure your Startup class 
 
```csharp
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            
            services.AddPlayback(Configuration);

            ...
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "My API", Version = "v1" });
            });

        }
```

```csharp        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ...
            
            app.UseSwagger();
            app.UseSwaggerUI(c=> c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));

            app.UsePlayback();
      
            ...
        }
```

Configure playback storage settings. The default storage service is Azure Blob Storage.
A Storage connection string and container name should be provided. Add this section to theappsettings.json file:
 
 "PlaybackBlobStorage": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "ContainerName": "playback"
  }
  
  
To view the playback headers in swagger decorate your api method with the PlaybackSwaggerFilter
 
 ```csharp
        [HttpGet]
        [SwaggerOperation("Hello")]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public async Task<string> Get()
 ```

Test it:
1. Navigate to swagger UI
2. Set the X-Playback-Mode request header to Record and try-out
3. Copy the X-Playback-Id value
4. Set the X-Playback-Mode to Playback and try-out.
5. You should receive the same result has in step 2. 

## Use case 2 : has a developer I want to record my api requests and outgoing responses in order to be able to replay them.

Use the IPlaybackContext interface into your outgoing services proxy to record and replay outgoing responses:

```csharp
   public class MyServiceProxy
   {
        IPlaybackContext _playbackContext;
        public MyServiceProxy(IPlaybackContext context )
        {
            _playbackContext = context;
        }

        public async Task<MyServiceResponse> Execute( MyServiceRequest command)
        {
            var result =  new MyServiceResponse() {  Output = $"MyService received input: {command.Input}" };
            if (_playbackContext.IsRecord)
            {
                await _playbackContext.RecordResult<MyServiceResponse>(result);
            }
            else if ( _playbackContext.IsPlayback )
            {
                return await _playbackContext.PlaybackResult<MyServiceResponse>();
            }
            return result;
        }
    }
```

## Use case 3 : has a developer I want to fake my api responses in order to design my api contract quickly.
 
 ```csharp
    public void ConfigureServices(IServiceCollection services)
    {
        ...

            services.AddPlayback(Configuration, fakeFactory: new MyPlaybackFakeFactory());

        ...
    }
 ```
 
Implement your fake factory: for example in this example the when requesting the uri: /api/hello with GET verb a Hello Fake string is returned
       
 ```csharp
    public class MyPlaybackFakeFactory : FakeFactoryBase
    {
        public override void GenerateFakeResponse(HttpContext context)
        {
            switch (context.Request.Path.Value.ToLower())
            {
                case "/api/hello":
                    if (context.Request.Method == "POST")
                        GenerateFakeResponse<HelloRequest, string>(context, HelloPost);
                    else if (context.Request.Method == "GET")
                        GenerateFakeResponse<string, string>(context, HelloGet);
                    break;
                default:
                    break;
            }
        }
       
        private string HelloGet(string request)
        {
            return "Hello FAKE";
        }
```

Test it:
1. Navigate to swagger UI
2. Set X-Playback-Mode header to Fake and try-out
3. You should receive the faked response. 
