import React, { Component } from 'react';
import { HubConnection} from '@microsoft/signalr';
import { AgGridColumn, AgGridReact } from 'ag-grid-react';
import 'ag-grid-community/dist/styles/ag-grid.css';
import 'ag-grid-community/dist/styles/ag-theme-balham.css';

export default class App extends Component {
    static displayName = App.name;

    constructor(props) {
        super(props);
        this.state = {
            lightroutes: [],
            loading: true, 
            bookingHubConnection: null
        };
    }

    componentDidMount() {
        const bookingHubConnection = new HubConnection('/hubs/lightroutes');

        this.setState({ bookingHubConnection }, () => {
            this.state.bookingHubConnection.start()
                .then(() => console.log('SignalR Started'))
                .catch(err => console.log('Error connecting SignalR - ' + err));

            this.state.bookingHubConnection.on('booking', (message) => {
                const bookingMessage = message;
                this.setState({ bookingMessage });
            });
        });
        this.populateClusterData();
    }

    renderGrid() {
        return (
            <div>
                <div
                    className="ag-theme-balham"
                    style={{ height: '100%', width: '100%' }}
                >
                    <AgGridReact
                        defaultColDef={{
                            filter: true,
                            filterParams: {
                                buttons: ['apply', 'reset', 'clear'],
                                excelMode: 'windows',
                                closeOnApply: true,
                                includeBlanksInEquals: true
                            },
                            resizable: true,
                            sortable: true,
                            autoHeight: true,
                            wrapText: true, 
                            floatingFilter: true
                        }}
                        pagination={true}
                        paginationPageSize={9999}
                        domLayout="autoHeight"
                        skipHeaderOnAutoSize={true}
                        onGridReady={this.onGridReady}
                        onFirstDataRendered={this.onFirstDataRendered} 

                        rowData={this.state.lightroutes}>
                        <AgGridColumn field="node"></AgGridColumn>
                        <AgGridColumn field="nodeIp"></AgGridColumn>
                        <AgGridColumn field="nameSpace"></AgGridColumn>
                        <AgGridColumn field="pod"></AgGridColumn>
                        <AgGridColumn field="podPhase"></AgGridColumn>
                        <AgGridColumn field="podIp"></AgGridColumn>
                        <AgGridColumn field="podPort"></AgGridColumn>
                        <AgGridColumn field="service"></AgGridColumn>
                        <AgGridColumn field="ingress"></AgGridColumn>
                        <AgGridColumn field="image"></AgGridColumn>
                    </AgGridReact>
                </div>
            </div>
        );
    }

    cellClassRulesPodPhase = {
        'pending': (params) => { return params?.data?.podPhase === "Pending" },
        'failed': (params) => { return params?.data?.podPhase === "Failed" },
        'running': (params) => { return params?.data?.podPhase === "Running" }
    }


    onGridReady(params) {
        console.log("grid is ready");
    }

    onFirstDataRendered (params){
        console.log("first data rendered");
        params.columnApi.autoSizeAllColumns();
}

    render() { 
        return (
            <div>
                <h1 id="tabelLabel" >K8S Dashboard</h1>
                <p>This the list of nodes, pods and ingress routes within the Kubernetes cluster</p>
                {this.renderGrid()}
            </div>
        );
    }

    async populateClusterData() {
        const response = await fetch('/k8scluster/lightroutes');
        const data = await response.json();
        this.setState({ lightroutes: data, loading: false });
    }
}
