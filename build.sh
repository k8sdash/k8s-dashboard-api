# /bin/sh

docker build -f Api.Dockerfile /mnt/c/github.com/k8sdash/K8SDashboard  -t k8s-dashboard-api:0.0.1
docker build -f Client.Dockerfile /mnt/c/github.com/k8sdash/K8SDashboard  -t k8s-dashboard-client:0.0.1
