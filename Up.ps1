# Generate a random SA password
$saPassword = -Join("ABCDEFGHIJKLMNOPQRSTUVWXYXabcdefghijklmnopqrstuvwxyz&@#$%1234".tochararray() | Get-Random -Count 10 | % {[char]$_})
$certPassword = -Join("ABCDEFGHIJKLMNOPQRSTUVWXYXabcdefghijklmnopqrstuvwxyz&@#$%1234".tochararray() | Get-Random -Count 10 | % {[char]$_})

# Set the SA password as an environment variable
$env:SA_PASSWORD = $saPassword
$env:CERT_PASSWORD = $certPassword

# Get the path to dotnet dev-certs and set in environment variable
if ($IsWindows) {
    $env:DEVCERTS_PATH = "${env:USERPROFILE}\.aspnet\https"
}
elseif ($IsLinux -or $IsMacOS) {
    $env:DEVCERTS_PATH = "${env:HOME}/.aspnet/https"
}

$certPath = Join-Path -Path $env:DEVCERTS_PATH -ChildPath "aspnetapp.pfx"

dotnet dev-certs https -ep $certPath -p $certPassword
dotnet dev-certs https --trust

if (-not(Test-Path $certPath)) {
    Write-Error "dotnet dev-certs not found at $certPath"
    exit -1
}

# copy the certs into the Blazor folder for use by nginx
New-Item ./src/WebUI/certs/ -ItemType Directory -Force
Copy-Item $certPath ./src/WebUI/certs/localhost.pfx

# Spin up your Docker Compose services
docker-compose up -d

# Give some time for your services to fully start up.
# Depending on your services, you might need to increase this time.
Start-Sleep -s 30

# Run your Postman collection
# newman run ./Dotnetflix.postman_collection.json -e ./Dotnetflix-docker-environment.json

# Tear down the services after tests are done
#docker-compose down
