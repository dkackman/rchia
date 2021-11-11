# rchia

Remote management CLI for [chia nodes](https://github.com/Chia-Network/chia-blockchain).

[![.NET](https://github.com/dkackman/rchia/actions/workflows/dotnet.yml/badge.svg)](https://github.com/dkackman/rchia/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/dkackman/rchia/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/dkackman/rchia/actions/workflows/codeql-analysis.yml)

## Introduction

A cross platform (Linux ARM & x64, Windows, MacOS) command line utility that mirrors the `chia` CLI, but uses RPC rather than running locally on the node. This allows management of any number of nodes from a central location, as long as [their RPC interface is exposed on the network](https://github.com/dkackman/rchia/wiki/Exposing-a-Node-on-the-Network).

## Getting Started

- [Exposing a node on the network](https://github.com/dkackman/rchia/wiki/Exposing-a-Node-on-the-Network)
- [Specifying an endpoint on the command line](https://github.com/dkackman/rchia/wiki/Specifiying-RPC-Endpoints)
- [Saving up endpoint definitions](https://github.com/dkackman/rchia/wiki/Managing-Saved-Enpoints)

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

Incorporates Ansi console features when available.
![image](https://user-images.githubusercontent.com/5160233/134552277-59128c00-64e0-474d-88ac-50b092993a68.png)

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
  bech32       Convert addresses to and from puzzle hashes.
  blocks       Show informations about blocks and coins.
               Requires a daemon or full_node endpoint.
  connections  Various methods for managing node connections.
               Requires a daemon or full_node endpoint.
  endpoints    Manage saved endpoints.
  farm         Manage your farm.
               Requires a daemon endpoint.
  keys         Manage your keys
               Requires a wallet or daemon endpoint.
  netspace     Calculates the estimated space on the network given two block header hashes.
               Requires a daemon or full_node endpoint.
  node         Commands to managing the node.
               Requires a daemon endpoint.
  plotnft      Manage your plot NFTs.
               Requires a wallet or daemon endpoint.
  plots        Manage your plots.
               Requires a harvester, plotter or daemon endpoint.
  services     Shows the status of the node.
               Requires a daemon endpoint.
  wallets      Manage your wallet.
               Requires a wallet or daemon endpoint.
```

### Example output

![image](https://user-images.githubusercontent.com/5160233/134552904-50ea4822-d53a-4144-85be-86c9bcbd1625.png)

## Usage for the Show Command
```bash
user@computer:~$ ./rchia show --help
show
  Shows various properties of a full node.
  Requires a daemon or full_node endpoint.

Usage:
  rchia [options] show [command]

Options:
  -?, -h, --help  Show help and usage information

Commands:
  add <host>   Connect to another Full Node by ip:port
  exit         Shut down the running Full Node
  header       Look up a block header hash by block height or hash
  state        Show the current state of the blockchain
  connections  List nodes connected to this Full Node
  prune        Prune stale connections
  remove <ID>  Remove a Node by the full or first 8 characters of NodeID
```
___

_chia and its logo are the registered trademark or trademark of Chia Network, Inc. in the United States and worldwide._
