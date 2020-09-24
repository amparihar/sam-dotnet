using System.Threading.Tasks;

using Lambda.Models;

namespace User.Service
{

    public interface IUserService
    {

        Task<SignUpRequest> SignUp(SignUpRequest request);

        Task<SignInRequest> SignIn(SignInRequest request);

        Task DeleteTable();

    }
}