const { createProxyMiddleware } = require('http-proxy-middleware');

const contextHttp = [
    "api"
];

module.exports = function (app) {

    const appProxyHttp = createProxyMiddleware(contextHttp, {
        target: 'http://localhost:9999',
        secure: false
    });

    //app.use(appProxyHttp);
};
