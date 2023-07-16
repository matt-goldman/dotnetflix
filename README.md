# dotnetflix

**NOTE:** There is a YouTube channel called DotNetFlix (see https://dotnetflix.com) - this is not affiliated with them, I just had the same idea for this cool name for my demo (albeit nearly a decade later).

Dotnetflix is a demo solution for my talk **Beyond Passwords: The future (and present) of authentication**. It shows how you can use existing, established familiar technologies to achieve passwordless authentication in your solution. It also highlights which authentication options are suited to different scenarios.

## Mechanisms used

| **Scenario**                    | **Authentication mechanism** |
|---------------------------------|------------------------------|
| API to API                      | Client credentials grant     |
| TV app                          | Device code grant            |
| Laptop or desktop: registration | Username and password        |
| Laptop, desktop, or phone: login | WebAuthN |


# Running the demo

You can see a demo online here: https://ambitious-meadow-0ec297a00.3.azurestaticapps.net/
Although I can't guarantee I will keep this running. It's on a low cost Azure tier and often goes dormant, so you may need to prod it awake a few times before it works.

You can also run the demo locally using either the dotnet cli or docker. You will need a Google API key for YouTube.

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

## Docker

Instead of starting all the services indivudally, you can run the whole solution in Docker.

**NOTE:** You will need Docker installed as well as PowerShell Core to get the environment set up.

1. Open PowerShell Core
2. Run the `Up.ps1` script
3. When prompted, enter your YouTube API key
4. (On Windows) when prompted, allow PowerShell to run as administrator (this installs a local certificate used by the services in the container)
5. Open a web browser and go to https://localhost:5005

**Note for macOS and Linux users**: The script should run on macOS and Linux too, but I haven't tested it yet. On macOS, the only part that has the potenatial to work differently is the certiifcate install, so at worst you can just install this manually. This part is skipped on Linux so I expect it to work anyway.

# Running the .NET MAUI app

The .NET MAUI app will run on macOS, Windows, Linux or iOS. Depending on which of these you are running it on, you will need to update the base URL and authority URL. If on Windows or macOS, you can use localhost with the right ports. If you're running it on iOS or Android you'll need to use a tunnel like ngrok. These instructions apply to .NET cli or docker.

# Postman

There is a postman collection and environment you can import to test the various OIDC flows used (but not WebAuthN).

# The code

Feel free to poke around. I'll add a wiki soon highlighting the most interesting parts of the code.