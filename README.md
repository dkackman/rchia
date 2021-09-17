# rchia

Remote management CLI for [chia nodes](https://github.com/Chia-Network/chia-blockchain).

[![.NET](https://github.com/dkackman/rchia/actions/workflows/dotnet.yml/badge.svg)](https://github.com/dkackman/rchia/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/dkackman/rchia/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/dkackman/rchia/actions/workflows/codeql-analysis.yml)

## Introduction

A cross platform (Linux, Windows, MacOS) command line utility that mirrors the `chia` CLI, but uses RPC rather than running locally on the node. This allows management of any number of nodes from a central location, as long as [their RPC interface is exposed on the network](https://github.com/dkackman/rchia/wiki/Exposing-a-Node-on-the-Network).

## Getting Started

- [Exposing a node on the network](https://github.com/dkackman/rchia/wiki/Exposing-a-Node-on-the-Network)
- [Setting up endpoints](https://github.com/dkackman/rchia/wiki/Managing-Saved-Enpoints)
- [Specifying an endpoint on the command line](https://github.com/dkackman/rchia/wiki/Specifiying-RPC-Endpoints)

## Build and Run

Install [the .net5 sdk](https://dotnet.microsoft.com/download)

```bash
dotnet build src
cd src/rchia/bin/Debug/net5.0/
./rchia --help
```

## Install

Download the appropriate installer from [the latest release](https://github.com/dkackman/rchia/releases).
There are three downloads types for each OS:
- `standalone` - the dotnet framework is bundled in the executable. Large file but no dependencies
- `singlefile` - the executable and its dependencies are bundled as a single file. Smaller file but requires the [dotnet runtime](https://dotnet.microsoft.com/download/dotnet/5.0)
- `files` - the executable and its dependencies are not bundled together. Smaller file but requires the [dotnet runtime](https://dotnet.microsoft.com/download/dotnet/5.0)

## Example

The details of the endpoint can be specified in the following ways:

### On the command line

```bash
./rchia show -s --endpoint-uri https://node1:8555 --cert-path ~/certs/node1/private_full_node.crt --key-path ~/certs/node1/private_full_node.key
```

### By using the `chia` config

```bash
./rchia show -s --default-chia-config
```

### Using saved endpoint connections

```bash
./rchia endpoints --add node1 https://node1:8555 ~/certs/node1/private_full_node.crt ~/certs/node1/private_full_node.key
./rchia show -s --endpoint node1
```

### Currently Suported Verbs

```bash
rchia

Usage:
  rchia [options] [command]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  endpoints              Manage saved endpoints.
  farm                   Manage your farm.
                         Requires a daemon endpoint.
  keys                   Manage your keys
                         Requires a wallet or daemon endpoint.
  netspace               Calculates the estimated space on the network given two block header hashes.
                         Requires a daemon or full_node endpoint.
  plotnft                Manage your plot NFTs.
                         Requires a daemon endpoint.
  plots                  Manage your plots.
                         Requires a daemon endpoint.
  show                   Shows various properties of a full node.
                         Requires a daemon or full_node endpoint.
  start <service-group>  Start service groups.
                         Requires a daemon endpoint.
  status                 Shows the status of the node.
                         Requires a daemon endpoint.
  stop <service-group>   Stop service groups.
                         Requires a daemon endpoint.
  wallet                 Manage your wallet.
                         Requires a wallet or daemon endpoint.
```

### Example output

```bash
user@computer:~$ ./rchia show -s --endpoint node1 -v
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
show
  Shows various properties of a full node.
  Requires a daemon or full_node endpoint.

Usage:
  rchia [options] show

Options:
  -a, --add-connection <URI>                   Connect to another Full Node by ip:port
  -b, --block-by-header-hash <HASH>            Look up a block by block header hash
  -bh, --block-header-hash-by-height <HEIGHT>  Look up a block header hash by block height
  -c, --connections                            List nodes connected to this Full Node
  -e, --exit-node                              Shut down the running Full Node
  -r, --remove-connection <NODE ID>            Remove a Node by the full or first 8 characters of NodeID
  -s, --state                                  Show the current state of the blockchain
  -uri, --endpoint-uri <PATH>                  The uri of the rpc endpoint, including the proper port and wss/https scheme prefix
  -cp, --cert-path <PATH>                      The full path to the .crt file to use for authentication
  -kp, --key-path <PATH>                       The full path to the .key file to use for authentication
  -ccp, --chia-config-path <PATH>              The full path to a chia config yaml file for endpoints
  -dcc, --default-chia-config                  Flag indicating to use the default chia config for endpoints
  -de, --default-endpoint                      Flag indicating to use the default saved endpoint
  -ep, --endpoint <ID>                         Use a saved endpoint
  -v, --verbose                                Set output to verbose messages
  -?, -h, --help                               Show help and usage information
```
___

_chia and its logo are the registered trademark or trademark of Chia Network, Inc. in the United States and worldwide._
