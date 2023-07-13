# dotnetflix

**NOTE:** There is a YouTube channel called DotNetFlix (see https://dotnetflix.com) - this is not affiliated with them, I just had the same idea for this cool name for my demo (albeit nearly a decade later).

Dotnetflix is a demo solution for my talk **Beyond Passwords: The future (and present) of authentication**. It shows how you can use existing, established familiar technologies to achieve passwordless authentication in your solution. It also highlights which authentication options are suited to different scenarios.

**Demo:** https://ambitious-meadow-0ec297a00.3.azurestaticapps.net/

## Mechanisms used

| **Scenario**                    | **Authentication mechanism** |
|---------------------------------|------------------------------|
| API to API                      | Client credentials grant     |
| TV app                          | Device code grant            |
| Laptop or desktop: registration | Username and password        |
| Laptop, desktop, or phone: login | WebAuthN |


# docker

* needs powershell core
* run up
* Accept cert prompt