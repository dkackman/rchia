./rchia bech32 address-from-hash 0xdb96f26f5245baec3c4e06da21d9d50c8718f9920b5e9354c3a936fc0ee49a66
./rchia bech32 hash-from-address xch1mwt0ym6jgkawc0zwqmdzrkw4pjr337vjpd0fx4xr4ym0crhynfnq96pztp
./rchia blocks block -h 100 -ep wsl
./rchia blocks recent -ep wsl
./rchia blocks adds-and-removes -h 100 -ep wsl
./rchia connections list -ep wsl
./rchia connections prune -ep wsl
./rchia endpoints add example https://example.com c:\example.crt c:\example.key
./rchia endpoints set-default example
./rchia endpoints show example
./rchia endpoints list
./rchia endpoints test example
./rchia endpoints remove example
./rchia farm challenges -ep wsl
./rchia farm summary -ep wsl
./rchia keys show -ep wsl
./rchia keys generate-and-print -ep wsl
./rchia node netspace -ep wsl
./rchia node ping -ep wsl
./rchia node version -ep wsl
./rchia node status -ep wsl
./rchia node stop -ep wsl
./rchia plotnft get-login-link -ep wsl -l 0x...
./rchia plotnft inspect -ep wsl -i 2
./rchia plotnft show -ep wsl -i 2
./rchia plots list -ep wsl
./rchia plots directories -ep wsl
./rchia plots plotters -ep wsl
./rchia plots queue -ep wsl
./rchia plots log -ep wsl
./rchia services list -ep wsl
./rchia wallets get-address -ep wsl
./rchia wallets get-transaction -tx 0x52fe2f8d65f1ca511a4d46c7fc0f4a7dfdac64af5df6caea8ee1cff18b8c3829 -ep wsl
./rchia wallets list-transactions -ep wsl
./rchia wallets show -ep wsl
