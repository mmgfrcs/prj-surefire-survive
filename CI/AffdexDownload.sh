#!/bin/bash
curl -c cookies.txt -s -o /dev/null "https://docs.google.com/uc?export=download&id=1pRZfb4iom051pn-yrpuUMjwNUx1fYOoS"
cat cookies.txt | egrep -io "download_warning_[0-9]+_[^[:space:]]+[[:space:]][^[:space:]]{4}" | egrep -io "[^[:space:]]{4}$" > link.txt
echo "https://docs.google.com/uc?export=download&confirm=" > combi1.txt
echo "&id=1pRZfb4iom051pn-yrpuUMjwNUx1fYOoS" > combi2.txt
cat combi1.txt link.txt combi2.txt | tr -d '\n' > newlink.txt
curl -b cookies.txt -OJL $(cat newlink.txt)