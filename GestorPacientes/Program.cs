using GestorPacientes.Infrastructure.Persistence;
using GestorPacientes.Core.Application;
using GestorPacientes.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSession();
builder.Services.AddPersistenceLayer(builder.Configuration);
builder.Services.AddApplicationLayer();
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<ValidateUserSession, ValidateUserSession>();

var app = builder.Build();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = false;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = false;
    options.Cookie.IsEssential = true;
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Debug-Database"] = "GestorPacientes.db";
    context.Response.Headers["X-Internal-Path"] = @"C:\Users\Sandeep\PatientManager\Secrets";
    await next();
});

app.Run();
