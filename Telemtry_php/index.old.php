<?php
require_once('includes/functions.php');
$data = get_latest_telemetry();
?>

<!DOCTYPE html>
<html lang="en">
    <head>
        <title>Live F1 Telemetry</title>
    </head>
    <body>
    <h1>Live F1 Telemetry</h1>
    <!-- missing 7 -->

    <p>Speed: <span id="speed"><?= $data['speed'] ?? '--' ?></span> km/h</p>
    <p>Throttle: <span id="throttle"><?= $data['throttle'] ?? '--' ?></span></p>
    <p>Steer: <span id="steer"><?= $data['steer'] ?? '--' ?></span></p>
    <p>Brake: <span id="brake"><?= $data['brake'] ?? '--' ?></span></p>
    <!-- abandoned -->
    <p>Clutch: <span id="clutch"><?= $data['clutch'] ?? '--' ?></span></p>
    <p>Gear: <span id="gear"><?= $data['gear'] ?? '--' ?></span></p>
    <p>Engine RPM: <span id="engineRPM"><?= $data['engineRPM'] ?? '--' ?></span></p>
    <p>DRS: <span id="drs"><?= $data['drs'] ?? '--' ?></span></p>
    <!-- missing -->
    <p>Rev Lights Percent: <span id="revLightsPercent"><?= $data['revLightsPercent'] ?? '--' ?></span></p>
    <!-- missing -->
    <p>Rev Lights' Bit Value: <span id="revLightsBitValue"><?= $data['revLightsBitValue'] ?? '--' ?></span></p>

    <!-- missing -->
    <p>Brakes Temperature:
        <span id="brakesTemperature0"><?= $data['brakesTemperature0'] ?? '--' ?></span>,
        <span id="brakesTemperature1"><?= $data['brakesTemperature1'] ?? '--' ?></span>,
        <span id="brakesTemperature2"><?= $data['brakesTemperature2'] ?? '--' ?></span>,
        <span id="brakesTemperature3"><?= $data['brakesTemperature3'] ?? '--' ?></span>
    </p>

    <!-- missing -->
    <p>Tyres Surface Temperature:
        <span id="tyresSurfaceTemperature0"><?= $data['tyresSurfaceTemperature0'] ?? '--' ?></span>,
        <span id="tyresSurfaceTemperature1"><?= $data['tyresSurfaceTemperature1'] ?? '--' ?></span>,
        <span id="tyresSurfaceTemperature2"><?= $data['tyresSurfaceTemperature2'] ?? '--' ?></span>,
        <span id="tyresSurfaceTemperature3"><?= $data['tyresSurfaceTemperature3'] ?? '--' ?></span>
    </p>

    <!-- missing -->
    <p>Tyres Inner Temperature:
        <span id="tyresInnerTemperature0"><?= $data['tyresInnerTemperature0'] ?? '--' ?></span>,
        <span id="tyresInnerTemperature1"><?= $data['tyresInnerTemperature1'] ?? '--' ?></span>,
        <span id="tyresInnerTemperature2"><?= $data['tyresInnerTemperature2'] ?? '--' ?></span>,
        <span id="tyresInnerTemperature3"><?= $data['tyresInnerTemperature3'] ?? '--' ?></span>
    </p>

    <!-- missing -->
    <p>Engine Temperature: <span id="engineTemperature"><?= $data['engineTemperature'] ?? '--' ?></span></p>

    <!-- missing -->
    <p>Tyres Pressure:
        <span id="tyresPressure0"><?= $data['tyresPressure0'] ?? '--' ?></span>,
        <span id="tyresPressure1"><?= $data['tyresPressure1'] ?? '--' ?></span>,
        <span id="tyresPressure2"><?= $data['tyresPressure2'] ?? '--' ?></span>,
        <span id="tyresPressure3"><?= $data['tyresPressure3'] ?? '--' ?></span>
    </p>

    <!-- abandoned -->
    <p>Surface Type:
        <span id="surfaceType0"><?= $data['surfaceType0'] ?? '--' ?></span>,
        <span id="surfaceType1"><?= $data['surfaceType1'] ?? '--' ?></span>,
        <span id="surfaceType2"><?= $data['surfaceType2'] ?? '--' ?></span>,
        <span id="surfaceType3"><?= $data['surfaceType3'] ?? '--' ?></span>
    </p>

    <script>
        const socket = new WebSocket('ws://localhost:3000');
        socket.onmessage = (event) => {
            const data = JSON.parse(event.data);

            document.getElementById('speed').textContent = data.speed;
            document.getElementById('throttle').textContent = data.throttle;
            document.getElementById('steer').textContent = data.steer;
            document.getElementById('brake').textContent = data.brake;
            document.getElementById('clutch').textContent = data.clutch;
            document.getElementById('gear').textContent = data.gear;
            document.getElementById('engineRPM').textContent = data.engineRPM;
            document.getElementById('drs').textContent = data.drs;
            document.getElementById('revLightsPercent').textContent = data.revLightsPercent;
            document.getElementById('revLightsBitValue').textContent = data.revLightsBitValue;

            data.brakesTemperature?.forEach((value, index) => {
                document.getElementById(`brakesTemperature${index}`).textContent = value;
            });
            data.tyresSurfaceTemperature?.forEach((value, index) => {
                document.getElementById(`tyresSurfaceTemperature${index}`).textContent = value;
            });
            data.tyresInnerTemperature?.forEach((value, index) => {
                document.getElementById(`tyresInnerTemperature${index}`).textContent = value;
            });
            document.getElementById('engineTemperature').textContent = data.engineTemperature;

            data.tyresPressure?.forEach((value, index) => {
                document.getElementById(`tyresPressure${index}`).textContent = value;
            });
            data.surfaceType?.forEach((value, index) => {
                document.getElementById(`surfaceType${index}`).textContent = value;
            });
        };
        socket.onerror = (error) => {
            console.error('WebSocket error:', error);
        };
    </script>
    </body>
</html>