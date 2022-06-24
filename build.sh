# /bin/sh

cd  /mnt/c/github.com/k8sdash/K8SDashboard

docker build -f Api.Dockerfile /mnt/c/github.com/k8sdash/K8SDashboard  -t k8s-dashboard-api:0.3.7
docker image tag k8s-dashboard-api:0.3.7 localhost:5000/k8s-dashboard-api:0.3.7
docker push localhost:5000/k8s-dashboard-api:0.3.7

kubectl apply -f deploy-k8s-dashboard.yaml



cd  /mnt/c/github.com/k8sdash/K8SDashboard

docker build -f Client.Dockerfile /mnt/c/github.com/k8sdash/K8SDashboard  -t k8s-dashboard-client:0.2.6
docker image tag k8s-dashboard-client:0.2.6 localhost:5000/k8s-dashboard-client:0.2.6
docker push localhost:5000/k8s-dashboard-client:0.2.6
 
kubectl apply -f deploy-k8s-dashboard.yaml