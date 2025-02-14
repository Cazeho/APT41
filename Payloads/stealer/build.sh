## dotnet version 8
wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt update
apt-get install -y dotnet-sdk-8.0

dotnet new console -n stealer
cd stealer
dotnet add package System.Data.SQLite
dotnet add package System.Management
dotnet add package System.Security.Cryptography.ProtectedData
dotnet publish -c Release -r win-x64 --self-contained true



wmic csproduct get uuid
