const cluster = require('cluster');
const os = require('os');
const WebSocket = require('ws');
const mysql = require('mysql2/promise');

const numCPUs = os.cpus().length;
const DB_CONFIG = {
    host: 'db',
    user: 'user',
    password: 'pass',
    database: 'telemetry',
    waitForConnections: true,
    connectionLimit: 10,
    queueLimit: 0
};
if (cluster.isMaster) {
    console.log(`Master ${process.pid} is running. Forking ${numCPUs} workers...`);

    for (let i = 0; i < numCPUs; i++) {
        cluster.fork();
    }
    cluster.on('exit', (worker, code, signal) => {
        console.log(`Worker ${worker.process.pid} died. Restarting...`);
        cluster.fork();
    });
} else {
    (async () => {
        const pool = mysql.createPool(DB_CONFIG);
        const wss = new WebSocket.Server({ port: 3000 });

        let lastId = 0;
        let lastHeartbeat = Date.now();
        let pollingActive = true;

        wss.on('connection', (ws) => {
            ws.on('message', (message) => {
                if (message.toString() === '__ping__') {
                    lastHeartbeat = Date.now();
                    if (!pollingActive) pollingActive = true;
                }
            });
        });
        function pausePollingIfStale() {
            const now = Date.now();
            if (now - lastHeartbeat > 1000) pollingActive = false;
        }
        setInterval(pausePollingIfStale, 500);

        setInterval(async () => {
            if (!pollingActive) return;

            try {
                const [rowsCarTelemetryData] = await pool.query(
                    "SELECT * FROM telemetryCarTelemetryData ORDER BY timestamp DESC LIMIT 1"
                );
                const latestCarTelemetryData = rowsCarTelemetryData[0];
                if (latestCarTelemetryData && latestCarTelemetryData.id > lastId) {
                    lastId = latestCarTelemetryData.id;
                    const message = JSON.stringify(latestCarTelemetryData);

                    wss.clients.forEach(client => {
                        if (client.readyState === WebSocket.OPEN) {
                            client.send(message);
                        }
                    });
                }

                const [rowsSessionData] = await pool.query(
                    "SELECT * FROM telemetryCarTelemetryData ORDER BY timestamp DESC LIMIT 1"
                );
                const latestSessionData = rowsSessionData[0];
                if (latestSessionData && latestSessionData.id > lastId) {
                    lastId = latestSessionData.id;
                    const message = JSON.stringify(latestSessionData);

                    wss.clients.forEach(client => {
                        if (client.readyState === WebSocket.OPEN) {
                            client.send(message);
                        }
                    });
                }
            } catch (err) {
                console.error(`Worker ${process.pid} polling error:`, err);
            }
        }, 1);
    })();
}