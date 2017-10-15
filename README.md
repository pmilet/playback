# Asp.Net Core Playback
an Asp.Net Core middleware library that simplifies the recording and playback of HTTP requests and responses. Suitable for reproducing user interactions in automated tests suites or for reproducing production issues in your development environment.

## The mechanics
The mechanics are quite simple: record incoming requests and ougoing responses in order to be able to reproduce them and test you Api code. 

For example save your user Api interactions in production environment and replay them in local environment.
Another scenario is the ability to fake  your api responses in order to iteratively design  your Api  

When the X-Playback-Mode is set to Record all the requests are saved to a storage ( only blob storage available for the moment) and a reponse header X-Playback-Id is returned.
To replay you recorded request you just need to set the X-Playback-Mode to Playback and X.Playback-Id to the value previously returned.

When the X-Playback-Mode is set to Fake all the responses can be faked by registering a playback factory class where you can easily fake all the responses.

You can also capture any outgoing request responses in order to test your Api code in total isolation ( for example in local dev environment). For that your outgoing service proxies can inject the IPlayackContext object into their constructor to use it for the recording and replay of any response type. 

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
        
'''csharp        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ...
            
            app.UseSwagger();
            app.UseSwaggerUI(c=> c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));

            app.UsePlayback();
      
            ...
        }
        
Configure playback storage settings. The only supported playback storage service (for the moment) is Azure Blob Storage.
A Storage connection string and container name should be provided.
Add this section to appsettings.json file
 
 "PlaybackStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=ijewels;AccountKey=gB0hTWJoD+QZ4Wmipn1cZjt9vKqZJ9bABy7z/zDBDT3Dgojr2sMzRgGDW/sGa5CG//Ah4O7saJClGSWH/7VgIg==;EndpointSuffix=core.windows.net",
    "Name": "playback"
  }
  
 Decorate your api method with the PlaybackSwaggerFilter
 
 '''csharp
  [HttpGet]
        [SwaggerOperation("Hello")]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public async Task<string> Get()

Test it:
1. Navigate to your swagger UI and select and api method to execute
2. Choose X-Playback-Mode = Record and try-out
3. Copy the X-Playback-Id value
4. Choose X-Playback-Mode = Playback and try-out
5. you should receive the same result has in step 2. 

## Use case 2 : has a developer i want to fake my api responses in order to design my api contract quickly.
 
 '''csharp
    public void ConfigureServices(IServiceCollection services)
    {
        ...

        services.AddFakeFactory<MyPlaybackFakeFactory>();

        ...
    }
        
Implement your fake factory for example like this...
       
 '''csharp
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

Test it:
1. Navigate to your swagger UI and select and api method to execute
2. Choose X-Playback-Mode = Fake and try-out
5. you should receive the faked result codified in your Fake Factory class. 


## Use case 3 : has a developer i want to record my api requests and also the outgoing responses in order to be able to replay them
 using a simple playback identifier.

Implement your service proxy leveraging the IPlaybackContext interface to record and replay your service outgoing responses:

'''csharp
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
