[![Docker](https://github.com/k8sdash/k8s-dashboard-api/actions/workflows/docker-publish.yml/badge.svg)](https://github.com/k8sdash/k8s-dashboard-api/actions/workflows/docker-publish.yml) [![.github/workflows/dotnet.yml](https://github.com/k8sdash/k8s-dashboard-api/actions/workflows/dotnet.yml/badge.svg)](https://github.com/k8sdash/k8s-dashboard-api/actions/workflows/dotnet.yml) 

[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=k8sdash_k8s-dashboard-api&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=k8sdash_k8s-dashboard-api) [![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=k8sdash_k8s-dashboard-api&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=k8sdash_k8s-dashboard-api) [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=k8sdash_k8s-dashboard-api&metric=bugs)](https://sonarcloud.io/summary/new_code?id=k8sdash_k8s-dashboard-api)

# k8s-dashboard-api
A simple .net 6.0 api to monitor Kubernetes, exposing ingress routes, pods and nodes

## TL;DR
```
kubectl apply -f https://raw.githubusercontent.com/k8sdash/k8s-dashboard-charts/main/deploy-k8sdash-kubernetes.yaml
```

## Installation
`k8s-dashboard-api` is available as a [docker image](https://hub.docker.com/repository/docker/k8sdash/k8s-dashboard-api).

### Before you begin
#### Prerequisites
* Kubernetes 1.19+

### Recommendations
`k8s-dashboard-api` and `k8s-dashboard-client` are designed to work together, even though other clients could consume `k8s-dashboard-api`. Regardless, it is recommended to install them using this [repo](https://github.com/k8sdash/k8s-dashboard-api/). 

## Technologies used
### Code base
* dotnet 6.0
* swagger OpenAPI
* signalR
* official C# kubernetes client

### CICD
* GitHub Actions
* SonarCloud.io
* Docker
* Docker Hub

## Contributing
GitHub pull requests are welcome!

