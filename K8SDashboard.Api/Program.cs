using K8SDashboard.Models;
using K8SDashboard.Services;
using Serilog;
using Prometheus;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


var appSettings = new AppSettings();
builder.Configuration.Bind("AppSettings", appSettings);
builder.Services.AddSingleton(appSettings);

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
    app.UseMetricServer();
}

app.UseAuthorization();

app.MapControllers();

app.Run();