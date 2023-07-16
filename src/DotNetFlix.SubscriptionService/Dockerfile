FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5004

ENV ASPNETCORE_URLS=https://+:5004

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
# RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
# USER appuser

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DotNetFlix.SubscriptionService.csproj", "./"]

RUN dotnet restore "DotNetFlix.SubscriptionService.csproj"
COPY . .

RUN dotnet build "DotNetFlix.SubscriptionService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DotNetFlix.SubscriptionService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ./certs/localhost.crt /usr/local/share/ca-certificates/cert.crt
COPY ./certs/cert.pfx ./https/cert.pfx

RUN update-ca-certificates

ENTRYPOINT ["dotnet", "DotNetFlix.SubscriptionService.dll"]
