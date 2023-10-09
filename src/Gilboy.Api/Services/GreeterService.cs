using Grpc.Core;
using Gilboy.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;

namespace Gilboy.Api.Services;

[Authorize]
[RequiredScope(API_SCOPE)]
public class GreeterService : Greeter.GreeterBase
{
    // Again, probably want to put these somewhere else but I've been at this for hours on a real project
    // so I'm being extremely lazy just so I can demonstrate how this is done.
    const string API_SCOPE = "access_as_user";
    
    private readonly ILogger<GreeterService> _logger;

    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        // Let's get the user name out of the JWT token
        var httpCtx = context.GetHttpContext();
        var userName = httpCtx.User?.Claims?.FirstOrDefault(c => c.Type.ToLower() == "name")?.Value ?? "Unknown";
        
        return Task.FromResult(new HelloReply
        {
            Message = $"Hello {userName}. You said your name was {request.Name}."
        });
    }
}