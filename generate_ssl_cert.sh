#!/bin/bash
######################
# Become a Certificate Authority
######################

set +e

# Generate private key
openssl genrsa -des3 -out myCA.key 2048
# Generate root certificate
openssl req -x509 -new -nodes -key myCA.key -sha256 -days 825 -out myCA.pem

######################
# Create CA-signed certs
######################

NAME=nikkedomains # Use your own domain name
# Generate a private key
openssl genrsa -out $NAME.key 2048
# Create a certificate-signing request
openssl req -new -key $NAME.key -out $NAME.csr
# Create a config file for the extensions
>$NAME.ext cat <<-EOF
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment
subjectAltName = @alt_names
[alt_names]
DNS.1 = *.nikke-kr.com
DNS.2 = aws-na-dr.intlgame.com
DNS.3 = *.intlgame.com
DNS.4 = *.playerinfinite.com
IP.1 = 192.168.3.13 # Optionally, add an IP address (if the connection which you have planned requires it)
EOF
# Create the signed certificate
openssl x509 -req -in $NAME.csr -CA myCA.pem -CAkey myCA.key -CAcreateserial \
-out $NAME.crt -days 825 -sha256 -extfile $NAME.ext

# Convert CA Cert to pfx
openssl pkcs12 -export -out myCA.pfx -inkey myCA.key -in myCA.pem

# Convert site cert to pfx
openssl pkcs12 -export -out site.pfx -inkey $NAME.key -in $NAME.crt

# copy certs
cp site.pfx nksrv/site.pfx
cp myCA.pem ServerSelector/myCA.pem
cp myCA.pfx ServerSelector/myCA.pfx

# clean up
rm nikkedomains* myCA*