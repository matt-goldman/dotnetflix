// See https://aka.ms/new-console-template for more information
using YouTube_Test_App;

Console.WriteLine("Hello, World!");

var svc = new YoutubeService();

var rslt = await svc.GetPlayLists();

Console.WriteLine("Got these playlists:");

foreach (var item in rslt)
{
    Console.WriteLine(item.Title);
}