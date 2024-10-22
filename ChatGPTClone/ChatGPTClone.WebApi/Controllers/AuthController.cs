using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ChatGPTClone.Application.Features.Auth.Commands.Login;

namespace ChatGPTClone.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ApiControllerBase
    {
        public AuthController (ISender mediatr) : base(mediatr) 
        { 
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthLoginCommand command, CancellationToken cancellationToken)
        {
            return Ok(await MediatR.Send(command , cancellationToken));
        }
    }
}
