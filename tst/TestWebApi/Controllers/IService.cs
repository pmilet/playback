using System.Threading.Tasks;

namespace TestWebApi.Controllers
{
    public interface IService
    {
        Task<MyServiceResponse> Execute(MyServiceRequest command);
    }
}