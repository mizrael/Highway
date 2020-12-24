# Highway
[![Highway](https://circleci.com/gh/mizrael/Highway.svg?style=shield&circle-token=b7635df8feb7c79524db993c3cf962863ad28aa1)](https://app.circleci.com/pipelines/github/mizrael/Highway)

## Description
Highway is a distributed saga management library, written in C# with .NET Core 5. 
It is intended to be reliable, fast, easy to use, configurable and extensible.

### Message Transport
Only in-memory transport is available for now.

### State Persistence

As of now, saga state persistence is available on MongoDB only.

## Samples
A .NET Console app is available in the `/samples/` folder. Before running it, make sure to spin-up the required infrastructure using the provided docker-compose configuration.

## Roadmap
- add RabbitMQ message transport
- add Azure ServiceBus message transport
- add CosmosDB saga state persistence