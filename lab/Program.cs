using ProductClient.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddHttpClient("ProductApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseAddress"]
        ?? throw new InvalidOperationException("Укажите Api:BaseAddress."));
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<IProductApiService, ProductApiService>();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Product/Error");
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Product}/{action=Index}/{id?}");
ProductClient.Api.DemoApi.StartIfNeeded(app);
app.Run();
