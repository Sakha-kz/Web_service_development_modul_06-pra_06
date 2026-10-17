using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProductSecurityApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAttribute : Attribute, IAuthorizationFilter
{
    public const string HeaderName = "X-Api-Key";
    public const string ValidApiKey = "SecretKey123";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var extractedApiKey))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                statusCode = StatusCodes.Status401Unauthorized,
                message = "API-ключ отсутствует. Передайте ключ в HTTP-заголовке 'X-Api-Key'."
            });
            return;
        }

        if (!string.Equals(extractedApiKey, ValidApiKey, StringComparison.Ordinal))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                statusCode = StatusCodes.Status401Unauthorized,
                message = "Неверный API-ключ. Доступ запрещен."
            });
            return;
        }
    }
}
