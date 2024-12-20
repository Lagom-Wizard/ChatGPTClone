﻿using System;
using ChatGPTClone.Application.Common.Models.General;
using ChatGPTClone.Application.Common.Models.Identity;
using MediatR;

namespace ChatGPTClone.Application.Features.Auth.Commands.Register;

public class AuthRegisterCommand : IRequest<ResponseDto<string>>        
{
    public string Email { get; set; }

    public string Password { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }


public IdentityRegisterRequest ToIdentityRegisterReuest()
{
    return new IdentityRegisterRequest(Email, Password, FirstName, LastName);
}
 }