const { createProxyMiddleware } = require('http-proxy-middleware');

const context = [
    "/k8scluster/lightroutes",
];

module.exports = function (app) {
    const appProxy = createProxyMiddleware(context, {
        target: 'http://localhost:5183',
        secure: false
    });

    app.use(appProxy);
};
