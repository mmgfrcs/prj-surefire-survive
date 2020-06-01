#!/bin/bash
git describe --abbrev=0 > ver.txt
git rev-list $(cat ver.txt).. --count > patch.txt
echo "." > separator.txt
cat ver.txt separator.txt patch.txt | tr -d '\n' > version.txt