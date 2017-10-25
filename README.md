# Asp.Net Core Playback
An Asp.Net Core middleware library that simplifies the recording and playback of WebApi calls. Suitable for saving user interactions in production to be replayed in development.

## Two usage scenarios
1. Record your incoming Api requests and incoming responses (from outgoing requests) . 
2. Fake the responses of your Api .

Once your Asp.NetCore Api has been configured for playback ( see code samples ) you can start recording your Api requests 

When the X-Playback-Mode is set to record the request will be saved (you will have to configure your repository in appsettings.json). 

```javascript
curl -X GET --header 'Accept: text/plain' --header 'X-Playback-Mode: Record' 'http://localhost/api/Hello/11
```

And a x-playback-id response header is received

```javascript
Response Headers
{
  "date": "Wed, 25 Oct 2017 20:46:47 GMT",
  "content-encoding": "gzip",
  "server": "Kestrel",
  "x-powered-by": "ASP.NET",
  "vary": "Accept-Encoding",
  "content-type": "text/plain; charset=utf-8",
  "transfer-encoding": "chunked",
  "x-playback-id": "_ApiGateway+Sample_v1.0_Hello%252F111_GET_757602046"
}
```
