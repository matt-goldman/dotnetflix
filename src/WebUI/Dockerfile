# Stage x: Convert .pfx to .crt and .key
FROM debian:stretch AS convert
ARG CERT_PASSWORD

RUN apt-get update && apt-get install -y openssl

COPY localhost.pfx .
RUN openssl pkcs12 -in /certs/localhost.pfx -clcerts -nokeys -out /certs/localhost.crt -password pass:${CERT_PASSWORD} && \
    openssl pkcs12 -in /certs/localhost.pfx -nocerts -nodes -out /certs/localhost.key -password pass:${CERT_PASSWORD}

# Stage x: Build the Blazor WASM app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

# Stage x: Use Nginx to serve the app
FROM nginx:latest AS final
COPY --from=build /app/wwwroot /usr/share/nginx/html
COPY --from=convert /certs/localhost.crt /etc/nginx/localhost.crt
COPY --from=convert /certs/localhost.key /etc/nginx/localhost.key