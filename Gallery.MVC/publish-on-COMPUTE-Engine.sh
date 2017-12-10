#!/bin/bash
if [[ -z "$HOME" ]]; then export HOME=/root; fi; mkdir -p "$HOME"
echo "Booted at $(date)" > $HOME/RESTART.log
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
command -v dotnet >/dev/null 2>&1 && hasDotNet=true
if [ -z "$hasDotNet" ]; then
  sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-xenial-prod xenial main" > /etc/apt/sources.list.d/dotnetdev.list'
  time (sudo apt-get update && sudo apt-get install -y --allow-unauthenticated iotop mc git htop lsof \
    && sudo apt-get install dotnet-sdk-2.0.3 -y --allow-unauthenticated)
fi

if [ ! -f "/swap" ]; then
  sudo bash -c 'dd if=/dev/zero of=/swap bs=1M count=1700 && mkswap /swap && swapon /swap'
fi

work=$HOME/Touch.Gallery
rm -rf $work
mkdir -p $work
cd $work
git clone https://github.com/devizer/Touch.Gallery Touch.Gallery
echo 3 | sudo tee /proc/sys/vm/drop_caches > /dev/null
pushd Touch.Gallery
rm -rf .git
cd Gallery.MVC
dotnet publish -c Release -o ../../Touch.Gallery-bin | tee $HOME/BUILD-Touch.Gallery.log
popd
rm -rf Touch.Gallery
# sudo swapoff /swap

target=/Touch-Galleries.App
sudo mkdir -p $target
ver=$(date +"%Y-%m-%d-%H-%M-%S")
echo NEW VERSION is: $target/$ver
time (sudo cp -r Touch.Gallery-bin $target/$ver)
for prev in $(ls -1 $target | grep -v $ver); do
  echo REMOVING PREV version: $prev
  sudo rm -rf $target/$prev
done

sudo kill $(cat /var/run/touch-galleries.pid)
cd $target/$ver
echo 3 | sudo tee /proc/sys/vm/drop_caches > /dev/null
echo "Starting the App at $(date)" >> $HOME/RESTART.log
# sudo bash -c 'ASPNETCORE_URLS=http://0.0.0.0:80\;http://0.0.0.0:8080\;http://0.0.0.0:5000 dotnet Gallery.MVC.dll' | tee $HOME/RUN-Touch.Gallery.log 2>&1
export ASPNETCORE_URLS=http://0.0.0.0:80\;http://0.0.0.0:8080\;http://0.0.0.0:5000
(sudo nohup dotnet Gallery.MVC.dll | tee $HOME/RUN-Touch.Gallery.log 2>&1) &
