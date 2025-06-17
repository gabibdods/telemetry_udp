const WebSocket = require('ws');
const mysql = require('mysql2/promise');

const wss = new WebSocket.Server({ port: 3000 });

let lastId = 0;
let lastHeartbeat = Date.now();
let pollingActive = true;

wss.on('connection', (ws) => {
    //console.log('Client connected');

    ws.on('message', (message) => {
        if (message.toString() === '__ping__') {
            lastHeartbeat = Date.now();
            if (!pollingActive) {
                pollingActive = true;
                //console.log("Resumed DB polling");
            }
        }
    });
});

function pausePollingIfStale() {
    const now = Date.now();
    if (now - lastHeartbeat > 1000 && pollingActive) {
        pollingActive = false;
        //console.log("Pausing DB polling: no new telemetry.");
    }
}

setInterval(pausePollingIfStale, 500);

(async function pollDatabase() {
    const conn = await mysql.createConnection({
        host: 'db',
        user: 'user',
        password: 'pass',
        database: 'telemetry'
    });

    setInterval(async () => {
        if (!pollingActive) return; // Skip DB query if no heartbeat

        try {
            const [rows] = await conn.query(
                "SELECT * FROM telemetry ORDER BY timestamp DESC LIMIT 1"
            );
            const latest = rows[0];

            if (latest && latest.id > lastId) {
                lastId = latest.id;
                const message = JSON.stringify(latest);
                //console.log("Broadcasting:", message); debug

                wss.clients.forEach(client => {
                    if (client.readyState === WebSocket.OPEN) {
                        client.send(message);
                    }
                });
            }
        } catch (err) {
            console.error('Polling error:', err);
        }
    }, 1);
})();