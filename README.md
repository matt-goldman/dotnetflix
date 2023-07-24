# dotnetflix

Watch the video:    
[<img src="https://i.ytimg.com/vi/K1t73xArqs0/maxresdefault.jpg" width="50%">](https://www.youtube.com/watch?v=K1t73xArqs0 "Beyond Passwords: The future (and present) of authentication")

**NOTE:** There is a YouTube channel called DotNetFlix (see https://dotnetflix.com) - this is not affiliated with them, I just had the same idea for this cool name for my demo (albeit nearly a decade later).

Dotnetflix is a demo solution for my talk **Beyond Passwords: The future (and present) of authentication**. It shows how you can use existing, established familiar technologies to achieve passwordless authentication in your solution. It also highlights which authentication options are suited to different scenarios.

# The code

Check [the wiki](https://github.com/matt-goldman/dotnetflix/wiki) for the most interesting parts and to see how things work.

# Running the demo

You can see a demo online here: https://ambitious-meadow-0ec297a00.3.azurestaticapps.net/
Although I can't guarantee I will keep this running. It's on a low cost Azure tier and often goes dormant, so you may need to prod it awake a few times before it works.

You can also run the demo locally using either the dotnet cli or docker. You will need a Google API key for YouTube.

**Note:** The migrations were all created using MSSQL LocalDB. If you're not running on Windows, you'll need to install Azure SQL Edge, and update the connection string in the Identity project.

TODO:
- [ ] Move this to PostgreSQL

## Docker

Instead of starting all the services indivudally, you can run the whole solution in Docker.

**NOTE:** You will need Docker installed as well as PowerShell Core to get the environment set up.

1. Open PowerShell Core
2. Run the `Up.ps1` script
3. When prompted, enter your YouTube API key
4. (On Windows) when prompted, allow PowerShell to run as administrator (this installs a local certificate used by the services in the container)
5. Open a web browser and go to https://localhost:5005

**Note for macOS and Linux users**: The script should run on macOS and Linux too, but I haven't tested it yet. On macOS, the only part that has the potenatial to work differently is the certiifcate install, so at worst you can just install this manually. This part is skipped on Linux so I expect it to work anyway.

## .NET CLI

1. Add a user secrets file to the Videos project (`src/DotNetFlix.VideoService`)
2. Add a secret for the YouTube API key:
```json
{
  "YouTube": {
    "ApiKey": "<Add your key here>"
  }
```
You can also add it in the format `"YouTube__ApiKey":"<Add your key here>"`

3. You will need to open multiple terminals or terminal tabs. Start the following projects with `dotnet run`:
    - `src/API`
    - `src/Identity`
    - `src/DotNetFlix/SubscriptionService`
    - `src/DotNetFlix/VideoService`
    - `src/WebUI`

4. Open a web browser and go to https://localhost:5005

# Visual Studio
You could edit the solution configuration to launch [multiple startup projects](https://learn.microsoft.com/visualstudio/ide/how-to-set-multiple-startup-projects). It will work with MSSQLLocalDB, you'll just need to start IdentityServer, the Blazor app, the API, the Videos service, and the Subscriptions service.

# Running the .NET MAUI app

The .NET MAUI app will run on macOS, Windows, Linux or iOS. Depending on which of these you are running it on, you will need to update the base URL and authority URL. If on Windows or macOS, you can use localhost with the right ports. If you're running it on iOS or Android you'll need to use a tunnel like ngrok. These instructions apply to .NET cli or docker.

# Postman

There is a postman collection and environment you can import to test the various OIDC flows used (but not WebAuthN). Works with either docker or local .NET (via CLI or Visual Studio).
