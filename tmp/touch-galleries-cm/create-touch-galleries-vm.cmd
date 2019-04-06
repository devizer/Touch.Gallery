set image=ubuntu-1604-xenial-v20180424
set image=ubuntu-1804-bionic-v20180426b
set image=ubuntu-1804-bionic-v20190404

gcloud compute --project=noted-terra-234718 instances create "touch-galleries-vm" --zone=us-east1-b --machine-type=f1-micro --subnet=default --address=35.237.72.41 --network-tier=PREMIUM --can-ip-forward --maintenance-policy=TERMINATE ^
  --service-account=456527588487-compute@developer.gserviceaccount.com --scopes=https://www.googleapis.com/auth/datastore,https://www.googleapis.com/auth/logging.write,https://www.googleapis.com/auth/monitoring.write,https://www.googleapis.com/auth/trace.append,https://www.googleapis.com/auth/bigtable.data,https://www.googleapis.com/auth/userinfo.email,https://www.googleapis.com/auth/servicecontrol,https://www.googleapis.com/auth/service.management.readonly,https://www.googleapis.com/auth/devstorage.read_write ^
  --metadata "startup-script=export HOME=/root; export HTTP_PORT=5050; mkdir -p /root; cd /root; wget --no-check-certificate -O publish-on-COMPUTE-Engine.sh https://github.com/devizer/Touch.Gallery/raw/master/Gallery.MVC/publish-on-COMPUTE-Engine.sh | tee wget.log 2>&1; sudo bash publish-on-COMPUTE-Engine.sh" ^
  --min-cpu-platform="Intel Skylake" --tags=http-server --image=ubuntu-1804-bionic-v20190404 --image-project=ubuntu-os-cloud --boot-disk-size=10GB --boot-disk-type=pd-standard --boot-disk-device-name=touch-galleries-vm

gcloud compute --project=noted-terra-234718 firewall-rules create default-allow-http --direction=INGRESS --priority=1000 --network=default --action=ALLOW --rules=tcp:80 --source-ranges=0.0.0.0/0 --target-tags=http-server

goto exi

gcloud beta compute --project "My First Project" instances create "touch-galleries-vm" --description "It serves touch-galleries.xyz" --zone "us-east1-b" --machine-type "f1-micro" --subnet "default" --address 35.237.72.41 \
  --metadata "startup-script=export HOME=/root; mkdir -p /root; cd /root; wget --no-check-certificate -O publish-on-COMPUTE-Engine.sh https://github.com/devizer/Touch.Gallery/raw/master/Gallery.MVC/publish-on-COMPUTE-Engine.sh | tee wget.log 2>&1; sudo bash publish-on-COMPUTE-Engine.sh" \
  --maintenance-policy "MIGRATE" --service-account "123571025325-compute@developer.gserviceaccount.com" --scopes "https://www.googleapis.com/auth/datastore","https://www.googleapis.com/auth/logging.write","https://www.googleapis.com/auth/monitoring.write","https://www.googleapis.com/auth/trace.append","https://www.googleapis.com/auth/servicecontrol","https://www.googleapis.com/auth/service.management.readonly","https://www.googleapis.com/auth/devstorage.read_only" --min-cpu-platform "Automatic" --tags "http-server" --image "$image" --image-project "ubuntu-os-cloud" --boot-disk-size "10" --boot-disk-type "pd-standard" --boot-disk-device-name "touch-galleries-vm"

export HOME=/root; sudo mkdir -p /root; wget --no-check-certificate -O publish-on-COMPUTE-Engine.sh-tmp https://github.com/devizer/Touch.Gallery/raw/master/Gallery.MVC/publish-on-COMPUTE-Engine.sh | tee wget.log 2>&1; sudo bash publish-on-COMPUTE-Engine.sh-tmp | tee "$HOME/full-boot-log.txt"


gcloud compute --project=noted-terra-234718 instances create instance-2 --zone=us-east1-b --machine-type=n1-standard-1 --subnet=default --network-tier=PREMIUM --metadata=startup-script=startup-script=export\ HOME=/root\;\ mkdir\ -p\ /root\;\ cd\ /root\;\ wget\ --no-check-certificate\ -O\ publish-on-COMPUTE-Engine.sh\ https://github.com/devizer/Touch.Gallery/raw/master/Gallery.MVC/publish-on-COMPUTE-Engine.sh\ \|\ tee\ wget.log\ 2\>\&1\;\ sudo\ bash\ publish-on-COMPUTE-Engine.sh --maintenance-policy=MIGRATE --service-account=456527588487-compute@developer.gserviceaccount.com --scopes=https://www.googleapis.com/auth/devstorage.read_only,https://www.googleapis.com/auth/logging.write,https://www.googleapis.com/auth/monitoring.write,https://www.googleapis.com/auth/servicecontrol,https://www.googleapis.com/auth/service.management.readonly,https://www.googleapis.com/auth/trace.append --image=debian-9-stretch-v20190326 --image-project=debian-cloud --boot-disk-size=10GB --boot-disk-type=pd-standard --boot-disk-device-name=instance-2

:exi

