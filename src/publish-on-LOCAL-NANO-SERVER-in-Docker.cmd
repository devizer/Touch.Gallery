dotnet publish -c Release -r win10-x64
net start docker 1>nul 2>&1
cd bin\Release\netcoreapp2.0\win10-x64\publish\
docker run ^
  -e ASPNETCORE_URLS=http://+:6068 -v %cd%:c:/app -w C:\app ^
  --name Gallery -h Gallery ^
  microsoft/dotnet:2.0.3-runtime-nanoserver-sac2016 Gallery.MVC.exe

rem http://Gallery:6068
