# Use the .NET SDK to build and publish the Identity service
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
COPY DotNetFlix.Identity.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app

# Use the runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "DotNetFlix.Identity.dll"]

# Running the migration during the container build
# This assumes that your DbContext resides in the DotNetFlix.Identity project
CMD dotnet ef database update --project DotNetFlix.Identity.csproj --connection ${ConnectionStrings__DefaultConnection}
