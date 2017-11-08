#!/bin/bash
if [[ -z "$HOME" ]]; then export HOME=/root; fi;
mkdir -p "$HOME"
sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-xenial-prod xenial main" > /etc/apt/sources.list.d/dotnetdev.list'
sudo apt-get update && sudo apt-get install -y --allow-unauthenticated iotop mc git htop lsof
sudo apt-get install dotnet-sdk-2.0.2 -y --allow-unauthenticated

sudo dd if=/dev/zero of=/swap bs=1M count=768
sudo mkswap /swap
sudo swapon /swap

work=$HOME/Touch.Gallery
rm -rf $work
mkdir -p $work
cd $work
git clone https://github.com/devizer/Touch.Gallery Touch.Gallery
echo 3 | sudo tee /proc/sys/vm/drop_caches > /dev/null
pushd Touch.Gallery/src
dotnet publish -c Release -o ../../Touch.Gallery-bin | tee $HOME/BUILD-Touch.Gallery.log
popd
# sudo swapoff /swap

rm -rf Touch.Gallery
cd Touch.Gallery-bin
echo 3 | sudo tee /proc/sys/vm/drop_caches > /dev/null
sudo bash -c 'ASPNETCORE_URLS=http://0.0.0.0:80\;http://0.0.0.0:8080\;http://0.0.0.0:5000 dotnet Gallery.MVC.dll' | tee $HOME/RUN-Touch.Gallery.log
