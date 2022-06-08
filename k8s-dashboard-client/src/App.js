import React, { Component } from 'react';

export default class App extends Component {
    static displayName = App.name;

    constructor(props) {
        super(props);
        this.state = { lightroutes: [], loading: true };
    }

    componentDidMount() {
        this.populateWeatherData();
    }

    static renderForecastsTable(lightroutes) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Namespace</th>
                        <th>Node</th>
                        <th>Node Ip</th>
                        <th>Pod Port</th>
                        <th>Pod Ip</th>
                        <th>Pod Phase</th>
                        <th>Image</th>
                    </tr>
                </thead>
                <tbody>
                    {lightroutes.map(lightRoute =>
                        <tr key={lightRoute.name}>
                            <td>{lightRoute.nameSpace}</td>
                            <td>{lightRoute.node}</td>
                            <td>{lightRoute.nodeIp}</td>
                            <td>{lightRoute.podPort}</td>
                            <td>{lightRoute.podIp}</td>
                            <td>{lightRoute.podPhase}</td>
                            <td>{lightRoute.image}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
            : App.renderForecastsTable(this.state.lightroutes);

        return (
            <div>
                <h1 id="tabelLabel" >K8S Dashboard</h1>
                <p>This component demonstrates fetching data from the server.</p>
                {contents}
            </div>
        );
    }

    async populateWeatherData() {
        const response = await fetch('/k8scluster/lightroutes');
        const data = await response.json();
        this.setState({ lightroutes: data, loading: false });
    }
}
