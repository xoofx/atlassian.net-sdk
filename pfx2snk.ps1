Param(
    [Parameter(Mandatory=$True,Position=1)]
    [string] $pfxFilePath,
    [string] $pfxPassword
)

# The path to the snk file we're creating
[string] $snkDirectoryPath = [IO.Path]::GetDirectoryName($pfxFilePath);
[string] $snkFileName = [IO.Path]::GetFileNameWithoutExtension($pfxFilePath) + ".snk";
[string] $snkFilePath = [IO.Path]::Combine($snkDirectoryPath, $snkFileName);

# Read in the bytes of the pfx file
[byte[]] $pfxBytes = Get-Content $pfxFilePath -Encoding Byte;

# Get a cert object from the pfx bytes with the private key marked as exportable
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2(
    $pfxBytes,
    $pfxPassword,
    [Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable);

# Export a CSP blob from the cert (which is the same format as an SNK file)
[byte[]] $snkBytes = ([Security.Cryptography.RSACryptoServiceProvider]$cert.PrivateKey).ExportCspBlob($true);

# Write the CSP blob/SNK bytes to the snk file
Set-Content -Path "$snkFilePath" -Value $snkBytes -Encoding Byte
#[IO.File]::WriteAllBytes($snkFilePath, $snkBytes);