using System.Threading.Tasks;

namespace RunNow.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string username, string password);
        Task<bool> FaceLoginAsync();
    }
}
