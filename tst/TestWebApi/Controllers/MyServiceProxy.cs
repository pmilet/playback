using pmilet.Playback.Core;
using System.Net.Http;
using System.Threading.Tasks;

namespace TestWebApi.Controllers
{
    public class MyServiceRequest
    {
        public string Input { get; set; }
    }

    public class MyServiceResponse
    {
        public string Output { get; set; }
    }

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
}
