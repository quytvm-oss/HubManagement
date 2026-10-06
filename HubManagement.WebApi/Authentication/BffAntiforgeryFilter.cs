using Microsoft.AspNetCore.Antiforgery;

namespace HubManagement.WebApi.Authentication;

public sealed class BffAntiforgeryFilter(IAntiforgery antiforgery) : IEndpointFilter
{
    private static readonly HashSet<string> SafeMethods =
        new(StringComparer.OrdinalIgnoreCase) { "GET", "HEAD", "OPTIONS", "TRACE" };

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!SafeMethods.Contains(context.HttpContext.Request.Method))
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context.HttpContext);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid CSRF token",
                    detail: "Fetch /api/v1/identity/csrf and send its token in the X-CSRF-TOKEN header.");
            }
        }

        return await next(context);
    }
}
