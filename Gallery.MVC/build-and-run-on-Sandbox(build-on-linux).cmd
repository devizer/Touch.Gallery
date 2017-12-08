set PROJECT=Touch.Gallery

pushd "%LOCALAPPDATA%"
echo [System.DateTime]::Now.ToString("yyyy-MM-dd,HH-mm-ss") | powershell -command - > .backup.timestamp
for /f %%i in (.backup.timestamp) do set datetime=%%i
popd

set Snapshot=%PROJECT%-%datetime%.zip

"C:\Program Files\7-Zip\7z.exe" a -tzip -mx=0 -mmt=off -xr!.git -xr!packages -xr!bin -xr!obj "%TEMP%\%Snapshot%" .

plink %SANDBOX_USER%@%SANDBOX_HOST% -l %SANDBOX_USER% -pw %SANDBOX_PASSWORD% -P %SANDBOX_PORT% "echo Hi! It is a sandbox $(hostname); rm -rf /%PROJECT%; mkdir -p /%PROJECT%;"
pscp -P %SANDBOX_PORT% -pw %SANDBOX_PASSWORD% -r "%TEMP%\%Snapshot%" %SANDBOX_USER%@%SANDBOX_HOST%:/%PROJECT%/%Snapshot%
del "%TEMP%\%Snapshot%"
plink %SANDBOX_USER%@%SANDBOX_HOST% -l %SANDBOX_USER% -pw %SANDBOX_PASSWORD% -P %SANDBOX_PORT% "cd /%PROJECT% && 7z x -y %Snapshot% && rm -f %Snapshot% && dotnet build -v:m -c Debug; dotnet run -c Debug --no-build --no-restore"

