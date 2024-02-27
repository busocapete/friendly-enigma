using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BuSoCa.Data.Config;
using cra_latest.Config;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Newtonsoft.Json;

namespace cra_latest.RegisterServices
{
	public static partial class ClientIDPInitializer
	{
		public static IServiceCollection RegisterIdentityServer(
		    this IServiceCollection services
		    WebApplicationBuilder builder)
			{
			var azureCredential = new DefaultAzureCredential();
	
			var secretClient = new SecretClient(
			new Uri(builder.Configuration["KeyVault:RootUri"]), azureCredential);
	
			var authSecret = secretClient
			.GetSecret(builder.Configuration["KeyVault:AuthProvider"]);
	
			var authConfig = JsonConvert.DeserializeObject<AuthProviderConfig>(authSecret.Value.Value);
	
			services.AddAuthentication(options =>
			{
				options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
				options.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
				options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			})
			.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
			{
				options.Cookie.SameSite = SameSiteMode.Strict;
				options.AccessDeniedPath = "/Authentication/AccessDenied";
			})
			.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
			{
				options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.Authority = authConfig.Authority;
				options.ClientId = authConfig.ClientId;
				options.ClientSecret = authConfig.ClientSecret.ToString();
				options.ResponseType = "code";
				options.ResponseMode = "query";
		
				options.GetClaimsFromUserInfoEndpoint = true;
				options.MapInboundClaims = false;
				options.SaveTokens = true;
		
				options.ClaimActions.Remove("aud");
				//sid claim required for logoutUrl in react application.
				//Do not delete the claim as it is in the MVC apps.
				//options.ClaimActions.DeleteClaim("sid");
				options.ClaimActions.DeleteClaim("idp");
				//requires to be included in client scopes at IDP level
				options.Scope.Add("roles");
				//options.Scope.Add("sourcersapi_fullaccess");
				options.Scope.Add("sourcersapi_read");
				options.Scope.Add("sourcersapi_write");
				options.Scope.Add("country");
				options.Scope.Add("sourcersapi");
				options.Scope.Add("email");
				//Required for Refresh tokens.
				options.Scope.Add("offline_access");
				//scopes require associated JSON mapping
				//if claim !array use, MapUniqueJsonKey instead
				options.ClaimActions.MapJsonKey("role", "role");
				options.ClaimActions.MapUniqueJsonKey("country", "country");
				options.ClaimActions.MapUniqueJsonKey("email", "email");
				//To be able to use User.IsInRole("PayingUser")
				//and User.Name in the Views, require the TokenValidation below
				options.TokenValidationParameters = new()
				{
					NameClaimType = "name",
					RoleClaimType = "role"
				};
		});
	  
		return services;
		}
	}
}

