# Asp.Net Core Playback
An Asp.Net Core middleware library that simplifies the recording and playback of WebApi calls. Suitable for saving user interactions in production to be replayed in development.

### There is two usage scenarios:
1. Record your incoming Api requests and incoming responses (from outgoing requests) . 
2. Fake the responses of your Api .

Once your Asp.NetCore Api is configured for playback ( see sample in github repo ) you can start recording and replaying Api requests 

When the X-Playback-Mode request header is set to Record the request will be saved.

```javascript
curl -X GET --header 'Accept: text/plain' --header 'X-Playback-Mode: Record' 'http://apigatewaysample.azurewebsites.net/api/Hello/hello'

then a  x-playback-id response header should be received. 

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

When the X-Playback-Mode request header is set to Playback the request will be replayed; you should also set the x-playback-id request header.

```javascript
curl -X GET --header 'Accept: text/plain' --header 'X-Playback-Id: _ApiGateway+Sample_v1.0_Hello%252Fhello_GET_757602046' --header 'X-Playback-Mode: Playback' 'http://apigatewaysample.azurewebsites.net/api/Hello/bye'
```

Notice that the response is exactly the same has before.

When setting the x-playback-mode to None the request is not saved neither replayed. 

