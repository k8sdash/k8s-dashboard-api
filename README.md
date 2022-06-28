[![.github/workflows/dotnet.yml](https://github.com/k8sdash/k8s-dashboard-api/actions/workflows/dotnet.yml/badge.svg)](https://github.com/k8sdash/k8s-dashboard-api/actions/workflows/dotnet.yml) [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=k8sdash_k8s-dashboard-api&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=k8sdash_k8s-dashboard-api) [![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=k8sdash_k8s-dashboard-api&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=k8sdash_k8s-dashboard-api) [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=k8sdash_k8s-dashboard-api&metric=bugs)](https://sonarcloud.io/summary/new_code?id=k8sdash_k8s-dashboard-api)

# k8s-dashboard-api
A simple .net 6.0 api to monitor Kubernetes, exposing ingress routes, pods and nodes

## TL;DR
```
kubectl apply -f https://raw.githubusercontent.com/k8sdash/k8s-dashboard-charts/main/deploy-k8s-dashboard.yaml
```

### Before you begin
#### Prerequisites
* Kubernetes 1.19+
* Preferably an nginx ingress 
