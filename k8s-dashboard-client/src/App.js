import React, { Component } from 'react';
import AppBar from '@mui/material/AppBar';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import CardActions from '@mui/material/CardActions';
import CardContent from '@mui/material/CardContent';
import CardHeader from '@mui/material/CardHeader';
import CssBaseline from '@mui/material/CssBaseline';
import Grid from '@mui/material/Grid';
import StarIcon from '@mui/icons-material/StarBorder';
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';
import Link from '@mui/material/Link';
import GlobalStyles from '@mui/material/GlobalStyles';
import Container from '@mui/material/Container';
import * as signalR from "@microsoft/signalr";
import 'ag-grid-community/dist/styles/ag-grid.css';
import 'ag-grid-community/dist/styles/ag-theme-balham.css';
import GitHubIcon from '@mui/icons-material/GitHub';
import Home from '@mui/icons-material/Home';
import { GridLightRoutes } from './ag-grid-light-routes';

const technologyLinks = [
    {
        title: 'Server Side',
        subheader: 'Technologies',
        description: [
            '.Net 6.0 OpenAPI',
            'SignalR',
        ],
    },
    {
        title: 'Client Side',
        subheader: 'Technologies',
        description: [
            'React',
            'Material UI',
        ],
    },
    {
        title: 'Container',
        subheader: 'Technologies',
        description: [
            'Kubernetes',
            'Docker',
        ],
    },
];

const footerLinks = [
    {
        title: 'About',
        description: [{
            title: 'Team', href: 'https://github.com/ebCrypto'
        }, {
            title: 'History', href: ''
        }, {
            title: 'Contact us', href: 'mailto:contact@k8sdashboard.com'
        }],
    },
    {
        title: 'Features',
        description: [
            {
                title: 'C# kubernetes client', href: 'https://github.com/kubernetes-client/csharp'
            },
            {
                title: '.Net 6.0', href: 'https://dotnet.microsoft.com/en-us/'
            },
            {
                title: 'SignalR', href: 'https://docs.microsoft.com/en-us/aspnet/signalr/overview/getting-started/introduction-to-signalr'
            }, {
                title: 'Swagger OpenAPI', href: 'https://swagger.io/resources/open-api/'
            }, {
                title: 'React', href: 'https://reactjs.org/'
            }, {
                title: 'MaterialUI', href: 'https://mui.com/material-ui/'
            }
        ],
    },
    {
        title: 'GitHub',
        description: [{
            title: 'Documentation', href: 'https://github.com/k8sdash/k8s-dashboard#readme'
        }, {
            title: 'Issues', href: 'https://github.com/k8sdash/k8s-dashboard/issues'
        }, {
            title: 'PRs Welcome', href: 'https://github.com/k8sdash/k8s-dashboard/pulls'
        }],
    },
    {
        title: 'Legal',
        description: [{ title: 'Licence', href: 'https://raw.githubusercontent.com/k8sdash/k8s-dashboard/main/LICENSE' }],
    },
];
export default class App extends Component {
    static displayName = App.name;

