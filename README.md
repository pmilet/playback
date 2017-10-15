# Asp.Net Core Playback
an Asp.Net Core middleware library that simplifies the recording and playback of HTTP requests and responses. Suitable for reproducing user interactions in automated tests suites or for reproducing production issues in your development environment.

## Use case 1 : has a developer i want to record my api requests in order to be able to replay them
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
  
1. Navigate to your swagger UI and select and api method to execute
2. Choose X-Playback-Mode = Record and try-out
3. Copy the X-Playback-Id value
4. Choose X-Playback-Mode = Playback and try-out
5. you should receive the same result has in step 2. 

## Use case 2 : has a developer i want to fake my api responses in order to design my api contract quickly.
 
## Use case 3 : has a developer i want to record my api requests and outgoing responses in order to be able to replay them
 using a simple playback identifier.


