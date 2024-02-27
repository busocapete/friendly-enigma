using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BuSoCa.Data.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json;

namespace Admin.Sourcers.Api.InjectServices
{
	public static partial class AuthenticationInitializer
	{
		public static IServiceCollection RegisterAuthentication(
			this IServiceCollection services, WebApplicationBuilder builder)
		{
		    var azureCredential = new DefaultAzureCredential();
	
		    var secretClient = new SecretClient(
			new Uri(builder.Configuration["KeyVault:RootUri"]), azureCredential);
	
		    var authSecret = secretClient
			.GetSecret(builder.Configuration["KeyVault:AuthProvider"]);
	
		    var authConfig = JsonConvert.DeserializeObject<AuthProviderConfig>(authSecret.Value.Value);
	
		    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddOAuth2Introspection(options =>
			{
			    options.Authority = authConfig.Authority;
			    options.ClientId = authConfig.ClientId;
			    options.ClientSecret = authConfig.ClientSecret;
			    options.NameClaimType = "given_name";
			    options.RoleClaimType = "role";
			});
	
		    return services;
		}
	}
  }
