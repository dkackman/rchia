# rchia

Remote management CLI for [chia nodes](https://github.com/Chia-Network/chia-blockchain).

[![.NET](https://github.com/dkackman/rchia/actions/workflows/dotnet.yml/badge.svg)](https://github.com/dkackman/rchia/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/dkackman/rchia/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/dkackman/rchia/actions/workflows/codeql-analysis.yml)

## Introduction

A cross platform (Linux, Windows, MacOS) command line utility that mirrors the `chia` CLI, but uses RPC rather than running locally on the node. This allows management of any number of nodes from a central location as long as their RPC interface is exposed on the network.

## Build and Run

Install [the .net5 sdk](https://dotnet.microsoft.com/download)

```bash
dotnet build src
cd src/rchia/bin/Debug/net5.0/
./rchia --help
```

## Example

The details of the endpoint can be specified in the following ways:

### On the command line

```bash
./rchia show -s --endpoint-uri https://node1:8555 --cert-path ~/certs/node1/private_full_node.crt --key-path ~/certs/node1/private_full_node.key
```

### By using the `chia` config

```bash
./rchia show -s --use-default-chia-config
```

### Using saved endpoint connections

```bash
./rchia endpoints --add node1 https://node1:8555 ~/certs/node1/private_full_node.crt ~/certs/node1/private_full_node.key
./rchia show -s --saved-endpoint node1
```

### Example output

```bash
user@computer:~$ ./rchia show -s --saved-endpoint node1 -v
Using endpoint https://node1:8555/ 

Current Blockchain Status: Full Node Synced
Peak: Hash:0xfa277f6ea103b5c2b6ad77ae6e909ab985e1b9312f286908e967ef644beec432
      Time: Fri Aug 27 2021 16:57:06 CDT             Height: 509806

Estimated network space: 42.200 TiB
Current difficulty: 21760
Current VDF sub_slot_iters: 71303168
Total iterations since the start of the blockchain: 1229673178499

  Height: | Hash:
   509806 | fa277f6ea103b5c2b6ad77ae6e909ab985e1b9312f286908e967ef644beec432
   509805 | cbe52496956fb10a9fa430f88e7101e8d3b8ed75066a573ae435a84ee7baa817
   509804 | a606adfaec3855413a57fddc111e6b7cf0670b881061063865f1890eb6825be8
   509803 | 6ef141f681e5e86dd6cec0bb84439149c059c224c8f3233aca0f54dcf0f029ea
   509802 | 6851b326eec500206755264bee3fe18313fd7bb87fc8b8a7b87d77075ce13a24
   509801 | 79d48e9bc5e5c4003774c4e841348a67d85dbad288ec4d84e7d173bacac96f81
   509800 | eb04e5eda4ff63f368b46002bb78d0808eec5d9b7c987d649d50b9eac547b36a
   509799 | d87ce5ff511bc5ae3e97e691548a4c2a24afe880f0ea80c20de0ede3676623d9
   509798 | 40406a69495a82b901801e1614584b5501c99193b93fc2f542f64ade4797482f
   509797 | 1dc42c681606442075522895212d62629a9767dca3d152fa18e5bf37189c76d0
```

## Usage for the Show Command
```bash
user@computer:~$ ./rchia show --help
rchia 1.0.0
Copyright (C) 2021 rchia

  -a, --add-connection          [URI] Connect to another Full Node by ip:port

  -b, --block-by-header-hash    [HASH] Look up a block by block header hash

  -c, --connections             List nodes connected to this Full Node

  -e, --exit-node               Shut down the running Full Node

  -r, --remove-connection       [NODE ID] Remove a Node by the full or first 8 characters of NodeID

  -s, --state                   Show the current state of the blockchain

  --endpoint-uri                (Group: endpoint) [URI] The uri of the rpc endpoint, including the proper port and wss/https scheme prefix

  --cert-path                   (Group: endpoint) [PATH] The full path to the .crt file to use for authentication

  --key-path                    (Group: endpoint) [PATH] The full path to the .key file to use for authentication

  --chia-config-path            (Group: endpoint) [PATH] The full path to a chia config yaml file for endpoints

  --default-chia-config     (Group: endpoint) Flag indicating to use the default chia config for endpoints

  --default-endpoint        (Group: endpoint) Flag indicating to use the default saved endpoint

  --saved-endpoint              (Group: endpoint) [ID] Use a saved endpoint

  -v, --verbose                 Set output to verbose messages.

  --help                        Display this help screen.

  --version                     Display version information.
```
___

_chia and its logo are the registered trademark or trademark of Chia Network, Inc. in the United States and worldwide._
