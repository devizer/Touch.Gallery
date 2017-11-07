#!/bin/bash
if [[ -z "$HOME" ]]; then export HOME=/root; fi;
mkdir -p "$HOME"
sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-xenial-prod xenial main" > /etc/apt/sources.list.d/dotnetdev.list'
sudo apt-get update && sudo apt-get install -y --allow-unauthenticated iotop mc git htop lsof
sudo apt-get install dotnet-sdk-2.0.2 -y --allow-unauthenticated

sudo dd if=/dev/zero of=/swap bs=1M count=512
sudo mkswap /swap
sudo swapon /swap

work=$HOME/Touch.Gallery
if [[ -z "$HOME" ]]; then export work=/root/sss; fi;
rm -rf $work
mkdir -p $work
cd $work
git clone https://github.com/devizer/Touch.Gallery
cd Touch.Gallery/src 
echo 3 | sudo tee /proc/sys/vm/drop_caches > /dev/null
dotnet publish -c Release > $HOME/BUILD-Gallery.log 2>&1
# sudo swapoff /swap

cd ./bin/Release/netcoreapp2.0/publish
echo 3 | sudo tee /proc/sys/vm/drop_caches > /dev/null
sudo bash -c 'ASPNETCORE_URLS=http://0.0.0.0:80\;http://0.0.0.0:8080\;http://0.0.0.0:5000 dotnet Gallery.MVC.dll' | tee $HOME/RUN-Gallery.log
