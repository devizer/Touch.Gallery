#!/bin/bash
if [[ -z "$HOME" ]]; then export HOME=/root; fi; mkdir -p "$HOME"
echo "Booted at $(date)" > $HOME/RESTART.log
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-xenial-prod xenial main" > /etc/apt/sources.list.d/dotnetdev.list'
time (sudo apt-get update && sudo apt-get install -y --allow-unauthenticated iotop mc git htop lsof \
 && sudo apt-get install dotnet-sdk-2.0.3 -y --allow-unauthenticated)

sudo bash -c 'dd if=/dev/zero of=/swap bs=1M count=1700 && mkswap /swap && swapon /swap'

work=$HOME/Touch.Gallery
rm -rf $work
mkdir -p $work
cd $work
git clone https://github.com/devizer/Touch.Gallery Touch.Gallery
echo 3 | sudo tee /proc/sys/vm/drop_caches > /dev/null
pushd Touch.Gallery/Gallery.MVC
dotnet publish -c Release -o ../../Touch.Gallery-bin | tee $HOME/BUILD-Touch.Gallery.log
popd
rm -rf Touch.Gallery
# sudo swapoff /swap

cd Touch.Gallery-bin
echo 3 | sudo tee /proc/sys/vm/drop_caches > /dev/null
echo "Starting the App at $(date)" >> $HOME/RESTART.log
sudo bash -c 'ASPNETCORE_URLS=http://0.0.0.0:80\;http://0.0.0.0:8080\;http://0.0.0.0:5000 dotnet Gallery.MVC.dll' | tee $HOME/RUN-Touch.Gallery.log 2>&1

