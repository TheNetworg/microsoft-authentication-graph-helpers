# Microsoft Graph Helpers
> Warning, this is just a proof of concept!

The purpose of this library is to simplify the interaction with Microsoft's authentication libraries and calling 1st party and other APIs.

Please keep on mind that this is still work in progress and there are things like Data Protection into the cache etc. missing right now.

## Setup
1. Add to Dependency Injection
    ```csharp
    // This library depends on MemoryCache / DistributedMemorycache for storing Tokens from the TokenCache
    services.AddMemoryCache();
    
    services.AddSingleton(new ClientCredential(_config["AzureAd:ClientId"], _config["AzureAd:ClientSecret"]));
    services.AddScoped<TokenCacheFactory>();
    services.AddScoped<AdalFactory>();
    services.AddScoped<MicrosoftGraphFactory>();
    services.AddScoped<AzureAdGraphFactory>();
    ```
1. Configure your OpenID Connect Middleware to redeem the code for token and store it into the cache
    ```csharp
    services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
    {
        options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
        options.Events = new OpenIdConnectEvents()
        {
            OnAuthorizationCodeReceived = async context =>
            {
                var authContext = context.HttpContext.RequestServices.GetRequiredService<AdalFactory>().GetAuthenticationContextForUser(context.Principal);
                var clientCred = context.HttpContext.RequestServices.GetRequiredService<Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential>();
                var authResult = await authContext.AcquireTokenByAuthorizationCodeAsync(context.ProtocolMessage.Code, new Uri(context.Properties.Items[OpenIdConnectDefaults.RedirectUriForCodePropertiesKey]), clientCred, "https://graph.microsoft.com");
                context.HandleCodeRedemption(authResult.AccessToken, authResult.IdToken);
            },
        };
    });
    ```
1. Profit
    ```csharp
    public class HomeController : Controller
    {
        private readonly MicrosoftGraphFactory _graphFactory;
        public HomeController(MicrosoftGraphFactory graphFactory)
        {
            _graphFactory = graphFactory;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var graphClient = _graphFactory.GetClientForUser(HttpContext.User);
            var users = await graphClient.Users.Request().GetAsync();

            return Json(users);
        }
        public async Task<IActionResult> IndexAsApp()
        {
            var tenantId = "";
            
            var graphClient = _graphFactory.GetClientForApplication(tenantId);
            var users = await graphClient.Users.Request().GetAsync();

            return Json(users);
        }
    }
    ```
## Authorization Attribute
Once you have set up the above, you may also make use of the `AzureAdAuthorizationAttribute`. The purpose of this is to make authorization with Azure AD roles and groups more simple.

Thanks to this extension, you can make use of real-time group and role based authorization. Currently, the setup is that either of the requirements has to be met. If you want multiple, like group membership and role, just stack it on top of each other.

For this to work, you also need to add `IHttpContextAccessor` to your services.
```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddHttpContextAccessor();
    ...
}
```

```csharp
public class HomeController : Controller
{
    [AzureAdAuthorization(roles: new string[] { AzureAdRoles.CompanyAdministrator }, groups: new string[] { ApplicationGroupIds.AppAdministrators })]
    public async Task<IActionResult> Index()
    {
        var graphClient = _graphFactory.GetClientForUser(HttpContext.User);
        var users = await graphClient.Users.Request().GetAsync();

        return Json(users);
    }
}
```

## Using with APIs
In order for this to work, you need to have JwtBearerMiddleware setup correctly (with `SaveTokens = true`) for Azure AD in your project. This method simplifies the on-behalf-of flow token redemption while leveraging the token cache and everything.
```csharp
public class HomeController : Controller
{
    private readonly MicrosoftGraphFactory _graphFactory;
    public HomeController(MicrosoftGraphFactory graphFactory)
    {
        _graphFactory = graphFactory;
    }
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Index()
    {
        var graphClient = _graphFactory.GetClientForApiUser(HttpContext.GetTokenAsync("access_token"), HttpContext.User);
        var users = await graphClient.Users.Request().GetAsync();

        return Json(users);
    }
}
```