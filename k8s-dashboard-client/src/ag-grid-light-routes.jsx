import React from 'react';
import { AgGridColumn, AgGridReact } from 'ag-grid-react';
import 'ag-grid-community/dist/styles/ag-grid.css';
import 'ag-grid-community/dist/styles/ag-theme-balham.css';

export const GridLightRoutes = ({
    onGridReady, onFirstDataRendered, lightRoutes
}) => {
    let cellClassRulesPodPhase = {
        'pending': (params) => { return params?.data?.podPhase === "Pending" },
        'failed': (params) => { return params?.data?.podPhase === "Failed" },
        'running': (params) => { return params?.data?.podPhase === "Running" },
        'completed': (params) => { return params?.data?.podPhase === "Completed" }
    }

    const getRowId = params => params.data.id;
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
                        autoHeight: false,
                        wrapText: true,
                        floatingFilter: true
                    }}
                    pagination={true}
                    paginationPageSize={9999}
                    domLayout="autoHeight"
                    skipHeaderOnAutoSize={false}
                    onGridReady={onGridReady}
                    onFirstDataRendered={onFirstDataRendered}
                    rowData={lightRoutes}
                    getRowId={getRowId}
                >
                    <AgGridColumn field="node"></AgGridColumn>
                    <AgGridColumn field="nodeIp"></AgGridColumn>
                    <AgGridColumn field="nameSpace"></AgGridColumn>
                    <AgGridColumn field="pod"></AgGridColumn>
                    <AgGridColumn field="podPhase" cellClassRules={cellClassRulesPodPhase}></AgGridColumn>
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