echo'
http://touch-galleries.devizer.tech HEAD
' >/tmp/touch-galleries.urls

siege -t30s -c25 -b -f /tmp/touch-galleries.urls

