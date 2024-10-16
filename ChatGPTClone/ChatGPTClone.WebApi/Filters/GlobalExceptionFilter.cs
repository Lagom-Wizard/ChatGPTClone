using ChatGPTClone.Application.Common.Models.Errors;
using ChatGPTClone.Application.Common.Models.General;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;
namespace ChatGPTClone.WebApi.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, context.Exception.Message);
        context.ExceptionHandled = true;

        if (context.Exception is FluentValidation.ValidationException) 
        {
            var exception = context.Exception as FluentValidation.ValidationException;
            var responseMessage = "one or more validation errors occured";
            List<ErrorDto> errors = new();

            var propertyNames = exception.Errors
                .Select(error => error.ErrorMessage)
                .Distinct();

            foreach (var propertyName in propertyNames)
            {
                var messages = exception.Errors
                    .Where(e => e.PropertyName == propertyName)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                errors.Add(new ErrorDto(propertyName, messages));
            }
            context.Result = new BadRequestObjectResult(new ResponseDto<string>(responseMessage, errors))
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }
        else
        {
            context.Result = new ObjectResult(new ResponseDto<string>("internal server error", false))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
  }