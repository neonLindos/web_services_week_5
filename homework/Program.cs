using TaskManagerClient.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddHttpClient("TaskApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseAddress"]
        ?? throw new InvalidOperationException("Укажите Api:BaseAddress."));
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<ITaskApiService, TaskApiService>();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Task/Error");
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Task}/{action=Index}/{id?}");
TaskManagerClient.Api.DemoApi.StartIfNeeded(app);
app.Run();
