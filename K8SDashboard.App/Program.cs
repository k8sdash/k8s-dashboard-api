using K8SDashboard.Models;
using K8SDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSwaggerDocument(config =>
{
    config.PostProcess = document =>
    {
        document.Info.Title = "K8SDashboard";
        document.Info.Description = "A simple Kubernetes Dashboard, exposing ingress routes, pods and nodes";
    };
});
builder.Services.AddSingleton<IK8sClientService, K8sClientService>();
builder.Services.AddSingleton<AppSettings>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseOpenApi();
app.UseSwaggerUi3();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
