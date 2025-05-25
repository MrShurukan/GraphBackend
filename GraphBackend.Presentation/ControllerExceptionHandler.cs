using GraphBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace GraphBackend;

public static class ControllerExceptionHandler
{
    public static async Task Handle(HttpContext context)
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature!.Error;
        
        var status = 500;
        if (exception is StatusBasedException statusBasedException)
        {
            status = statusBasedException.StatusCode;
        }
        
        var message = exception.Message;

        if (exception is DbUpdateException)
        {
            var exceptionMessage = exception.InnerException!.Message.Split(Environment.NewLine);
            message = exception.InnerException!.Message.Split(Environment.NewLine)[0];

            if (!exceptionMessage[2].Contains("Detail redacted")) message += Environment.NewLine + exceptionMessage[2];
        }
        else if (exception.InnerException is not null)
            message += $" (прикрепл. ошибка: {exception.InnerException.Message})";
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        switch (exception)
        {
            case Locked423Exception locked423Exception:
                await context.Response.WriteAsJsonAsync(new
                {
                    error = message,
                    confirmedWarnings = locked423Exception.ConfirmedWarnings,
                    warningId = locked423Exception.Type
                });
                break;
            case DbUpdateConcurrencyException:
                await context.Response.WriteAsJsonAsync(new
                {
                    error =
                        "Произошло одновременное редактирование ресурса двумя пользователями. Пожалуйста, попробуйте повторить действие или обновить страницу"
                });
                break;
            default:
                await context.Response.WriteAsJsonAsync(new {error = message});
                break;
        }
    } 
}