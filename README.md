# Asp.Net Core Playback
an Asp.Net Core middleware library that simplifies the recording and playback of HTTP requests and responses. Suitable for reproducing user interactions in automated tests suites or for reproducing production issues in your development environment.

## Two usage scenarios
1. Record  Api incoming requests and ougoing responses to reproduce them in order to test you Api code in isolation. 
2. Fake Api responses by implementing and registering a simple Fake Factory class in order to design your Api interface in quick iterations. 

For example we could save all our user Api interactions in production and replay them in a local environment to quickly troubleshoot issues.

When the X-Playback-Mode header is set to Record the request is saved to a remote storage ( only blob storage available for the moment) and then a X-Playback-Id reponse header is returned to be used for replay.

To replay a recorded request set the X-Playback-Mode request header to Playback and X.Playback-Id request header to the value returned in the record step.

When the X-Playback-Mode is set to Fake fake responses can be returned. The faked responses are codified in a fake factory class you should implement and register.

There is also the possibility to capture any Api outgoing request responses in order to test the Api in total isolation.
For that you could use the IPlaybackContext interface into your outgoing service proxies ( by injecting the IPlaybackContext into their  constructors ). This interface provides methods for saving and replaying the outgoing call responses and associate it to a Api playbackid. 

## Use case 1 : has a developer i want to record api requests in order to be able to replay them
 using a simple playback identifier.
 
 In you web api project add a reference to nuget package pmilet.Playback
 
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

Configure playback storage settings. The only supported playback storage service (for the moment) is Azure Blob Storage.
A Storage connection string and container name should be provided.
Add this section to appsettings.json file
 
 "PlaybackStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=ijewels;AccountKey=gB0hTWJoD+QZ4Wmipn1cZjt9vKqZJ9bABy7z/zDBDT3Dgojr2sMzRgGDW/sGa5CG//Ah4O7saJClGSWH/7VgIg==;EndpointSuffix=core.windows.net",
    "Name": "playback"
  }
  
 Decorate your api method with the PlaybackSwaggerFilter
 
 ```csharp
        [HttpGet]
        [SwaggerOperation("Hello")]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public async Task<string> Get()
 ```

Test it:
1. Navigate to your swagger UI and select and api method to execute
2. Choose X-Playback-Mode = Record and try-out
3. Copy the X-Playback-Id value
4. Choose X-Playback-Mode = Playback and try-out
5. you should receive the same result has in step 2. 

## Use case 2 : has a developer i want to fake my api responses in order to design my api contract quickly.
 
 ```csharp
    public void ConfigureServices(IServiceCollection services)
    {
        ...

        services.AddFakeFactory<MyPlaybackFakeFactory>();

        ...
    }
 ```
 
Implement your fake factory for example like this...
       
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
1. Navigate to your swagger UI and select and api method to execute
2. Choose X-Playback-Mode = Fake and try-out
5. you should receive the faked result codified in your Fake Factory class. 


## Use case 3 : has a developer i want to record my api requests and also the outgoing responses in order to be able to replay them
 using a simple playback identifier.

Implement your service proxy leveraging the IPlaybackContext interface to record and replay your service outgoing responses:

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
