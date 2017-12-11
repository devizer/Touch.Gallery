# gcloud beta compute --project "touch-galleries" instances create "touch-galleries-vm" --description "It serves touch-galleries.xyz" --zone "us-east1-b" --machine-type "f1-micro" --subnet "default" \
#   --address 35.196.19.138 \
#   --metadata "startup-script=export HOME=/root; mkdir -p /root; cd /root; wget --no-check-certificate -O publish-on-COMPUTE-Engine.sh https://github.com/devizer/Touch.Gallery/raw/master/Gallery.MVC/publish-on-COMPUTE-Engine.sh | tee wget.log 2>&1; sudo bash publish-on-COMPUTE-Engine.sh" \
#  --maintenance-policy "MIGRATE" --service-account "123571025325-compute@developer.gserviceaccount.com" --scopes "https://www.googleapis.com/auth/datastore","https://www.googleapis.com/auth/logging.write","https://www.googleapis.com/auth/monitoring.write","https://www.googleapis.com/auth/trace.append","https://www.googleapis.com/auth/servicecontrol","https://www.googleapis.com/auth/service.management.readonly","https://www.googleapis.com/auth/devstorage.read_only" --min-cpu-platform "Automatic" --tags "http-server" --image "ubuntu-1604-xenial-v20171208" --image-project "ubuntu-os-cloud" --boot-disk-size "10" --boot-disk-type "pd-standard" --boot-disk-device-name "touch-galleries-vm"

RED='\033[0;31m'
YELLOW='\033[1;33m'
L_BLUE='\033[1;34m'
L_GREEN='\033[1;32m'
L_VALUE='\033[1;35m'
L_MESSAGE=$L_GREEN
NC='\033[0m' 

command -v jq 1>/dev/null 2>&1 || (apt-get update && apt-get install -y jq)

function check_exit_code() {
  exit_code=$1; operation=$2
  if [ ! "$exit_code" -eq 0 ]; then print_error "Failed: ${operation}. Exit Code: $exit_code"; exit 1; fi
}

