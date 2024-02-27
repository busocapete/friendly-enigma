using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using Admin.Sourcers.Api.Config;
using Admin.Sourcers.Api.InjectServices;
using Admin.Sourcers.Api.Middleware;
using AutoMapper;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BuSoCa.Data.Config;
using BuSoCa.Data.Data;
using BuSoCa.MappingProfiles;
using BuSoCa.Model.Email;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SourcersDbContext>(options =>
    options.UseSqlServer(builder.Configuration
    .GetConnectionString("SourcersConnection"), b => b.MigrationsAssembly("BuSoCa.Data")));

builder.Services.AddDbContext<BuSoCaContext>(options =>
    options.UseSqlServer(builder.Configuration
    .GetConnectionString("BuSoCaConnection"), b => b.MigrationsAssembly("BuSoCa.Data"))) ;

// Inject Services > ControllersInitializer
builder.Services.RegisterControllers();

//Inject EnumModelBinders so that strings of the value work in controllers
builder.Services.RegisterEnumModelBinders();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Must be available for Authorization Policies
builder.Services.AddHttpContextAccessor();

//injectServices > RepositoryInitializer
builder.Services.RegisterSourcersRepositories();
builder.Services.RegisterBuSoCaRepositories();

builder.Services.RegisterSendGridServices();

builder.Services.RegisterServiceBuses();


builder.Services.RegisterWorkerServices();

builder.Services.RegisterAuthorizationPolicies();
builder.Services.RegisterAuthorization();

builder.Services.RegisterConfigOptions(builder);

builder.Services.RegisterCors();


JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.RegisterAuthentication(builder);



// AutoMapper not including BuSoCa.Mapping Profiles
// without using it or calling Assemblies.Load("BuSoCa.MappingProfile")
var loadBuSoCaMappingProfiles = new LoadBuSoCaMappingProfile();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


var app = builder.Build();

MigrateDatabase.MigrateDatabaseAtStartup(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors("Sourcers_Admin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
