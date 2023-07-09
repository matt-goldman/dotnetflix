# Stage 1: Build the Blazor WASM app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

# Stage 2: Use Nginx to serve the app
FROM nginx:latest AS final
COPY --from=build /app/wwwroot /usr/share/nginx/html
