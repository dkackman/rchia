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

## Usage for the Show Command
```bash
user@computer:~./rchia show --help
rchia 1.0.0
Copyright (C) 2021 rchia

  -s, --state                Show the current state of the blockchain

  -e, --exit-node            Shut down the running Full Node

  -c, --connections          List nodes connected to this Full Node

  -a, --add-connection       Connect to another Full Node by ip:port

  -r, --remove-connection    Remove a Node by the first 8 characters of NodeID

  -v, --verbose              Set output to verbose messages.

  --endpoint-uri             The uri of the rpc endpoint, including the proper port and wss/https scheme prefix

  --cert-path                The full path to the .crt file to use for authentication

  --key-path                 The full path to the .key file to use for authentication

  --config-path              The full path to a chia config yaml file for endpoints

  --use-default-config       Flag indicating to use the default chia config for endpoints

  --help                     Display this help screen.

  --version                  Display version information.
```
___

_chia and its logo are the registered trademark or trademark of Chia Network, Inc. in the United States and worldwide._
