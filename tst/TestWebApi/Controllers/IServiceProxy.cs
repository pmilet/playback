using System.Threading.Tasks;

namespace TestWebApi.Controllers
{
    public interface IServiceProxy
    {
        Task<MyServiceResponse> Execute(MyServiceRequest command);
    }
}