
using System.Threading.Tasks;

using Lambda.Models;

namespace SignUp.Service
{
    public interface ISignUpService
    {
        Task<SignUpRequest> SignUp(SignUpRequest request);

        Task DeleteTable();
    }
}