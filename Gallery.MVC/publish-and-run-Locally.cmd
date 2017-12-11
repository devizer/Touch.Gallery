dotnet publish -v:m -c Release
cd bin\Release\netcoreapp2.0\publish
SET ASPNETCORE_URLS=http://+:6067
start "PRODUCTION Gallery.MVC" dotnet Gallery.MVC.dll 
rem --urls="http://*:28186"