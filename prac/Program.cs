using StudentClient.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddHttpClient("StudentApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseAddress"]
        ?? throw new InvalidOperationException("Укажите Api:BaseAddress."));
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<IStudentApiService, StudentApiService>();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Student/Error");
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Student}/{action=Index}/{id?}");
app.Run();
