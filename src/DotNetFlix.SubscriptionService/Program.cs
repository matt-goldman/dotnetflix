var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.MapGet("/subscription", (string subscriberName) =>
{
    return new Subscription(subscriberName, subscriberName.ToLower().Contains("goldman"));
})
.RequireAuthorization()
.WithName("GetSubscription")
.WithOpenApi();

app.Run();

internal record Subscription(string subscriberName, bool isPremium);