function new_line() { printf "\n"; }
function print_error() { printf "${RED}$1${NC}\n"; }
function print_message() { printf "${L_GREEN}$1${NC}\n"; }
function print_values() {
  while [[ $# > 0 ]] ; do
    type=$1; shift; msg=$1; shift
    if [ "$type" == "M" ]; then printf "${L_GREEN}$msg${NC} "; fi
    if [ "$type" == "V" ]; then printf "${L_VALUE}[$msg]${NC} "; fi
  done  
  printf "\n"
}


static_ip_name=touch-galleries
project_name=touch-galleries
zone=us-east1-b

date=$(date +"%Y-%m-%d-%H-%M-%S")
vm_base_name=touch-galleries-vm-next
vm_name=$vm_base_name-$date
disk_name=$vm_name

print_values M "Looking for static IP address named" V "${static_ip_name}" M "in the project" V "$project_name"
gcloud compute addresses list --project "$project_name" --format yaml --filter="name:(${static_ip_name})" >report-addresses.yaml
gcloud compute addresses list --project "$project_name" --format json --filter="name:(${static_ip_name})" >report-addresses.json
static_ip_address=$(cat report-addresses.yaml | grep address: | awk '{print $2}' )
if [ -z "$static_ip_address" ]; then
  print_error "\nERROR: Unknown static IP address [$static_ip_name] in project [$project_name]\n"
  exit 1;
fi

new_line
print_values M "FOUND: Static IP address" V "$static_ip_name" M "in the project" V "$project_name" M "is" V "$static_ip_address"
gcloud compute addresses list --project "$project_name" --filter="name:(${static_ip_name})"

old_owner_ref=$(cat report-addresses.yaml | tail -n 1 | grep -E "^- http" | awk '{print $2}')
old_owner=$(basename $old_owner_ref)

new_line
if [ -n $old_owner_ref ] || [ -n $old_owner ]; then
  print_values M "OLD Owner ref:" V "$old_owner_ref"
  print_values M "OLD Owner:" V "$old_owner"
else
  print_message "It seems old owner of static ip is NO-BO-DY"
fi

prev_vm_list=$(gcloud compute instances list --project "$project_name" | tail -n +2 | awk '{print $1}' | grep "$vm_base_name" | grep -v "$vm_name")
print_values M "Previuos VM will be deleted only after routing to static IP" V "$static_ip_address" 
print_values M "Prev VMs are: "
for prev in $prev_vm_list; do
  print_values M "   - " V "$prev"
done

new_line
print_values M "Creating new instance" V "$vm_name"
gcloud beta compute --format=yaml --project "$project_name" instances create "$vm_name" --zone "us-east1-b" --machine-type "f1-micro" --subnet "default" \
  --metadata "startup-script=export HOME=/root; sudo mkdir -p /root; wget --no-check-certificate -O publish-on-COMPUTE-Engine.sh-tmp https://github.com/devizer/Touch.Gallery/raw/master/Gallery.MVC/publish-on-COMPUTE-Engine.sh | tee wget.log 2>&1; sudo bash publish-on-COMPUTE-Engine.sh-tmp" \
  --maintenance-policy "MIGRATE" --service-account "123571025325-compute@developer.gserviceaccount.com" \
  --scopes "https://www.googleapis.com/auth/datastore","https://www.googleapis.com/auth/logging.write","https://www.googleapis.com/auth/monitoring.write","https://www.googleapis.com/auth/trace.append","https://www.googleapis.com/auth/servicecontrol","https://www.googleapis.com/auth/service.management.readonly","https://www.googleapis.com/auth/devstorage.read_only" \
  --min-cpu-platform "Automatic" --tags "http-server" --image "ubuntu-1604-xenial-v20171208" --image-project "ubuntu-os-cloud" --boot-disk-size "10" --boot-disk-type "pd-standard" --boot-disk-device-name "$disk_name" \
  >report-create.yaml

code=$?; check_exit_code "$code" "Creating new instance [$vm_name]"

temp_ip=$(cat report-create.yaml | grep natIP: | awk '{print $2}' )

print_values M "------- New VM" V "${vm_name}" M "ethemeral IP is" V "$temp_ip" M "-----------"

  printf "\nWaiting for http://${temp_ip} "
  counter=0; total=1200;
  started=""
  while [ $counter -lt $total ]; do
    counter=$((counter+1));
    wget -t 1 -T 1 -q -nv -O /dev/null http://$temp_ip 2>/dev/null && started="yes" || true
    if [ -n "$started" ]; then print_message "\nHTTP SERVER ${temp_ip} started in $counter seconds"; break; else (sleep 1; printf "."$counter); fi
  done

if [ -z $started ]; then
  print_error "\nERROR: HTTP Service not started correctly in $total seconds";
  print_error "Instance $vm_name should be deleted"
  exit 2;
fi;

print_values M "\nRemoving Ethemeral IP address from newly created VM" V "$vm_name"
time (gcloud compute instances delete-access-config --project "$project_name" --zone "$zone" --access-config-name external-nat "$vm_name")

if [ -n "$old_owner" ]; then
  print_values M "\nRemoving Static IP address" V "$static_ip_address" M "from old owner" V "$old_owner"
  time (gcloud compute instances delete-access-config --project "$project_name" --zone "$zone" "$old_owner" --access-config-name external-nat)
fi

print_values M "\nAssigning Static IP to newly created VM" V "$vm_name"
time (gcloud compute instances add-access-config --project "$project_name" --zone "$zone" "$vm_name" --access-config-name external-nat --address $static_ip_address)


for prev in $prev_vm_list; do
  print_values M "Deleting prev VM" V "$prev"
  gcloud compute instances delete -q "$prev" --project "$project_name" --zone "$zone"
done
