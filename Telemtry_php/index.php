<html lang="en">
    <head>
        <title>Telemetry Dashboard</title>
        <style>
            @font-face {
                font-family: 'Digital';
                src: url('Formula1-Wide.woff') format('woff');
                font-weight: normal;
                font-style: normal;
            }
            body {
                background: #222;
                color: #fff;
                font-family: 'Digital', sans-serif;
                text-align: center;
            }
            canvas {
                background: transparent;
                border-radius: 50%;
                font-family: 'Digital', sans-serif;
                margin-top: 20px;
            }
            #header {
                margin-top: 10px;
            }
            #drs {
                height: 23px;
                width: 100px;
                margin: -70px auto;
                font-size: 20px;
                font-weight: bold;
                padding: 5px 20px;
                border-radius: 5px;
                font-family: 'Digital', sans-serif;
            }
            #gear {
                height: 23px;
                width: 100px;
                margin: 70px auto;
                font-size: 20px;
                font-weight: bold;
                padding: 5px 20px;
                border-radius: 5px;
                font-family: 'Digital', sans-serif;
            }
            #steeringBarContainer {
                width: 400px;
                height: 20px;
                background: #333;
                margin: 40px auto;
                position: relative;
                border-radius: 10px;
            }
            #steeringBar {
                height: 100%;
                background: orange;
                position: absolute;
                top: 0;
            }
            #steeringLabel {
                margin-top: -20px;
                font-size: 20px;
                color: #aaa;
            }
        </style>
    </head>
    <body>
        <h1 id="header">Telemetry Dashboard</h1>
        <canvas id="telemetryCanvas"  width="800" height="500"></canvas>

        <div id="drs">DRS</div>
        <div id="gear">Gear 0</div>

        <div id="steeringBarContainer">
            <div id="steeringBar"></div>
        </div>
        <div id="steeringLabel">Steer 0.00</div>

        <!-- don't forget to add columns to the mySQL Query as the dashboard gets bigger -->
        <script>
            const socket = new WebSocket('ws://localhost:3000');

            const canvas = document.getElementById('telemetryCanvas');
            const ctx = canvas.getContext('2d');
            const dpr = window.devicePixelRatio || 1;

            const logicalWidth = 800;
            const logicalHeight = 500;

            canvas.width = logicalWidth * dpr;
            canvas.height = logicalHeight * dpr;
            canvas.style.width = logicalWidth + 'px';
            canvas.style.height = logicalHeight + 'px';
            ctx.scale(dpr, dpr);

            const centerX = logicalWidth / 2;
            const centerY = logicalHeight / 2;
            const radius_spd = 200;
            const radius_tb = 150;

            const drawArc_spd = (start, end, color, thickness) => {
                ctx.beginPath();
                ctx.arc(centerX, centerY, radius_spd, start, end);
                ctx.strokeStyle = color;
                ctx.lineWidth = thickness;
                ctx.lineCap = 'round';
                ctx.stroke();
            };

            const drawArc_tb = (start, end, color, thickness) => {
                ctx.beginPath();
                ctx.arc(centerX, centerY, radius_tb, start, end);
                ctx.strokeStyle = color;
                ctx.lineWidth = thickness;
                ctx.lineCap = 'round';
                ctx.stroke();
            };

            const drawText = (text, y, size = 30) => {
                ctx.font = `${size}px Digital`;
                ctx.fillStyle = '#fff';
                ctx.textAlign = 'center';
                ctx.fillText(text, centerX, y);
            };

            function renderTelemetry(data) {
                ctx.clearRect(0, 0, canvas.width, canvas.height);

                const degToRad = d => d * Math.PI / 180;
                const baseAngle = Math.PI * 0.66;
                const fullCircle = Math.PI * 1.68;
                const fullCircle_tb = Math.PI * 0.97;
                const thick = 40;
                const size = 40;

                drawArc_spd(baseAngle, baseAngle + fullCircle, '#0a6efd22', thick);
                const speedAngle = baseAngle + (data.speed / 360) * fullCircle;
                drawArc_spd(baseAngle, speedAngle, '#005eed', thick);

                const throttleStart = degToRad(120);
                const throttleEnd = throttleStart + data.throttle * degToRad(174.5);
                drawArc_tb(throttleStart, throttleStart + fullCircle_tb, '#32CD3222', thick);
                drawArc_tb(throttleStart, throttleEnd, '#22BD22', thick);

                const brakeStart = degToRad(60);
                const brakeEnd = brakeStart - data.brake * degToRad(104.5);
                drawArc_tb(brakeStart - fullCircle_tb + degToRad(70), brakeStart, '#DD222222', thick);
                drawArc_tb(brakeEnd, brakeStart, '#CD1212', thick);

                drawText(`${Math.round(data.speed)}`, centerY - 15, size);
                drawText('KMH', centerY + 5, size * 0.3);
                drawText(`${data.engineRPM}`, centerY + 50, size * 0.7);
                drawText('RPM', centerY + 70, size * 0.3);

                let gearText = data.gear === 0 ? 'N' : data.gear === -1 ? 'R' : data.gear;
                document.getElementById('gear').textContent = `Gear ${gearText}`;

                const drs = document.getElementById('drs');
                if (data.drs === 1) {
                    drs.style.background = '#42DD42';
                    drs.style.color = '#000';
                } else {
                    drs.style.background = 'transparent';
                    drs.style.color = '#fff';
                }

                const bar = document.getElementById('steeringBar');
                const label = document.getElementById('steeringLabel');
                const containerWidth = document.getElementById('steeringBarContainer').offsetWidth;
                const halfWidth = containerWidth / 2;

                const steerNormalized = Math.max(-1, Math.min(1, data.steer));
                if (steerNormalized >= 0) {
                    bar.style.left = halfWidth + 'px';
                    bar.style.width = (halfWidth * steerNormalized) + 'px';
                } else {
                    bar.style.left = (halfWidth + steerNormalized * halfWidth) + 'px';
                    bar.style.width = (-halfWidth * steerNormalized) + 'px';
                }
                label.textContent = `Steer ${steerNormalized.toFixed(2)}`;
            }
            socket.onmessage = (event) => {
                console.log("RECEIVED DATA:", event.data);
                const data = JSON.parse(event.data);
                renderTelemetry(data);
            };
            setInterval(() => {
                if (socket.readyState === WebSocket.OPEN) {
                    socket.send('__ping__');
                }
            }, 500);
        </script>
    </body>
</html>