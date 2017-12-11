#!/bin/bash

function GoProject {
 pid=$1
 echo Project_ID: $pid

 echo -e "\n ---------- VERSIONS of [$pid] ----------- "
 # all version excluding latest, including active stopped
 gcloud app versions list --project=$pid | grep STOPPED | grep -v " 1.00 " > .stopped-versions-$pid 2>/dev/null
 echo Stopped Versions of [$pid]:
 cat .stopped-versions-$pid

 cat .stopped-versions-$pid | awk '{print $1 " " $2}' | while read pver ; do 
  cmd="gcloud app versions delete --project=$pid -s $pver -q"
  echo "  $cmd"
  bash -c "$cmd"
 done

 echo -e "\n --------------- IMAGES of [$pid] ------------- "
 # all containers except of latest
 gcloud container images list --project=$pid --repository=us.gcr.io/$pid/appengine | sort | tail -n +2 | awk 'NR>1{print buf}{buf = $0}' > .images-$pid
 cat .images-$pid | while read container_id; do
   echo PREV Container: $container_id
   cmd="gcloud container images delete --project=$pid $container_id -q"
   echo "$cmd"
   bash -c "$cmd"
 done

 echo -e "\n ------------ LAYERS in a STORAGE of [$pid] ---------------- "
 gsutil ls -l -p $pid gs://us.artifacts.$pid.appspot.com/containers/images | sort -k 2

 echo ""
}


gcloud projects list | tail -n +2 | awk '{print $1}' > .project_ids
cat .project_ids | while read project_id ; do GoProject $project_id ; done
