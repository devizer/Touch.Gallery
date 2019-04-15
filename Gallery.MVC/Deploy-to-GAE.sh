#!/bin/bash
# wget -q -nv --no-check-certificate -O - https://github.com/devizer/Touch.Gallery/raw/master/Gallery.MVC/Deploy-to-GAE.sh | bash

set -e 
set -u
REPO=https://github.com/devizer/Touch.Gallery
Bucket=pet-projects-europe
echo "SOURCE' REPO: [$REPO]"
echo "TARGET BUCKET: [$Bucket]"


function clear_cache () { echo 3 | sudo tee /proc/sys/vm/drop_caches > /dev/null; }
dir=$(basename $REPO)
work=$HOME/transient-builds
if [[ -d "/transient-builds" ]]; then work=/transient-builds; fi
echo "WORK/BUILD DIRECTORY: [$work/$dir]"
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
time sudo bash -c 'tar cf - Touch.Gallery | pv | gzip -3 > Touch.Gallery.tar.gz'; ls -la Touch.Gallery.tar.gz
rm -rf Touch.Gallery
gsutil cp Touch.Gallery.tar.gz gs://$Bucket/
popd
rm -rf $dir
echo "Src builded $(date)" 
