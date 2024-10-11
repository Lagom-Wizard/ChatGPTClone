using ChatGPTClone.Application.Features.ChatSessions.Commands.Create;
using ChatGPTClone.Application.Features.ChatSessions.Queries.GetAll;
using ChatGPTClone.Application.Features.ChatSessions.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ChatGPTClone.WebApi.Controllers;

[Authorize]
public class ChatSessionsController : ApiControllerBase
{
    public ChatSessionsController(ISender mediatR) : base(mediatR)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        return Ok(await MediatR.Send(new ChatSessionGetAllQuery(), cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await MediatR.Send(new ChatSessionGetByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(ChatSessionCreateCommand command, CancellationToken cancellationToken)
    {
        return Ok(await MediatR.Send(command, cancellationToken));
    }

    

}