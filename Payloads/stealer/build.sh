dotnet new console -n stealer
cd stealer
dotnet add package System.Data.SQLite
dotnet add package System.Management
dotnet publish -c Release -r win-x64 --self-contained true
