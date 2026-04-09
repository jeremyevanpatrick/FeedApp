using FeedApp3.Shared.Services;
using FeedApp3.Shared.Services.Queues;
using FeedApp3.Shared.Settings;
using FeedApp3.Web.Data;
using FeedApp3.Web.Services;
using FeedApp3.Web.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ModelStateResponseFilter>();
    options.Filters.Add<ExceptionFilter>();
});

builder.Logging.ClearProviders();
builder.Services.AddSingleton<IExternalScopeProvider, LoggerExternalScopeProvider>();
builder.Services.AddSingleton<ILoggerProvider, RemoteLoggerProvider>();

builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("Application"));
builder.Services.Configure<RemoteLoggingSettings>(builder.Configuration.GetSection("RemoteLogging"));

builder.Services.AddHttpClient<PublicHttpClient>(
    (sp, client) =>
    {
        var settings = sp.GetRequiredService<IOptions<ApplicationSettings>>().Value;
        client.BaseAddress = new Uri(settings.ApiBaseUrl);
    });

builder.Services.AddSingleton<ILogProcessorQueue, LogProcessorQueue>();
builder.Services.AddSingleton<LogSenderService>();

builder.Services.AddScoped<IFeedClient, FeedClient>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<JwtAuthorizationMessageHandler>();

builder.Services.AddHttpClient<AuthenticatedHttpClient>(
    (sp, client) =>
    {
        var settings = sp.GetRequiredService<IOptions<ApplicationSettings>>().Value;
        client.BaseAddress = new Uri(settings.ApiBaseUrl);
    })
    .AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

builder.Services.AddScoped<IAuthClient, AuthClient>();

builder.Services.AddHostedService<LogSenderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Feeds}")
    .WithStaticAssets();


app.Run();
