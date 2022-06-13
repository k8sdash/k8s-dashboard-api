import React, { Component } from 'react';
import * as signalR from "@microsoft/signalr";
import 'ag-grid-community/dist/styles/ag-grid.css';
import 'ag-grid-community/dist/styles/ag-theme-balham.css';
import { GridLightRoutes } from './ag-grid-light-routes';

export default class App extends Component {
    static displayName = App.name;

    constructor(props) {
       super(props);
        this.state = {
            lightroutes: [],
            loading: true
        };

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('http://localhost:5183/hubs/lightroutes')
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Trace)
            .build();
          

        this.connection.on("propagate", (pod, eventType) => { this.handlePodEvent(pod, eventType) });

        this.connection.start().catch(err => console.error(`error connecting to signalR hub ${err}`));
    }

    handlePodEvent(pod, eventType) {
        console.log(`SignalR Pod ${pod} Event ${eventType}`);
        this.setState({ loading: true });
        this.populateClusterData();
    }

    componentDidMount() {
        console.log("component did mount!");
        this.populateClusterData();
    }

    onGridReady(params) {
        console.log("grid is ready");
    }

    onFirstDataRendered(params) {
        console.log("first data rendered");
        params.columnApi.autoSizeAllColumns();
    }

    render() {
        return (
            <div>
                <h1 id="tabelLabel" >K8S Dashboard</h1>
                <p>This the list of nodes, pods and ingress routes within the Kubernetes cluster</p>
                <GridLightRoutes onFirstDataRendered={this.onFirstDataRendered} onGridReady={this.onGridReady} lightRoutes={this.state.lightroutes}></GridLightRoutes>
            </div>
        );
    }

    async populateClusterData() {
        const response = await fetch('http://localhost:5183/k8scluster/lightroutes');
        const data = await response.json();
        this.setState({ lightroutes: data, loading: false });
    }
}