# /bin/sh

cd  /mnt/c/github.com/k8sdash/K8SDashboard

docker build -f Api.Dockerfile /mnt/c/github.com/k8sdash/K8SDashboard  -t k8s-dashboard-api:0.2.8
docker image tag k8s-dashboard-api:0.2.8 localhost:5000/k8s-dashboard-api:0.2.8
docker push localhost:5000/k8s-dashboard-api:0.2.8

docker build -f Client.Dockerfile /mnt/c/github.com/k8sdash/K8SDashboard  -t k8s-dashboard-client:0.0.5
docker image tag k8s-dashboard-client:0.0.5 localhost:5000/k8s-dashboard-client:0.0.5
docker push localhost:5000/k8s-dashboard-client:0.0.5

kubectl apply -f k8s-dashboard.yaml