﻿using ChatGPTClone.Application.Common.Interfaces;
using ChatGPTClone.Application.Common.Models.General;
using MediatR;

namespace ChatGPTClone.Application.Features.Auth.Commands.Register;

public class AuthRegisterCommandHandler : IRequestHandler<AuthRegisterCommand, ResponseDto<AuthRegisterDto>>
{
    private readonly IIdentityService _identityService;

    public AuthRegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<ResponseDto<AuthRegisterDto>> Handle(AuthRegisterCommand request, CancellationToken cancellationToken)
    {
        var response = await _identityService.RegisterAsync(request.ToIdentityRegisterReuest(), cancellationToken);

        return new ResponseDto<AuthRegisterDto>(AuthRegisterDto.Create(response), "User registered successfully");
    }
}