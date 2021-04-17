# Durable Event Processing

Proof of concept of a globally available event processing system using Azure Front Door, Azure Kubernetes Service, Azure Event Hubs, Azure Cosmos DB, and Microsoft Orleans.

## Setup

Use the sections below to setup and configure the proof of concept. The full setup will likely take approximately one hour.

### Prerequisites
You will need the following tools to complete setup:

- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [Project Bicep tooling](https://github.com/Azure/bicep/blob/main/docs/installing.md)
- [kubectl](https://kubernetes.io/docs/tasks/tools/)
- [Helm](https://helm.sh/docs/intro/install/)

Once the above tools are installed, complete each of these sections in order:

1. [Deploy Azure infrastructure](docs/1.iac.md)
1. [Add pod identity to the AKS clusters](docs/2.podidentity.md)
1. [Add the Azure Key Vault Secrets Store CSI driver to AKS](docs/3.akvsecretsstore.md)
1. [Build and push the Docker images for the OrleansPoc.Api.SiloHost and OrleansPoc.Processor.SiloHost projects](docs/4.buildimages.md)
1. [Install the Api.SiloHost and Processor.SiloHost charts](docs/5.installapps.md)
1. [Deploy Azure Front Door](docs/6.frontdoor.md)

## Usage

## License

The MIT License (MIT)

Copyright © 2020 Ryan Graham

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.