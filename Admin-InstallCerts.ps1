param($certPath, $certPassword)
$securePassword = (ConvertTo-SecureString -String $certPassword -AsPlainText -Force)
Import-PfxCertificate -FilePath $certPath -CertStoreLocation Cert:\LocalMachine\Root -Password $securePassword