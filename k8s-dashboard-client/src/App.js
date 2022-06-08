import React, { Component } from 'react';
import { AgGridColumn, AgGridReact } from 'ag-grid-react';
import 'ag-grid-community/dist/styles/ag-grid.css';
import 'ag-grid-community/dist/styles/ag-theme-balham.css';

export default class App extends Component {
    static displayName = App.name;

    constructor(props) {
        super(props);
        this.state = {
            rowData: [],
            lightroutes: [],
            loading: true
        };
    }

    componentDidMount() {
        fetch('https://www.ag-grid.com/example-assets/row-data.json')
        .then(result => result.json())
        .then(rowData => this.setState({ rowData }))
        this.populateClusterData();
    }

    renderForecastsTable(lightroutes) {
        return (
            <div >

                <div
                    className="ag-theme-balham"
                    style={{ height: '400px' , width: '980px' }}
                >
                    <AgGridReact
                        rowData={this.state.lightroutes}>
                        <AgGridColumn field="name"></AgGridColumn>
                        <AgGridColumn field="nameSpace"></AgGridColumn>
                        <AgGridColumn field="node"></AgGridColumn>
                        <AgGridColumn field="nodeIp"></AgGridColumn>
                        <AgGridColumn field="podPort"></AgGridColumn>
                        <AgGridColumn field="podIp"></AgGridColumn>
                        <AgGridColumn field="podPhase"></AgGridColumn>
                        <AgGridColumn field="image"></AgGridColumn>
                    </AgGridReact>
                </div>
                </div>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
            : this.renderForecastsTable(this.state.lightroutes);

        return (
            <div>
                <h1 id="tabelLabel" >K8S Dashboard</h1>
                <p>This component demonstrates fetching data from the server.</p>
                {contents}
            </div>
        );
    }

    async populateClusterData() {
        const response = await fetch('/k8scluster/lightroutes');
        const data = await response.json();
        this.setState({ lightroutes: data, loading: false });
    }
}
