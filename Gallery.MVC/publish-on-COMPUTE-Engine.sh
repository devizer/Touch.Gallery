#!/bin/bash
# set -e 
if [[ -z "$HOME" ]]; then export HOME=/root; fi; mkdir -p "$HOME"
echo "Booted at $(date)" >> $HOME/RESTART.log

if [[ -z "$DOTNET_SDK_VER" ]]; then DOTNET_SDK_VER=2.2; fi

export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
dotnet --info >/dev/null 2>&1 && hasDotNet=true
if [ -z "$hasDotNet" ]; then
  export DOTNET_CLI_TELEMETRY_OPTOUT=1
  time (sudo apt-get update && sudo apt-get install -y --allow-unauthenticated iotop mc git htop lsof apt-transport-https)
  # old
  # curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
  # sudo mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg
  # echo deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-$(lsb_release -c -s)-prod $(lsb_release -c -s) main | sudo tee /etc/apt/sources.list.d/dotnetdev.list > /dev/null
  # sudo apt-get install dotnet-sdk-2.1.105 -y --allow-unauthenticated
  # new
  wget -q -O packages-microsoft-prod.deb https://packages.microsoft.com/config/ubuntu/$(lsb_release -s -r)/packages-microsoft-prod.deb
  sudo dpkg -i packages-microsoft-prod.deb
  time (sudo apt-get update && sudo apt-get install -y --allow-unauthenticated dotnet-sdk-$DOTNET_SDK_VER)
  echo "Dotnet $DOTNET_SDK_VER installed $(date)" >> $HOME/RESTART.log
fi
sudo sync

if [ ! -f "/swap" ]; then
  sudo bash -c 'dd if=/dev/zero of=/swap bs=1M count=1700 && mkswap /swap && swapon /swap'
  echo "Swap Created $(date)" >> $HOME/RESTART.log
fi
sudo mkswap /swap >/dev/null 2>&1 || true
sudo swapon /swap >/dev/null 2>&1 || true
sudo sync

target=/Touch-Galleries.App
if [ ! -f "$target/.done" ]; then
  # OPTIONAL BUILD

    work=$HOME/Touch.Gallery
    rm -rf $work
    mkdir -p $work
    cd $work
    git clone https://github.com/devizer/Touch.Gallery Touch.Gallery
    echo 3 | sudo tee /proc/sys/vm/drop_caches > /dev/null
    echo "Src downloaded $(date)" >> $HOME/RESTART.log
    pushd Touch.Gallery
    rm -rf .git
    cd Gallery.MVC
    dotnet publish -c Release -r linux-x64 -o ../../Touch.Gallery-bin | tee $HOME/BUILD-Touch.Gallery.log
    popd
    rm -rf Touch.Gallery
    echo "Src builded $(date)" >> $HOME/RESTART.log


    target=/Touch-Galleries.App
    sudo mkdir -p $target
    ver=ver-$(date +"%Y-%m-%d-%H-%M-%S")
    echo NEW VERSION is: $target/$ver
    sudo mv Touch.Gallery-bin $target/$ver
    for prev in $(ls -1 $target | grep -v $ver | grep ver); do
      echo REMOVING PREV version: $prev
      sudo rm -rf $target/$prev
    done

    echo $ver > $target/.done
else
  echo "App already built. Build skipped at $(date)" >> $HOME/RESTART.log
  cd $target
  ver=$(ls -1 | grep 'ver-' | sort -r | head -n 1)
fi; 
sync

echo "Try to kill running process"
sudo kill $(sudo cat /var/run/touch-galleries.pid) || true
echo Starting APP from folder $target/$ver
cd $target/$ver
echo 3 | sudo tee /proc/sys/vm/drop_caches > /dev/null
echo "Starting the App at $(date)" >> $HOME/RESTART.log
# sudo bash -c 'ASPNETCORE_URLS=http://0.0.0.0:80\;http://0.0.0.0:8080\;http://0.0.0.0:5000 dotnet Gallery.MVC.dll' | tee $HOME/RUN-Touch.Gallery.log 2>&1
(sudo nohup bash -c 'ASPNETCORE_URLS=http://0.0.0.0:80\;http://0.0.0.0:8080\;http://0.0.0.0:5000 dotnet Gallery.MVC.dll' | sudo tee $HOME/RUN-Touch.Gallery.log 2>&1) &

echo "Upgrade finished $(date)" >> $HOME/RESTART.log
