set PROJECT=Gallery.MVC

dotnet publish --self-contained -c Debug -r linux-x64
set output=bin\Debug\netcoreapp2.0\linux-x64\publish\

plink %SANDBOX_USER%@%SANDBOX_HOST% -l %SANDBOX_USER% -pw %SANDBOX_PASSWORD% -P %SANDBOX_PORT% "echo Hi! It is a sandbox $(hostname); rm -rf /%PROJECT%; mkdir -p /%PROJECT%; killall %PROJECT%"
pscp -P %SANDBOX_PORT% -pw %SANDBOX_PASSWORD% -r %output% %SANDBOX_USER%@%SANDBOX_HOST%:/%PROJECT%/
plink %SANDBOX_USER%@%SANDBOX_HOST% -l %SANDBOX_USER% -pw %SANDBOX_PASSWORD% -P %SANDBOX_PORT% "cd /%PROJECT% && chmod +x %PROJECT%; ./%PROJECT%"
