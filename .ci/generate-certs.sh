#!/usr/bin/env bash

set -eo pipefail

script_path=$(dirname $(realpath -s $0))
certs_dir="$script_path/certs"
opensearch_dir="$script_path/opensearch"

openssl_conf="$certs_dir/openssl.conf"

root_ca_key="$certs_dir/root-ca.key"
root_ca_crt="$certs_dir/root-ca.crt"
root_ca_pem="$opensearch_dir/root-ca.pem"

esnode_key="$certs_dir/esnode.key"
esnode_key_pem="$opensearch_dir/esnode-key.pem"
esnode_csr="$certs_dir/esnode.csr"
esnode_crt="$certs_dir/esnode.crt"
esnode_pem="$opensearch_dir/esnode.pem"

kirk_key="$certs_dir/kirk.key"
kirk_csr="$certs_dir/kirk.csr"
kirk_crt="$certs_dir/kirk.crt"
kirk_p12="$certs_dir/kirk.p12"

common_crt_args="-extfile $openssl_conf -days 36500 -CA $root_ca_crt -CAkey $root_ca_key -CAcreateserial"
common_csr_args="-config $openssl_conf -days 36500"

# Stop Git Bash / MSYS / Cygwin from mangling the cert subjects
subj_prefix=""
if [[ "$(uname)" == MINGW* ]]; then
    subj_prefix="/"
fi

if [[ ! -f $root_ca_key ]]; then
    rm -f $root_ca_crt
    openssl genrsa -out $root_ca_key
fi

if [[ ! -f $root_ca_crt ]]; then
    rm -f *.crt $root_ca_pem
    openssl req -new -x509 \
        -key $root_ca_key \
        -subj "$subj_prefix/DC=com/DC=example/O=Example Com Inc./OU=Example Com Inc. Root CA/CN=Example Com Inc. Root CA" \
        $common_csr_args -extensions root-ca \
        -out $root_ca_crt
fi

if [[ ! -f $root_ca_pem ]]; then
    cp $root_ca_crt $root_ca_pem
fi

if [[ ! -f $esnode_key ]]; then
    rm -f $esnode_csr $esnode_key_pem
    openssl genrsa -out $esnode_key
fi

if [[ ! -f $esnode_key_pem ]]; then
    openssl pkcs8 -topk8 -in $esnode_key -nocrypt -out $esnode_key_pem
fi

if [[ ! -f $esnode_csr ]]; then
    rm -f $esnode_crt
    openssl req -new \
        $common_csr_args \
        -key $esnode_key \
        -subj "$subj_prefix/DC=de/L=test/O=node/OU=node/CN=node-0.example.com" \
        -out $esnode_csr
fi

if [[ ! -f $esnode_crt ]]; then
    rm -f $esnode_pem
    openssl x509 -req -in $esnode_csr $common_crt_args -extensions esnode -out $esnode_crt
fi

if [[ ! -f $esnode_pem ]]; then
    cp $esnode_crt $esnode_pem
fi

if [[ ! -f $kirk_key ]]; then
    rm -f $kirk_csr
    openssl genrsa -out $kirk_key
fi

if [[ ! -f $kirk_csr ]]; then
    rm -f $kirk_crt
    openssl req -new \
        $common_csr_args \
        -subj "$subj_prefix/C=de/L=test/O=client/OU=client/CN=kirk" \
        -key $kirk_key \
        -out $kirk_csr
fi

if [[ ! -f $kirk_crt ]]; then
    rm -f $kirk_p12
    openssl x509 -req -in $kirk_csr $common_crt_args -extensions kirk -out $kirk_crt
fi

if [[ ! -f $kirk_p12 ]]; then
    openssl pkcs12 -export \
        -in $kirk_crt \
        -inkey $kirk_key \
        -descert \
        -passout pass:kirk \
        -out $kirk_p12
fi
