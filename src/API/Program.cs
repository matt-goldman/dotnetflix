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
    });

builder.Services.AddSingleton<SubscriptionsService>();
builder.Services.AddSingleton<VideosService>();

var serviceOptions = builder.Configuration.Get<ServiceConfig>()!;
builder.Services.AddSingleton(serviceOptions);

builder.Services.AddHttpClient(BaseService.IdentityClient, client => client.BaseAddress = new Uri(authority));

builder.Services.AddHttpClient(SubscriptionsService.SubscriptionsClient, client => client.BaseAddress = new Uri(serviceOptions.SubscriptionsClient.BaseUrl));

builder.Services.AddHttpClient(VideosService.VideosClient, client => client.BaseAddress = new Uri(serviceOptions.VideosClient.BaseUrl));


builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:7009");
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
