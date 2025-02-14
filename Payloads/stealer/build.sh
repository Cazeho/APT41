dotnet new console -n stealer
cd stealer
dotnet add package System.Data.SQLite
dotnet add package System.Management
dotnet add package System.Security.Cryptography.ProtectedData
dotnet publish -c Release -r win-x64 --self-contained true
