gcloud beta compute --project "touch-galleries" instances create "touch-galleries-vm" --description "It serves touch-galleries.xyz" --zone "us-east1-b" --machine-type "f1-micro" --subnet "default" --address 35.196.19.138 \
  --metadata "startup-script=export HOME=/root; mkdir -p /root; cd /root; wget --no-check-certificate -O publish-on-COMPUTE-Engine.sh https://github.com/devizer/Touch.Gallery/raw/master/Gallery.MVC/publish-on-COMPUTE-Engine.sh | tee wget.log 2>&1; sudo bash publish-on-COMPUTE-Engine.sh" \
  --maintenance-policy "MIGRATE" --service-account "123571025325-compute@developer.gserviceaccount.com" --scopes "https://www.googleapis.com/auth/datastore","https://www.googleapis.com/auth/logging.write","https://www.googleapis.com/auth/monitoring.write","https://www.googleapis.com/auth/trace.append","https://www.googleapis.com/auth/servicecontrol","https://www.googleapis.com/auth/service.management.readonly","https://www.googleapis.com/auth/devstorage.read_only" --min-cpu-platform "Automatic" --tags "http-server" --image "ubuntu-1604-xenial-v20171208" --image-project "ubuntu-os-cloud" --boot-disk-size "10" --boot-disk-type "pd-standard" --boot-disk-device-name "touch-galleries-vm"