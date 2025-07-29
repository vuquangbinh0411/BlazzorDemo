using BlazingTrails.Client;
using BlazingTrails.Client.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadContent>("head::after");

// cấu hình gọi API có gắn token
builder.Services.AddHttpClient("SecureAPIClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:5001"); // URL của BlazingTrails.Api
})
.AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// fallback HttpClient không gắn token
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// đăng ký MediatR
builder.Services.AddMediatR(typeof(Program).Assembly);

// cấu hình đăng nhập Auth0
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Auth0", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";

    // thêm các scope cần thiết
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("email");

    // thêm audience để nhận access token cho API
    options.ProviderOptions.AdditionalProviderParameters["audience"] =
        builder.Configuration["Auth0:Audience"]!;
})
.AddAccountClaimsPrincipalFactory<CustomUserFactory<RemoteUserAccount>>();

await builder.Build().RunAsync();
