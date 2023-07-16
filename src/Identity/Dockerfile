FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5001

ENV ASPNETCORE_URLS=https://+:5001

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
# RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
# USER appuser

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DotNetFlix.Identity.csproj", "./"]

RUN dotnet restore "DotNetFlix.Identity.csproj"
COPY . .

RUN dotnet build "DotNetFlix.Identity.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DotNetFlix.Identity.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ./certs/localhost.crt /usr/local/share/ca-certificates/cert.crt
COPY ./certs/cert.pfx ./https/cert.pfx

RUN update-ca-certificates

ENTRYPOINT ["dotnet", "DotNetFlix.Identity.dll"]

# Running the migration during the container build
# This assumes that your DbContext resides in the DotNetFlix.Identity project
CMD dotnet ef database update --project DotNetFlix.Identity.csproj --connection ${ConnectionStrings__DefaultConnection}
