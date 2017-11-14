# Compile using SDK
# C:\dotnet publish --self-contained -v=m -r linux-arm -c Release
 
# NET Core Dependencies
apt-get update && apt-get install -y --no-install-recommends curl libunwind8 liblttng-ust0 libcurl3 libssl1.0.0 libuuid1 libkrb5-3 zlib1g libicu55 && apt-get clean

# Install .NET Core
export DOTNET_VERSION=2.0.0
export DOTNET_DOWNLOAD_URL=https://dotnetcli.blob.core.windows.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-runtime-$DOTNET_VERSION-linux-arm.tar.gz
export DOTNET_DOWNLOAD_SHA=4A16E7AA761714F74B351BE63C86334B5D5FFB88D9FF4FF3C51B3F4F01DC12FE283B9F6E18E2A48776C9B3EE48F1B52D09E0680C645C3CB765761EEFCD0A9459
curl -SL $DOTNET_DOWNLOAD_URL --output dotnet.tar.gz 
echo "$DOTNET_DOWNLOAD_SHA dotnet.tar.gz" | sha512sum -c - 
mkdir -p /usr/share/dotnet 
tar -zxf dotnet.tar.gz -C /usr/share/dotnet 
rm dotnet.tar.gz 
ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

# Run the App
export ASPNETCORE_URLS=http://+:6066
# export LD_LIBRARY_PATH="/usr/share/dotnet/shared/Microsoft.NETCore.App/2.0.0"
dotnet Gallery.MVC.dll
