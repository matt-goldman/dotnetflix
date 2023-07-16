using DotNetFlix.API.Helpers;
using DotNetFlix.API.Services;
using DotNetFlix.API.Services.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string authority = builder.Configuration.GetValue<string>(nameof(authority))!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = "DotnetflixApi";
        options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };

        // IdentityServer responds based on the host header. In the browser this is
        // localhost, but inside the dcker network its address is identityserver.
        if(builder.Environment.IsEnvironment("Docker"))
        {
            options.TokenValidationParameters.ValidateIssuer = false;
        }

    });

builder.Services.AddScoped<SubscriptionsService>();
builder.Services.AddScoped<VideosService>();


builder.Services.Configure<ServiceConfig>(builder.Configuration.GetSection(nameof(ServiceConfig)));

builder.Services.AddScoped<TokenHandler>();

builder.Services.AddHttpClient(TokenHandler.IdentityClient, client => client.BaseAddress = new Uri(authority));

string subscriptionsUri = builder.Configuration.GetValue<string>("ServiceConfig:SubscriptionsClient:BaseUrl")!;
builder.Services.AddHttpClient(SubscriptionsService.SubscriptionsClient, client => client.BaseAddress = new Uri(subscriptionsUri))
    .AddHttpMessageHandler((s) => s.GetService<TokenHandler>());

string videosUri = builder.Configuration.GetValue<string>("ServiceConfig:VideosClient:BaseUrl")!;
builder.Services.AddHttpClient(VideosService.VideosClient, client => client.BaseAddress = new Uri(videosUri))
    .AddHttpMessageHandler((s) => s.GetService<TokenHandler>());


builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:5005", "https://ambitious-meadow-0ec297a00.3.azurestaticapps.net");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
