using DotNetFlix.VideoService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<YouTubeVideosService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/playlists", async (YouTubeVideosService service) =>
{
    var result = await service.GetPlayLists();
    return result;
})
.RequireAuthorization()
.WithName("Playlists")
.WithOpenApi();

app.MapGet("/playlists/{id}/videos", async (string id, YouTubeVideosService service) =>
{
    var result = await service.GetPlaylistVideos(id);
    return result;
})
.RequireAuthorization()
.WithName("Videos")
.WithOpenApi();

app.Run();
