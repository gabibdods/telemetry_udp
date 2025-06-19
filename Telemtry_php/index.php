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
                font-family: 'Digital';
                position: relative;
                align-content: center;
                text-align: center;
                background: #222;
                color: #eee;
            }
            #header {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                top: 10px;
            }
            canvas {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                height: 500px;
                width: 800px;
                background: transparent;
                border-radius: 50%;
            }
            #staticCanvas {
                left : 400px;
                z-index: 0;
            }
            #dynamicCanvas {
                top : -1px;
                left : -406px;
                z-index: 1;
            }
            #speed {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                top: -330px;
                font-size: 40px;
            }
            #kmh {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                top: -320px;
            }
            #engineRPM {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                top: -310px;
                font-size: 32px;
            }
            #rpm {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                top: -302px;
            }
            #gear {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                top: -355px;
                height: 23px;
                width: 110px;
                font-size: 20px;
                margin: 70px auto;
                font-weight: bold;
                padding: 5px 20px;
                border-radius: 5px;
            }
            #drs {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                top: -300px;
                height: 23px;
                width: 100px;
                font-size: 20px;
                font-weight: bold;
                margin: -70px auto;
                padding: 5px 20px;
                border-radius: 5px;
            }
            #steeringBarContainer {
                position: relative;
                align-content: center;
                top: -120px;
                height: 20px;
                width: 400px;
                background: #333;
                margin: 40px auto;
                border-radius: 10px;
            }
            #steeringBar {
                position: absolute;
                align-content: center;
                top: 0;
                height: 100%;
                background: orange;
            }
            #steeringLabel {
                font-family: 'Digital';
                position: relative;
                align-content: center;
                top: -130px;
                font-size: 20px;
                margin-top: -20px;
                color: orange;
            }
            .sessionDataDisplay {
                position: relative;
                top: -800px;
            }
            .rouletteContainer {
                position: relative;
                width: 600px;
                height: 59px;
                overflow: hidden;
                border-radius: 8px;
                box-shadow: 0 0 15px #000;
                background-color: #222;
            }
            .rouletteTable {
                transition: transform 0.5s ease-in-out;
                position: absolute;
                width: 100%;
            }
            .rouletteRow {
                justify-content: space-between;
                display: flex;
                padding: 16px 24px;
                box-sizing: border-box;
                width: 100%;
                height: 60px;
                border-bottom: 1px solid #333;
                background: #222;
                color: #eee;
            }
            .rouletteRow:not(:nth-child(1)) {
                box-shadow: 0 -20px 20px -20px #000 inset;
            }
            .rouletteRow.next {
                box-shadow: 0 20px 20px -20px #000 inset;
            }
            button {
                position: relative;
                background: #444;
                color: #fff;
                border: none;
                padding: 8px 16px;
                border-radius: 4px;
                cursor: pointer;
            }
            button:hover {
                color: #666;
            }
            .buttons {
                position: absolute;
                align-items: center;
                gap: 12px;
                margin: 0 220px;
            }
            .scrollUp {
                background: transparent;
                height: 29px;
                color: #eee;
                top: -11px;
                left: 90px;
                font-size: 20px;
            }
            .scrollDown {
                background: transparent;
                height: 29px;
                color: #eee;
                top: -11px;
                left: 80px;
                font-size: 20px;
            }
        </style>
    </head>
    <body>
        <h1 id="header">Telemetry Dashboard</h1>
        <canvas id="staticCanvas"></canvas>
        <canvas id="dynamicCanvas"></canvas>
        <div id="speed">---</div>
        <div id="kmh">KMH</div>
        <div id="engineRPM">-----</div>
        <div id="rpm">RPM</div>
        <div id="gear">Gear -</div>
        <div id="drs">DRS</div>
        <div id="steeringBarContainer">
            <div id="steeringBar"></div>
        </div>
        <div id="steeringLabel">Steering</div>
        <div class="sessionDataDisplay">
            <div class="rouletteContainer">
                <div id="rouletteTable" class="rouletteTable">
                    <div class="rouletteRow">
                        <strong>Weather</strong>
                        <span>-----</span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Track Temp</strong>
                        <span>--°C</span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Air Temp</strong>
                        <span>--°C</span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Total Laps</strong>
                        <span>--</span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Track Length</strong>
                        <span>----m</span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Time Left</strong>
                        <span>--m--s</span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                    <div class="rouletteRow">
                        <strong>Duration</strong>
                        <span>--m--s</span>
                        <span class="buttons">
                            <button id="scrollUpArrow" class="scrollUp" onclick="scrollUp()">⇑</button>
                            <button id="scrollDownArrow" class="scrollDown" onclick="scrollDown()">⇓</button>
                        </span>
                    </div>
                </div>
            </div>
        </div>
        <script>
            const table = document.getElementById('rouletteTable');
            const rowHeight = 60;
            let currentIndex = 0;
            const totalRows = table.children.length;
            const interval = setInterval(function() {
                    if (currentIndex === totalRows - 1) {
                        currentIndex = -1;
                    }
                    if (currentIndex < totalRows - 1) {
                        currentIndex++;
                    }
                    updateScroll();
                }, 3000
            );
            function updateScroll() {
                table.style.transform = `translateY(-${currentIndex * rowHeight}px)`;
            }
            function scrollDown() {
                if (currentIndex === totalRows - 1) {
                    currentIndex = -1;
                }
                if (currentIndex < totalRows - 1) {
                    currentIndex++;
                }
                updateScroll();
            }
            function scrollUp() {
                if (currentIndex === 0) {
                    currentIndex = totalRows;
                }
                if (currentIndex > 0) {
                    currentIndex--;
                }
                updateScroll();
            }
            const socket = new WebSocket('ws://localhost:3000');
            const dpr = window.devicePixelRatio || 1;
            const logicalWidth = 800;
            const logicalHeight = 500;
