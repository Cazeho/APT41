dotnet new console -n stealer
cd stealer
dotnet publish -c Release -r win-x64 --self-contained true
