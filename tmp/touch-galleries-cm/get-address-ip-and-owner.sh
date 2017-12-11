echo $(cat report-addresses.json 2>/dev/null| jq -r '.[0] | .address ')
static_ip_address=$(cat report-addresses.json 2>/dev/null| jq -r '.[0] | .address ')


echo $(cat report-addresses.json 2>/dev/null| jq -r '.[0] | .users[0] ')
old_owner=$(cat report-addresses.json 2>/dev/null| jq -r '.[0] | .users[0] ')

