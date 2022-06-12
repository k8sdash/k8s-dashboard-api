const { createProxyMiddleware } = require('http-proxy-middleware');

const contextHttp = [
    "/k8scluster/lightroutes"
];

module.exports = function (app) {

    const appProxyHttp = createProxyMiddleware(contextHttp, {
        target: 'http://localhost:5183',
        secure: false
    });

    app.use(appProxyHttp);
};
