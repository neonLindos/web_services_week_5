using DemoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<DemoStore>();
var app = builder.Build();
app.MapControllers();
app.Run();
