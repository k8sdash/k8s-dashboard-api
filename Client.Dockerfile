FROM mhart/alpine-node:14 AS builder
WORKDIR /app
COPY k8s-dashboard-client .
#RUN npm install react-scripts -g --silent
RUN yarn install
RUN yarn run build

FROM mhart/alpine-node:14
RUN yarn global add serve
WORKDIR /app
COPY --from=builder /app/build .
CMD ["serve", "-p", "80", "-s", "."]
