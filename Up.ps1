# Generate a random SA password
$saPassword = New-Guid

# Set the SA password as an environment variable
$env:SA_PASSWORD = $saPassword.ToString()

# Spin up your Docker Compose services
docker-compose up -d

# Give some time for your services to fully start up.
# Depending on your services, you might need to increase this time.
Start-Sleep -s 30

# Run your Postman collection
newman run ./Dotnetflix.postman_collection.json -e ./Dotnetflix-docker-environment.json

# Tear down the services after tests are done
docker-compose down
