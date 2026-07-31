using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace SharedLibrary.Common;

public class AuthorizationPropagationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationPropagationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null && context.Request.Headers.TryGetValue("Authorization", out var auth) && !string.IsNullOrEmpty(auth))
        {
      
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(auth.ToString());
        }

        return base.SendAsync(request, cancellationToken);
    }
}
