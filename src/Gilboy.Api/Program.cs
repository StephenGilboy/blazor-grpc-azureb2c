using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Gilboy.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
// By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
// For instance, 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles' claim.
// This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
// See Github Repo I Got this from https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/2917bc097d2d480d31c941dc52c6687b70de4580/4-WebApp-your-API/4-2-B2C/TodoListService/Startup.cs#L27
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

// Adds Microsoft Identity platform (AAD v2.0) support to protect this Api
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            IdentityModelEventSource.ShowPII = true;
            builder.Configuration.Bind("AzureAdB2C", options);
        },
        options => { builder.Configuration.Bind("AzureAdB2C", options); });

// Add services to the container.
builder.Services.AddGrpc();

if (builder.Environment.IsDevelopment())
{
    // This is what I was talking about in the Gilboy.Web/Program.cs file
    // I have to use http instead of https on Mac.
    
    // This will allow the service to run on macOS with the default configuration.
    builder.WebHost.ConfigureKestrel(options =>
    {
        // Setup a HTTP/2 endpoint without TLS.
        options.ListenLocalhost(5211, o => o.Protocols =
            HttpProtocols.Http2);
    });
}


var app = builder.Build();

// Configure the Auth middleware like every other ASP.NET Core app
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();