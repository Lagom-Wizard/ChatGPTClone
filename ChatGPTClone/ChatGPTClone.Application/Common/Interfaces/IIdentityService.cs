
using ChatGPTClone.Application.Common.Models.Identity;

namespace ChatGPTClone.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<bool> AuthenticateAsync(IdentityAuthenticateRequest request, CancellationToken cancellationToken);
        Task<bool> CheckEmailExistAsync(string email, CancellationToken cancellationToken);
        Task<bool> CheckEmailExistsAsync(string email, CancellationToken cancellationToken);
        Task<IdentityLoginResponse> LoginAsync(IdentityLoginRequest request, CancellationToken cancellationToken);
        Task<IdentityVerifyEmailResponse> VerifyEmailAsync(IdentityVerifyEmailRequest request, CancellationToken cancellationToken);
    }
}