/* debug * let dataBase = { speed : 360, throttle : 1, brake : 1, engineRPM : 13500, drs : 1, gear : 8, steer : 1 }*/
            const staticCanvas = document.getElementById('staticCanvas');
            const ctxStatic = staticCanvas.getContext('2d');
            staticCanvas.height = logicalHeight * dpr;
            staticCanvas.width = logicalWidth * dpr;
            ctxStatic.scale(dpr, dpr);

            const dynamicCanvas = document.getElementById('dynamicCanvas');
            const ctxDynamic = dynamicCanvas.getContext('2d');
            dynamicCanvas.height = logicalHeight * dpr;
            dynamicCanvas.width = logicalWidth * dpr;
            ctxDynamic.scale(dpr, dpr);

            const centerX = logicalWidth / 2;
            const centerY = logicalHeight / 2;
            const radiusSpeed = 200;
            const radiusThrottle = 150;
            const radiusBrake = 150;
            // statics in the dynamic
            const degToRad = d => d * Math.PI / 180;
            const baseAngle = Math.PI * 0.66;
            const fullCircle = Math.PI * 1.68;
            const fullCircle_tb = Math.PI * 0.97;
            const thick = 40;
            const size = 40;
            const brakeStart = degToRad(60);
            const throttleStart = degToRad(120);
            const drs = document.getElementById('drs');
            const bar = document.getElementById('steeringBar');
            const label = document.getElementById('steeringLabel');
            const halfWidth = 200;
            // end
            const speedArcs = new Map();
            for (let speed = 0; speed <= 360; speed++) {
                const offCanvas = document.createElement('canvas');
                const offCtx = offCanvas.getContext('2d');
                offCanvas.height = logicalHeight * dpr;
                offCanvas.width = logicalWidth * dpr;
                offCtx.translate(-3, -5);
                offCtx.beginPath();
                offCtx.arc(centerX, centerY, radiusSpeed, Math.PI * 0.66, Math.PI * 0.66 + (speed / 360) * Math.PI * 1.68);
                offCtx.strokeStyle = '#005eed';
                offCtx.lineWidth = thick;
                offCtx.lineCap = 'round';
                offCtx.stroke();
                speedArcs.set(speed, offCanvas);
            }
            const drawSpeedArc = (currentSpeed) => {
                const speed = Math.floor(currentSpeed);
                const canvasSpeedArc = speedArcs.get(speed);
                const speedAngle = baseAngle + (speed / 360) * fullCircle;
                ctxDynamic.drawImage(canvasSpeedArc, baseAngle, speedAngle)

            };
            const drawSpeedBackgroundArc = (start, end, color, thickness) => {
                ctxStatic.beginPath();
                ctxStatic.arc(centerX, centerY, radiusSpeed, start, end);
                ctxStatic.strokeStyle = color;
                ctxStatic.lineWidth = thickness;
                ctxStatic.lineCap = 'round';
                ctxStatic.stroke();
            };
            const drawThrottleArc = (start, end, color, thickness) => {
                ctxDynamic.beginPath();
                ctxDynamic.arc(centerX, centerY, radiusThrottle, start, end);
                ctxDynamic.strokeStyle = color;
                ctxDynamic.lineWidth = thickness;
                ctxDynamic.lineCap = 'round';
                ctxDynamic.stroke();
            };
            const drawThrottleBackgroundArc = (start, end, color, thickness) => {
                ctxStatic.beginPath();
                ctxStatic.arc(centerX, centerY, radiusThrottle, start, end);
                ctxStatic.strokeStyle = color;
                ctxStatic.lineWidth = thickness;
                ctxStatic.lineCap = 'round';
                ctxStatic.stroke();
            };
            const drawBrakeArc = (start, end, color, thickness) => {
                ctxDynamic.beginPath();
                ctxDynamic.arc(centerX, centerY, radiusBrake, start, end);
                ctxDynamic.strokeStyle = color;
                ctxDynamic.lineWidth = thickness;
                ctxDynamic.lineCap = 'round';
                ctxDynamic.stroke();
            };
            const drawBrakeBackgroundArc = (start, end, color, thickness) => {
                ctxStatic.beginPath();
                ctxStatic.arc(centerX, centerY, radiusBrake, start, end);
                ctxStatic.strokeStyle = color;
                ctxStatic.lineWidth = thickness;
                ctxStatic.lineCap = 'round';
                ctxStatic.stroke();
            };
            drawSpeedBackgroundArc(baseAngle, baseAngle + fullCircle, '#0a6efd22', thick);
            drawThrottleBackgroundArc(throttleStart, throttleStart + fullCircle_tb, '#32CD3222', thick);
            drawBrakeBackgroundArc(brakeStart - fullCircle_tb + degToRad(70), brakeStart, '#DD222222', thick);
            function renderTelemetry(data) {
                ctxDynamic.clearRect(0, 0, dynamicCanvas.width, dynamicCanvas.height)
                drawSpeedArc(data.speed);

                const throttleEnd = throttleStart + data.throttle * degToRad(174.5);
                drawThrottleArc(throttleStart, throttleEnd, '#22BD22', thick);

                const brakeEnd = brakeStart - data.brake * degToRad(104.5);
                drawBrakeArc(brakeEnd, brakeStart, '#CD1212', thick);

                document.getElementById('speed').textContent = `${Math.floor(data.speed)}`;

                document.getElementById('engineRPM').textContent = `${data.engineRPM}`;

                if (data.drs === 1) {
                    drs.style.background = '#42DD42';
                    drs.style.color = '#000';
                } else {
                    drs.style.background = 'transparent';
                    drs.style.color = '#eee';
                }
                let gearText = data.gear === 0 ? 'N' : data.gear === -1 ? 'R' : data.gear;
                document.getElementById('gear').textContent = `Gear ${gearText}`;

                const steerNormalized = Math.max(-1, Math.min(1, data.steer));
                if (steerNormalized >= 0) {
                    bar.style.left = halfWidth + 'px';
                    bar.style.width = (halfWidth * steerNormalized) + 'px';
                } else {
                    bar.style.left = (halfWidth + steerNormalized * halfWidth) + 'px';
                    bar.style.width = (-halfWidth * steerNormalized) + 'px';
                }
            }
/* debug * renderTelemetry(dataBase);*/
            socket.onmessage = (event) => {
                const dataBase = JSON.parse(event.data);
                /* currently using:
                *   telemetryCarTelemetryData.speed
                *   telemetryCarTelemetryData.throttle
                *   telemetryCarTelemetryData.steer
                *   telemetryCarTelemetryData.brake
                *   telemetryCarTelemetryData.gear
                *   telemetryCarTelemetryData.engineRPM
                *   telemetryCarTelemetryData.drs
                */
                renderTelemetry(dataBase);
            };
            setInterval(() => {
                if (socket.readyState === WebSocket.OPEN) {
                    socket.send('__ping__');
                }
            }, 500);
        </script>
    </body>
</html>