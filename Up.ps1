# check for a docker environment file
# also, if the .env file has been created, the certs will also have been created
if (-not(Test-Path "./.env")) {
    # no env file found, so create one

    $apiKey = Read-Host -Prompt "Enter your YouTube API Key"

    # Generate a random SA password
    $saPassword = -Join("ABCDEFGHIJKLMNOPQRSTUVWXYXabcdefghijklmnopqrstuvwxyz&@#$%1234".tochararray() | Get-Random -Count 10 | % {[char]$_})
    $certPassword = -Join("ABCDEFGHIJKLMNOPQRSTUVWXYXabcdefghijklmnopqrstuvwxyz&@#$%1234".tochararray() | Get-Random -Count 10 | % {[char]$_})

    # Write the passwords to the .env file
    "SA_PASSWORD=$saPassword" | Out-File -FilePath .env -Append
    "CERT_PASSWORD=$certPassword" | Out-File -FilePath .env -Append
    "YOUTUBE_API_KEY=$apiKey" | Out-File -FilePath .env -Append
    
    # Create the certs
    if (-not(Test-Path "./certs")) {
        New-Item ./certs/ -type Directory
    }

    Copy-Item ./v3.ext ./certs/v3.ext

    $dockerCommand = "apt-get update && "`
    + "apt-get install -y openssl && "`
    + "openssl req -x509 -newkey rsa:4096 -keyout /certs/key.pem -out /certs/cert.pem -days 365 -nodes -subj '/CN=localhost' -extensions v3_req -config /certs/v3.ext && "`
    + "openssl pkcs12 -export -out /certs/cert.pfx -inkey /certs/key.pem -in /certs/cert.pem -password pass:${certPassword} && "`
    + "openssl pkcs12 -in /certs/cert.pfx -clcerts -nokeys -out /certs/localhost.crt -password pass:${certPassword}"

    docker run --rm -it -v ${PWD}/certs:/certs debian:latest bash -c $dockerCommand

    $dockerConvertCommand = "apt-get update && "`
    + "apt-get install -y openssl && "`
    + "openssl pkcs12 -in /certs/cert.pfx -nocerts -nodes -out /certs/localhost.key -password pass:${certPassword} -info"
    docker run --rm -it -v ${PWD}/certs:/certs debian:latest bash -c $dockerConvertCommand

    # Get the full path to the folder containing the cert0ificate
    $certFolderPath = (Get-Item -Path "./certs").FullName

    # Write the cert folder path to the .env file
    "DEVCERTS_PATH=$certFolderPath" | Out-File -FilePath .env -Append

    # install the certificate
    if ($IsWindows) {
        $certPath = Join-Path -Path $certFolderPath -ChildPath "cert.pfx"
        Import-PfxCertificate -FilePath $certPath -CertStoreLocation Cert:\CurrentUser\My -Password (ConvertTo-SecureString -String $certPassword -AsPlainText -Force)
        
        # Run the Admin-InstallCerts.ps1 script as an administrator
        # Requires elevated privileges to install machine root certificates
        $scriptPath = (Get-Item -Path "./Admin-InstallCerts.ps1").FullName

        Start-Process -FilePath "powershell" -ArgumentList "-File $scriptPath -certPath $certPath -certPassword $certPassword" -Verb RunAs
    }
    elseif ($IsMacOS) {
        security import ./certs/cert.pfx -k ~/Library/Keychains/login.keychain-db -P $certPassword -A
    }
    elseif ($IsLinux) {
        Write-Host "NOTE: A self-signed certificate has been created for you. You will need to install it manually. (You can trust it when you first browse to one of the pages)."
    }

    # copy the certs to the Identity and Subscription project folders
    if (-not(Test-Path "./src/Identity/certs")) {
        New-Item ./src/Identity/certs/ -type Directory
    }

    Copy-Item ./certs/* ./src/Identity/certs

    if (-not(Test-Path "./src/DotNetFlix.SubscriptionService/certs")) {
        New-Item ./src/DotNetFlix.SubscriptionService/certs/ -type Directory
    }

    Copy-Item ./certs/* ./src/DotNetFlix.SubscriptionService/certs
}

# Spin up your Docker Compose services
docker-compose up -d

# Give some time for your services to fully start up.
# Depending on your services, you might need to increase this time.
# Start-Sleep -s 30

# Run your Postman collection
# newman run ./Dotnetflix.postman_collection.json -e ./Dotnetflix-docker-environment.json

# Tear down the services after tests are done
#docker-compose down
