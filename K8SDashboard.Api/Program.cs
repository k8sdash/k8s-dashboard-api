using K8SDashboard.Models;
using K8SDashboard.Services;
using Serilog;
using Prometheus;
using Microsoft.OpenApi.Models;
using K8SDashboard.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var appSettings = new AppSettings();
builder.Configuration.Bind("AppSettings", appSettings);
builder.Services.AddSingleton(appSettings);
builder.Services.AddSingleton<K8SClientService>();
builder.Services.AddSingleton<K8SEventManager>();
builder.Services.AddSignalR(hubOptions =>
{
    //hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(5);
    //hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(60);
    //hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientPermission", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins(appSettings.CorsPolicyWithOrigins) 
            .WithMethods("GET","POST")
            .AllowCredentials()
            ;
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = appSettings.ApiTitle,
        Description = appSettings.ApiDescription,
        Contact = new OpenApiContact
        {
            Name = appSettings.ApiContactName,
            Url = new Uri(appSettings.ApiContactUrl)
        },
        License = new OpenApiLicense
        {
            Name = appSettings.ApiLicenseName,
            Url = new Uri(appSettings.ApiLicenseUrl)
        }
    });
});
builder.Services.AddSingleton<IK8SClientService, K8SClientService>();

builder.Host.UseSerilog((ctx, lc) => lc
.ReadFrom.Configuration(ctx.Configuration));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseMetricServer();
app.UseAuthorization();

app.MapControllers();
app.MapHub<LightRoutesHub>("/hubs/lightroutes");
app.UseCors("ClientPermission");
using var scope = app.Services.CreateScope();
var k8SClientService = scope.ServiceProvider?.GetService<K8SClientService>();
var k8SEventManager = scope.ServiceProvider?.GetService<K8SEventManager>();
if (k8SClientService!= null && k8SClientService.Valid() && k8SEventManager != null)
    k8SEventManager.Start();
else
    return;

app.Run();