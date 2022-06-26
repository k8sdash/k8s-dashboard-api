# /bin/sh

cd  /mnt/c/github.com/k8sdash/k8s-dashboard-api

docker build .  -t k8s-dashboard-api:0.4.0
docker image tag k8s-dashboard-api:0.4.0 localhost:5000/k8s-dashboard-api:0.4.0
docker push localhost:5000/k8s-dashboard-api:0.4.0

kubectl apply -f deploy-k8s-dashboard.yaml


cd  /mnt/c/github.com/k8sdash/k8s-dashboard-client

docker build . -t k8s-dashboard-client:0.3.6
docker image tag k8s-dashboard-client:0.3.6 localhost:5000/k8s-dashboard-client:0.3.6
docker push localhost:5000/k8s-dashboard-client:0.3.6

cd .. 

kubectl apply -f deploy-k8s-dashboard.yaml