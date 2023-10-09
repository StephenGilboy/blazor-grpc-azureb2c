using Gilboy.Messages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Gilboy.Web.Data;
using Microsoft.IdentityModel.Logging;

//********** NOTE THIS IS ALL AN EXAMPLE **********//
//********** YOU WILL NEED TO CLEAN IT UP FOR PRODUCTION USE **********//

var builder = WebApplication.CreateBuilder(args);

// Add MemoryCache - I'm assuming this is to keep track of the user's JWT token
// Take not of the logs:
// Only in-memory caching is used. The cache is not persisted and will be lost if the machine is restarted. It also does not scale for a web app or web API, where the number of users can grow large. In production, web apps and web APIs should use distributed caching like Redis. See https://aka.ms/msal-net-cca-token-cache-serialization
builder.Services.AddDistributedMemoryCache();

// Add the Scope we need to access the gRPC service - This was created in the Azure B2C Gilboy API App Registration
// This is just an example, pretty sure this should be in app settings but I'm being way too lazy to do that right now
const string API_SCOPE = "https://gilboyrmrf.onmicrosoft.com/api/access_as_user";

// Add the cookie policy - I'm assuming this is to keep track of the user's JWT token
// I just know you won't get a token without it
builder.Services.Configure<CookiePolicyOptions>(opt =>
{
    opt.CheckConsentNeeded = context => true;
    opt.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    opt.HandleSameSiteCookieCompatibility();
});

builder.Services.AddOptions();

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C"))
    // Add this so we can get the user token to send to the gRPC service
    .EnableTokenAcquisitionToCallDownstreamApi(new[] { API_SCOPE })
    .AddInMemoryTokenCaches();


builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
    // If you remove this line you will get a 401 Unauthorized error when trying to access the gRPC service
    // But the logs say "Do not use in production" so I'm not sure what the alternative is
    IdentityModelEventSource.ShowPII = true;
    // By default, all incoming requests will be authorized according to the default policy
    options.FallbackPolicy = options.DefaultPolicy;
});

// Time to add the gRPC client
builder.Services.AddGrpcClient<Greeter.GreeterClient>(opt =>
{
    // This is the address of the gRPC service - I have to use http because I'm on a Mac
    // Of course this should be in app settings but I'm still being way to lazy to do that right now
    opt.Address = new Uri("http://localhost:5211");
}).AddCallCredentials(async (context, metadata, serviceProvider) =>
{
    // Get the User's token and add it to the gRPC call headers
    var provider = serviceProvider.GetRequiredService<ITokenAcquisition>();
    var token = await provider.GetAccessTokenForUserAsync(new[] { API_SCOPE });
    metadata.Add("Authorization", $"Bearer {token}");
})
    // Again because I'm on a Mac and have to use http I have to add this
    // Don't do this in production, please.
    .ConfigureChannel(opt =>
{
        opt.UnsafeUseInsecureChannelCallCredentials = true;
});

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();
builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();
builder.Services.AddTransient<GreeterWebService>();
builder.Services.AddSingleton<WeatherForecastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();