    constructor(props) {
        super(props);
        this.state = {
            lightroutes: [],
            loading: true
        };

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(this.getBackendUrl() + '/hubs/lightroutes')
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
            <React.Fragment>
                <GlobalStyles styles={{ ul: { margin: 0, padding: 0, listStyle: 'none' } }} />
                <CssBaseline />
                <AppBar
                    position="static"
                    color="default"
                    elevation={0}
                    sx={{ borderBottom: (theme) => `1px solid ${theme.palette.divider}` }}
                >
                    <Toolbar sx={{ flexWrap: 'wrap' }}>
                        <Typography variant="h6" color="inherit" noWrap sx={{ flexGrow: 1 }}>
                            K8S Dashboard
                        </Typography>
                        <nav>
                            <Link
                                variant="button"
                                color="text.primary"
                                href="#"
                                sx={{ my: 1, mx: 1.5 }}
                            >
                                <Home /> Home
                            </Link>
                            <Link
                                variant="button"
                                color="text.primary"
                                href="https://github.com/k8sdash/k8s-dashboard"
                                sx={{ my: 1, mx: 1.5 }}
                            >
                                <GitHubIcon /> GitHub
                            </Link>
                            <Link
                                variant="button"
                                color="text.primary"
                                href={this.getBackendUrl() + "/api/"}
                                sx={{ my: 1, mx: 1.5 }}
                            >
                                Swagger
                            </Link>
                        </nav>
                    </Toolbar>
                </AppBar>
                {/* Hero unit */}
                <Container disableGutters maxWidth="sm" component="main" sx={{ pt: 8, pb: 6 }}>
                    <Typography
                        component="h1"
                        variant="h2"
                        align="center"
                        color="text.primary"
                        gutterBottom
                    >
                        K8S Dashboard
                    </Typography>
                    <Typography variant="h5" align="center" color="text.secondary" component="p">
                        Quickly monitor in real time the nodes, pods, services and ingress routes within your Kubernetes Cluster.
                    </Typography>
                </Container>
                {/* End hero unit */}
                {/* Hero unit */}
                <Container maxWidth="m">
                    <GridLightRoutes onFirstDataRendered={this.onFirstDataRendered} onGridReady={this.onGridReady} lightRoutes={this.state.lightroutes}></GridLightRoutes>
                </Container>
                {/* End hero unit */}
                {/* Hero unit */}
                <Container disableGutters maxWidth="sm" component="main" sx={{ pt: 8, pb: 6 }}>
                    <Typography
                        component="h2"
                        variant="h3"
                        align="center"
                        color="text.primary"
                        gutterBottom
                    >
                        Behind the scene
                    </Typography>
                    <Typography variant="h5" align="center" color="text.secondary" component="p">
                        Technologies in use:
                    </Typography>
                </Container>
                {/* End hero unit */}
                <Container maxWidth="md" component="main">
                    <Grid container spacing={5} alignItems="flex-end">
                        {
                            technologyLinks.map((tier) => (
                                <Grid
                                    item
                                    key={tier.title}
                                    xs={12}
                                    sm={tier.title === 'Client Side' ? 12 : 6}
                                    md={4}
                                >
                                    <Card>
                                        <CardHeader
                                            title={tier.title}
                                            subheader={tier.subheader}
                                            titleTypographyProps={{ align: 'center' }}
                                            action={tier.title === 'Client Side' ? <StarIcon /> : null}
                                            subheaderTypographyProps={{
                                                align: 'center',
                                            }}
                                            sx={{
                                                backgroundColor: (theme) =>
                                                    theme.palette.mode === 'light'
                                                        ? theme.palette.grey[200]
                                                        : theme.palette.grey[700],
                                            }}
                                        />
                                        <CardContent>
                                            <Box
                                                sx={{
                                                    display: 'flex',
                                                    justifyContent: 'center',
                                                    alignItems: 'baseline',
                                                    mb: 2,
                                                }}
                                            >
                                            </Box>
                                            <ul>
                                                {tier.description.map((line) => (
                                                    <Typography
                                                        component="li"
                                                        variant="subtitle1"
                                                        align="center"
                                                        key={line}
                                                    >
                                                        {line}
                                                    </Typography>
                                                ))}
                                            </ul>
                                        </CardContent>
                                        <CardActions>
                                            <Button fullWidth variant={tier.buttonVariant}>
                                                {tier.buttonText}
                                            </Button>
                                        </CardActions>
                                    </Card>
                                </Grid>
                            ))}
                    </Grid>
                </Container>
                {/* Footer */}
                <Container
                    maxWidth="md"
                    component="footer"
                    sx={{
                        borderTop: (theme) => `1px solid ${theme.palette.divider}`,
                        mt: 8,
                        py: [3, 6],
                    }}
                >
                    <Grid container spacing={4} justifyContent="space-evenly">
                        {footerLinks.map((footer) => (
                            <Grid item xs={6} sm={3} key={footer.title}>
                                <Typography variant="h6" color="text.primary" gutterBottom>
                                    {footer.title}
                                </Typography>
                                <ul>
                                    {footer.description.map((item) => (
                                        <li key={item.title}>
                                            <Link href={item.href} variant="subtitle1" color="text.secondary">
                                                {item.title}
                                            </Link>
                                        </li>
                                    ))}
                                </ul>
                            </Grid>
                        ))}
                    </Grid>
                    <Typography variant="body2" color="text.secondary" align="center" sx={{ mt: 5 }}>
                        {'Copyright � '}
                        <Link color="inherit" href="https://github.com/k8sdash/">
                            k8sdash
                        </Link>{' '}
                        {new Date().getFullYear()}
                        {'.'}
                    </Typography>
                </Container>
                {/* End footer */}
            </React.Fragment>
        );
    }

    async populateClusterData() {
        const response = await fetch(this.getBackendUrl() + '/api/v1/k8scluster/lightroutes');
        const data = await response.json();
        this.setState({ lightroutes: data, loading: false });
    }


    getBackendUrl() {
        if (process.env.REACT_APP_LOCAL_BACKEND_HOST && typeof process.env.REACT_APP_LOCAL_BACKEND_HOST !== "undefined" && process.env.REACT_APP_LOCAL_BACKEND_HOST !== "undefined") {
            console.log('using backend:' + process.env.REACT_APP_LOCAL_BACKEND_HOST);
            return process.env.REACT_APP_LOCAL_BACKEND_HOST;
        }
        else {
            return '';
        }
    }
}