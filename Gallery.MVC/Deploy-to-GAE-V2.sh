#!/bin/bash
set -e 
REPO=https://github.com/devizer/Touch.Gallery


function clear_cache () { echo 3 | sudo tee /proc/sys/vm/drop_caches > /dev/null; }
dir=$(basename $REPO)
work=$HOME/transient-builds
if [[ -d "/transient-builds" ]]; then work=/transient-builds; fi
mkdir -p $work; cd $work
rm -rf $dir
git clone $REPO $dir
pushd $dir
rm -rf .git
clear_cache
echo "Src downloaded $(date)" 
cd Gallery.MVC
dotnet publish -c Release -r linux-x64 --self-contained -o ./bin/Touch.Gallery
cd bin
time sudo bash -c 'tar cf - Touch.Gallery | pv | gzip -9 > Touch.Gallery.tar.gz'; ls -la Touch.Gallery.tar.xz
rm -rf Touch.Gallery
gsutil cp Touch.Gallery.tar.gz gs://pet-projects-binaries/
popd
rm -rf $dir
echo "Src builded $(date)" 
