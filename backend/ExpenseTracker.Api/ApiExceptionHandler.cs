using ExpenseTracker.Application;
using ExpenseTracker.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api;

/// <summary>Maps known failures to problem responses with a message safe to show.</summary>
public sealed class ApiExceptionHandler : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        int? status = exception switch
        {
            DomainException => StatusCodes.Status422UnprocessableEntity,
            BillReaderUnavailableException => StatusCodes.Status503ServiceUnavailable,
            BillReadFailedException => StatusCodes.Status502BadGateway,
            _ => null,
        };
        if (status is null) return false;

        context.Response.StatusCode = status.Value;
        await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Detail = exception.Message }, ct);
        return true;
    }
}
