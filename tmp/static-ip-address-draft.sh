old_owner=touch-galleries-vm
new_owner=touch-galleries-vm-next-2017-12-10-13-28-36
gcloud compute instances delete-access-config --project touch-galleries --zone us-east1-b "$old_owner" --access-config-name external-nat
gcloud compute instances delete-access-config --project touch-galleries --zone us-east1-b "$new_owner" --access-config-name external-nat

old_owner=touch-galleries-vm
new_owner=touch-galleries-vm-next-2017-12-10-13-28-36

echo "Removing Ethemeral IP address from newly created VM [$vm_name]"
gcloud compute instances delete-access-config --project $project --zone $zone "$vm_name" --access-config-name external-nat

if [ -n "$old_owner" ]; then
  echo "Remove Static IP address [$static_ip_address] from old owner [$old_owner]"
  gcloud compute instances delete-access-config --project $project --zone $zone "$old_owner" --access-config-name external-nat
fi

echo "Assigning Static IP to newly created vm [$vm_name]"
gcloud compute instances add-access-config --project $project --zone $zone "$vm_name" --access-config-name external-nat --address $static_ip_address
