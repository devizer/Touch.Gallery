dotnet publish -v:m -c Release
cd bin\Release\netcoreapp2.0\publish
SET ASPNETCORE_URLS=http://0.0.0.0:6067
SET ASPNETCORE_URLS=http://+:5000
start "PRODUCTION Gallery.MVC" dotnet Gallery.MVC.dll 
rem --urls="http://*:28186"