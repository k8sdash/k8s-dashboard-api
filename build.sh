# /bin/sh

cd  /mnt/c/github.com/k8sdash/K8SDashboard

docker build -f Api.Dockerfile /mnt/c/github.com/k8sdash/K8SDashboard  -t k8s-dashboard-api:0.3.3
docker image tag k8s-dashboard-api:0.3.3 localhost:5000/k8s-dashboard-api:0.3.3
docker push localhost:5000/k8s-dashboard-api:0.3.3

kubectl apply -f deploy-k8s-dashboard.yaml



docker build -f Client.Dockerfile /mnt/c/github.com/k8sdash/K8SDashboard  -t k8s-dashboard-client:0.0.7
docker image tag k8s-dashboard-client:0.0.7 localhost:5000/k8s-dashboard-client:0.0.7
docker push localhost:5000/k8s-dashboard-client:0.0.7
 
kubectl apply -f deploy-k8s-dashboard.yaml