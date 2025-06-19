using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using MySqlConnector;


namespace Telemtry
{
    public class Worker : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var client = new UdpClient(20777);
            var connectionString = "Server=db;Port=3306;Database=telemetry;User=user;Password=pass;";

            while (!stoppingToken.IsCancellationRequested)
            {
                UdpReceiveResult result = await client.ReceiveAsync();
                var packet = result.Buffer;
/* debug **     Console.WriteLine($"L:{packet.Length}"); */

                using var ms = new MemoryStream(packet);
                using var br = new BinaryReader(ms);
//-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------Inserters
                if (packet.Length == 1352)
                {//-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of PacketCarTelemetryData of 1352B
                    PacketHeader packetHeaderCarTelemetryData = new PacketHeader
                    {
                        packetFormat = br.ReadUInt16(),
                        gameYear = br.ReadByte(),
                        gameMajorVersion = br.ReadByte(),
                        gameMinorVersion = br.ReadByte(),
                        packetVersion = br.ReadByte(),
                        packetId = br.ReadByte(),
                        sessionUID = br.ReadUInt64(),
                        sessionTime = br.ReadSingle(),
                        frameIdentifier = br.ReadUInt32(),
                        overallFrameIdentifier = br.ReadUInt32(),
                        playerCarIndex = br.ReadByte(),
                        secondaryPlayerCarIndex = br.ReadByte()
                    };
                    CarTelemetryData[] carTelemetryData = new CarTelemetryData[22];
                    for (int i = 0; i < 22; i++)
                    {
                        carTelemetryData[i] = ReadCarTelemetryData(br);
                    }
                    byte mfdPanelIndex = br.ReadByte();                                     // 0�4 or 255
                    byte mfdPanelIndexSecondaryPlayer = br.ReadByte();                      // 0�4 or 255
                    sbyte suggestedGear = br.ReadSByte();                                   // -1, 0 = none, 1�8
                 //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                    var dataCarTelemetry = carTelemetryData[packetHeaderCarTelemetryData.playerCarIndex];
                    
                    using var conn = new MySqlConnection(connectionString);
                    await conn.OpenAsync();

                    using var cmd = new MySqlCommand(@"
                        INSERT INTO telemetryCarTelemetryData (
                            speed, throttle, steer, brake, clutch, gear, engineRPM, drs, revLightsPercent, revLightsBitValue,
                            brakesTemperature0, brakesTemperature1, brakesTemperature2, brakesTemperature3,
                            tyresSurfaceTemperature0, tyresSurfaceTemperature1, tyresSurfaceTemperature2, tyresSurfaceTemperature3,
                            tyresInnerTemperature0, tyresInnerTemperature1, tyresInnerTemperature2, tyresInnerTemperature3,
                            engineTemperature,
                            tyresPressure0, tyresPressure1, tyresPressure2, tyresPressure3,
                            surfaceType0, surfaceType1, surfaceType2, surfaceType3
                        ) VALUES (
                            @speed, @throttle, @steer, @brake, @clutch, @gear, @engineRPM, @drs, @revLightsPercent, @revLightsBitValue,
                            @brakesTemperature0, @brakesTemperature1, @brakesTemperature2, @brakesTemperature3,
                            @tyresSurfaceTemperature0, @tyresSurfaceTemperature1, @tyresSurfaceTemperature2, @tyresSurfaceTemperature3,
                            @tyresInnerTemperature0, @tyresInnerTemperature1, @tyresInnerTemperature2, @tyresInnerTemperature3,
                            @engineTemperature,
                            @tyresPressure0, @tyresPressure1, @tyresPressure2, @tyresPressure3,
                            @surfaceType0, @surfaceType1, @surfaceType2, @surfaceType3
                        )", conn
                    );
                    cmd.Parameters.AddWithValue("@speed", dataCarTelemetry.speed);
                    cmd.Parameters.AddWithValue("@throttle", dataCarTelemetry.throttle);
                    cmd.Parameters.AddWithValue("@steer", dataCarTelemetry.steer);
                    cmd.Parameters.AddWithValue("@brake", dataCarTelemetry.brake);
                    cmd.Parameters.AddWithValue("@clutch", dataCarTelemetry.clutch);
                    cmd.Parameters.AddWithValue("@gear", dataCarTelemetry.gear);
                    cmd.Parameters.AddWithValue("@engineRPM", dataCarTelemetry.engineRPM);
                    cmd.Parameters.AddWithValue("@drs", dataCarTelemetry.drs);
                    cmd.Parameters.AddWithValue("@revLightsPercent", dataCarTelemetry.revLightsPercent);
                    cmd.Parameters.AddWithValue("@revLightsBitValue", dataCarTelemetry.revLightsBitValue);
                    cmd.Parameters.AddWithValue("@brakesTemperature0", dataCarTelemetry.brakesTemperature?[0] ?? 0);
                    cmd.Parameters.AddWithValue("@brakesTemperature1", dataCarTelemetry.brakesTemperature?[1] ?? 0);
                    cmd.Parameters.AddWithValue("@brakesTemperature2", dataCarTelemetry.brakesTemperature?[2] ?? 0);
                    cmd.Parameters.AddWithValue("@brakesTemperature3", dataCarTelemetry.brakesTemperature?[3] ?? 0);
                    cmd.Parameters.AddWithValue("@tyresSurfaceTemperature0", dataCarTelemetry.tyresSurfaceTemperature?[0] ?? 0);
                    cmd.Parameters.AddWithValue("@tyresSurfaceTemperature1", dataCarTelemetry.tyresSurfaceTemperature?[1] ?? 0);
                    cmd.Parameters.AddWithValue("@tyresSurfaceTemperature2", dataCarTelemetry.tyresSurfaceTemperature?[2] ?? 0);
                    cmd.Parameters.AddWithValue("@tyresSurfaceTemperature3", dataCarTelemetry.tyresSurfaceTemperature?[3] ?? 0);
                    cmd.Parameters.AddWithValue("@tyresInnerTemperature0", dataCarTelemetry.tyresInnerTemperature?[0] ?? 0);
                    cmd.Parameters.AddWithValue("@tyresInnerTemperature1", dataCarTelemetry.tyresInnerTemperature?[1] ?? 0);
                    cmd.Parameters.AddWithValue("@tyresInnerTemperature2", dataCarTelemetry.tyresInnerTemperature?[2] ?? 0);
                    cmd.Parameters.AddWithValue("@tyresInnerTemperature3", dataCarTelemetry.tyresInnerTemperature?[3] ?? 0);
                    cmd.Parameters.AddWithValue("@engineTemperature", dataCarTelemetry.engineTemperature);
                    cmd.Parameters.AddWithValue("@tyresPressure0", dataCarTelemetry.tyresPressure?[0] ?? 0);
                    cmd.Parameters.AddWithValue("@tyresPressure1", dataCarTelemetry.tyresPressure?[1] ?? 0);
                    cmd.Parameters.AddWithValue("@tyresPressure2", dataCarTelemetry.tyresPressure?[2] ?? 0);
                    cmd.Parameters.AddWithValue("@tyresPressure3", dataCarTelemetry.tyresPressure?[3] ?? 0);
                    cmd.Parameters.AddWithValue("@surfaceType0", dataCarTelemetry.surfaceType?[0] ?? 0);
                    cmd.Parameters.AddWithValue("@surfaceType1", dataCarTelemetry.surfaceType?[1] ?? 0);
                    cmd.Parameters.AddWithValue("@surfaceType2", dataCarTelemetry.surfaceType?[2] ?? 0);
                    cmd.Parameters.AddWithValue("@surfaceType3", dataCarTelemetry.surfaceType?[3] ?? 0);
                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"telemetryCarTelemetryData sql: {ex}");
                        continue;
                    }
                    continue;
                }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                if (packet.Length == 753)
                {//-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------start of PacketSessionData of 753B
                    PacketHeader packetHeaderPacketSessionData = new PacketHeader
                    {
                        packetFormat = br.ReadUInt16(),
                        gameYear = br.ReadByte(),
                        gameMajorVersion = br.ReadByte(),
                        gameMinorVersion = br.ReadByte(),
                        packetVersion = br.ReadByte(),
                        packetId = br.ReadByte(),
                        sessionUID = br.ReadUInt64(),
                        sessionTime = br.ReadSingle(),
                        frameIdentifier = br.ReadUInt32(),
                        overallFrameIdentifier = br.ReadUInt32(),
                        playerCarIndex = br.ReadByte(),
                        secondaryPlayerCarIndex = br.ReadByte()
                    };
                    var dataPacketSession = ReadPacketSessionData(br);
                 //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------end
                    using var conn = new MySqlConnection(connectionString);
                    await conn.OpenAsync();

                    using var cmd = new MySqlCommand(@"
                        INSERT INTO telemetrySessionData (
                            weather, trackTemperature, airTemperature, totalLaps, trackLength, sessionType, trackId, formula, sessionTimeLeft, sessionDuration, pitSpeedLimit, gamePaused, isSpectating, spectatorCarIndex, sliProNativeSupport, numMarshalZones,
                            marshalZones0_zoneStart, marshalZones0_zoneFlag,
                            marshalZones1_zoneStart, marshalZones1_zoneFlag,
                            marshalZones2_zoneStart, marshalZones2_zoneFlag,
                            marshalZones3_zoneStart, marshalZones3_zoneFlag,
                            marshalZones4_zoneStart, marshalZones4_zoneFlag,
                            marshalZones5_zoneStart, marshalZones5_zoneFlag,
                            marshalZones6_zoneStart, marshalZones6_zoneFlag,
                            marshalZones7_zoneStart, marshalZones7_zoneFlag,
                            marshalZones8_zoneStart, marshalZones8_zoneFlag,
                            marshalZones9_zoneStart, marshalZones9_zoneFlag,
                            marshalZones10_zoneStart, marshalZones10_zoneFlag,
                            marshalZones11_zoneStart, marshalZones11_zoneFlag,
                            marshalZones12_zoneStart, marshalZones12_zoneFlag,
                            marshalZones13_zoneStart, marshalZones13_zoneFlag,
                            marshalZones14_zoneStart, marshalZones14_zoneFlag,
                            marshalZones15_zoneStart, marshalZones15_zoneFlag,
                            marshalZones16_zoneStart, marshalZones16_zoneFlag,
                            marshalZones17_zoneStart, marshalZones17_zoneFlag,
                            marshalZones18_zoneStart, marshalZones18_zoneFlag,
                            marshalZones19_zoneStart, marshalZones19_zoneFlag,
                            marshalZones20_zoneStart, marshalZones20_zoneFlag,
                            safetyCarStatus, networkGame, numWeatherForecastSamples,
                            weatherForecastSamples0_sessionType, weatherForecastSamples0_timeOffset, weatherForecastSamples0_weather, weatherForecastSamples0_trackTemperature, weatherForecastSamples0_trackTemperatureChange, weatherForecastSamples0_airTemperature, weatherForecastSamples0_airTemperatureChange, weatherForecastSamples0_rainPercentage,
                            weatherForecastSamples1_sessionType, weatherForecastSamples1_timeOffset, weatherForecastSamples1_weather, weatherForecastSamples1_trackTemperature, weatherForecastSamples1_trackTemperatureChange, weatherForecastSamples1_airTemperature, weatherForecastSamples1_airTemperatureChange, weatherForecastSamples1_rainPercentage,
                            weatherForecastSamples2_sessionType, weatherForecastSamples2_timeOffset, weatherForecastSamples2_weather, weatherForecastSamples2_trackTemperature, weatherForecastSamples2_trackTemperatureChange, weatherForecastSamples2_airTemperature, weatherForecastSamples2_airTemperatureChange, weatherForecastSamples2_rainPercentage,
                            weatherForecastSamples3_sessionType, weatherForecastSamples3_timeOffset, weatherForecastSamples3_weather, weatherForecastSamples3_trackTemperature, weatherForecastSamples3_trackTemperatureChange, weatherForecastSamples3_airTemperature, weatherForecastSamples3_airTemperatureChange, weatherForecastSamples3_rainPercentage,
                            weatherForecastSamples4_sessionType, weatherForecastSamples4_timeOffset, weatherForecastSamples4_weather, weatherForecastSamples4_trackTemperature, weatherForecastSamples4_trackTemperatureChange, weatherForecastSamples4_airTemperature, weatherForecastSamples4_airTemperatureChange, weatherForecastSamples4_rainPercentage,
                            weatherForecastSamples5_sessionType, weatherForecastSamples5_timeOffset, weatherForecastSamples5_weather, weatherForecastSamples5_trackTemperature, weatherForecastSamples5_trackTemperatureChange, weatherForecastSamples5_airTemperature, weatherForecastSamples5_airTemperatureChange, weatherForecastSamples5_rainPercentage,
                            weatherForecastSamples6_sessionType, weatherForecastSamples6_timeOffset, weatherForecastSamples6_weather, weatherForecastSamples6_trackTemperature, weatherForecastSamples6_trackTemperatureChange, weatherForecastSamples6_airTemperature, weatherForecastSamples6_airTemperatureChange, weatherForecastSamples6_rainPercentage,
                            weatherForecastSamples7_sessionType, weatherForecastSamples7_timeOffset, weatherForecastSamples7_weather, weatherForecastSamples7_trackTemperature, weatherForecastSamples7_trackTemperatureChange, weatherForecastSamples7_airTemperature, weatherForecastSamples7_airTemperatureChange, weatherForecastSamples7_rainPercentage,
                            weatherForecastSamples8_sessionType, weatherForecastSamples8_timeOffset, weatherForecastSamples8_weather, weatherForecastSamples8_trackTemperature, weatherForecastSamples8_trackTemperatureChange, weatherForecastSamples8_airTemperature, weatherForecastSamples8_airTemperatureChange, weatherForecastSamples8_rainPercentage,
                            weatherForecastSamples9_sessionType, weatherForecastSamples9_timeOffset, weatherForecastSamples9_weather, weatherForecastSamples9_trackTemperature, weatherForecastSamples9_trackTemperatureChange, weatherForecastSamples9_airTemperature, weatherForecastSamples9_airTemperatureChange, weatherForecastSamples9_rainPercentage,
                            weatherForecastSamples10_sessionType, weatherForecastSamples10_timeOffset, weatherForecastSamples10_weather, weatherForecastSamples10_trackTemperature, weatherForecastSamples10_trackTemperatureChange, weatherForecastSamples10_airTemperature, weatherForecastSamples10_airTemperatureChange, weatherForecastSamples10_rainPercentage,
                            weatherForecastSamples11_sessionType, weatherForecastSamples11_timeOffset, weatherForecastSamples11_weather, weatherForecastSamples11_trackTemperature, weatherForecastSamples11_trackTemperatureChange, weatherForecastSamples11_airTemperature, weatherForecastSamples11_airTemperatureChange, weatherForecastSamples11_rainPercentage,
                            weatherForecastSamples12_sessionType, weatherForecastSamples12_timeOffset, weatherForecastSamples12_weather, weatherForecastSamples12_trackTemperature, weatherForecastSamples12_trackTemperatureChange, weatherForecastSamples12_airTemperature, weatherForecastSamples12_airTemperatureChange, weatherForecastSamples12_rainPercentage,
                            weatherForecastSamples13_sessionType, weatherForecastSamples13_timeOffset, weatherForecastSamples13_weather, weatherForecastSamples13_trackTemperature, weatherForecastSamples13_trackTemperatureChange, weatherForecastSamples13_airTemperature, weatherForecastSamples13_airTemperatureChange, weatherForecastSamples13_rainPercentage,
                            weatherForecastSamples14_sessionType, weatherForecastSamples14_timeOffset, weatherForecastSamples14_weather, weatherForecastSamples14_trackTemperature, weatherForecastSamples14_trackTemperatureChange, weatherForecastSamples14_airTemperature, weatherForecastSamples14_airTemperatureChange, weatherForecastSamples14_rainPercentage,
                            weatherForecastSamples15_sessionType, weatherForecastSamples15_timeOffset, weatherForecastSamples15_weather, weatherForecastSamples15_trackTemperature, weatherForecastSamples15_trackTemperatureChange, weatherForecastSamples15_airTemperature, weatherForecastSamples15_airTemperatureChange, weatherForecastSamples15_rainPercentage,
                            weatherForecastSamples16_sessionType, weatherForecastSamples16_timeOffset, weatherForecastSamples16_weather, weatherForecastSamples16_trackTemperature, weatherForecastSamples16_trackTemperatureChange, weatherForecastSamples16_airTemperature, weatherForecastSamples16_airTemperatureChange, weatherForecastSamples16_rainPercentage,
                            weatherForecastSamples17_sessionType, weatherForecastSamples17_timeOffset, weatherForecastSamples17_weather, weatherForecastSamples17_trackTemperature, weatherForecastSamples17_trackTemperatureChange, weatherForecastSamples17_airTemperature, weatherForecastSamples17_airTemperatureChange, weatherForecastSamples17_rainPercentage,
                            weatherForecastSamples18_sessionType, weatherForecastSamples18_timeOffset, weatherForecastSamples18_weather, weatherForecastSamples18_trackTemperature, weatherForecastSamples18_trackTemperatureChange, weatherForecastSamples18_airTemperature, weatherForecastSamples18_airTemperatureChange, weatherForecastSamples18_rainPercentage,
                            weatherForecastSamples19_sessionType, weatherForecastSamples19_timeOffset, weatherForecastSamples19_weather, weatherForecastSamples19_trackTemperature, weatherForecastSamples19_trackTemperatureChange, weatherForecastSamples19_airTemperature, weatherForecastSamples19_airTemperatureChange, weatherForecastSamples19_rainPercentage,
                            weatherForecastSamples20_sessionType, weatherForecastSamples20_timeOffset, weatherForecastSamples20_weather, weatherForecastSamples20_trackTemperature, weatherForecastSamples20_trackTemperatureChange, weatherForecastSamples20_airTemperature, weatherForecastSamples20_airTemperatureChange, weatherForecastSamples20_rainPercentage,
                            weatherForecastSamples21_sessionType, weatherForecastSamples21_timeOffset, weatherForecastSamples21_weather, weatherForecastSamples21_trackTemperature, weatherForecastSamples21_trackTemperatureChange, weatherForecastSamples21_airTemperature, weatherForecastSamples21_airTemperatureChange, weatherForecastSamples21_rainPercentage,
                            weatherForecastSamples22_sessionType, weatherForecastSamples22_timeOffset, weatherForecastSamples22_weather, weatherForecastSamples22_trackTemperature, weatherForecastSamples22_trackTemperatureChange, weatherForecastSamples22_airTemperature, weatherForecastSamples22_airTemperatureChange, weatherForecastSamples22_rainPercentage,
                            weatherForecastSamples23_sessionType, weatherForecastSamples23_timeOffset, weatherForecastSamples23_weather, weatherForecastSamples23_trackTemperature, weatherForecastSamples23_trackTemperatureChange, weatherForecastSamples23_airTemperature, weatherForecastSamples23_airTemperatureChange, weatherForecastSamples23_rainPercentage,
                            weatherForecastSamples24_sessionType, weatherForecastSamples24_timeOffset, weatherForecastSamples24_weather, weatherForecastSamples24_trackTemperature, weatherForecastSamples24_trackTemperatureChange, weatherForecastSamples24_airTemperature, weatherForecastSamples24_airTemperatureChange, weatherForecastSamples24_rainPercentage,
                            weatherForecastSamples25_sessionType, weatherForecastSamples25_timeOffset, weatherForecastSamples25_weather, weatherForecastSamples25_trackTemperature, weatherForecastSamples25_trackTemperatureChange, weatherForecastSamples25_airTemperature, weatherForecastSamples25_airTemperatureChange, weatherForecastSamples25_rainPercentage,
                            weatherForecastSamples26_sessionType, weatherForecastSamples26_timeOffset, weatherForecastSamples26_weather, weatherForecastSamples26_trackTemperature, weatherForecastSamples26_trackTemperatureChange, weatherForecastSamples26_airTemperature, weatherForecastSamples26_airTemperatureChange, weatherForecastSamples26_rainPercentage,
                            weatherForecastSamples27_sessionType, weatherForecastSamples27_timeOffset, weatherForecastSamples27_weather, weatherForecastSamples27_trackTemperature, weatherForecastSamples27_trackTemperatureChange, weatherForecastSamples27_airTemperature, weatherForecastSamples27_airTemperatureChange, weatherForecastSamples27_rainPercentage,
                            weatherForecastSamples28_sessionType, weatherForecastSamples28_timeOffset, weatherForecastSamples28_weather, weatherForecastSamples28_trackTemperature, weatherForecastSamples28_trackTemperatureChange, weatherForecastSamples28_airTemperature, weatherForecastSamples28_airTemperatureChange, weatherForecastSamples28_rainPercentage,
                            weatherForecastSamples29_sessionType, weatherForecastSamples29_timeOffset, weatherForecastSamples29_weather, weatherForecastSamples29_trackTemperature, weatherForecastSamples29_trackTemperatureChange, weatherForecastSamples29_airTemperature, weatherForecastSamples29_airTemperatureChange, weatherForecastSamples29_rainPercentage,
                            weatherForecastSamples30_sessionType, weatherForecastSamples30_timeOffset, weatherForecastSamples30_weather, weatherForecastSamples30_trackTemperature, weatherForecastSamples30_trackTemperatureChange, weatherForecastSamples30_airTemperature, weatherForecastSamples30_airTemperatureChange, weatherForecastSamples30_rainPercentage,
                            weatherForecastSamples31_sessionType, weatherForecastSamples31_timeOffset, weatherForecastSamples31_weather, weatherForecastSamples31_trackTemperature, weatherForecastSamples31_trackTemperatureChange, weatherForecastSamples31_airTemperature, weatherForecastSamples31_airTemperatureChange, weatherForecastSamples31_rainPercentage,
                            weatherForecastSamples32_sessionType, weatherForecastSamples32_timeOffset, weatherForecastSamples32_weather, weatherForecastSamples32_trackTemperature, weatherForecastSamples32_trackTemperatureChange, weatherForecastSamples32_airTemperature, weatherForecastSamples32_airTemperatureChange, weatherForecastSamples32_rainPercentage,
                            weatherForecastSamples33_sessionType, weatherForecastSamples33_timeOffset, weatherForecastSamples33_weather, weatherForecastSamples33_trackTemperature, weatherForecastSamples33_trackTemperatureChange, weatherForecastSamples33_airTemperature, weatherForecastSamples33_airTemperatureChange, weatherForecastSamples33_rainPercentage,
                            weatherForecastSamples34_sessionType, weatherForecastSamples34_timeOffset, weatherForecastSamples34_weather, weatherForecastSamples34_trackTemperature, weatherForecastSamples34_trackTemperatureChange, weatherForecastSamples34_airTemperature, weatherForecastSamples34_airTemperatureChange, weatherForecastSamples34_rainPercentage,
                            weatherForecastSamples35_sessionType, weatherForecastSamples35_timeOffset, weatherForecastSamples35_weather, weatherForecastSamples35_trackTemperature, weatherForecastSamples35_trackTemperatureChange, weatherForecastSamples35_airTemperature, weatherForecastSamples35_airTemperatureChange, weatherForecastSamples35_rainPercentage,
                            weatherForecastSamples36_sessionType, weatherForecastSamples36_timeOffset, weatherForecastSamples36_weather, weatherForecastSamples36_trackTemperature, weatherForecastSamples36_trackTemperatureChange, weatherForecastSamples36_airTemperature, weatherForecastSamples36_airTemperatureChange, weatherForecastSamples36_rainPercentage,
                            weatherForecastSamples37_sessionType, weatherForecastSamples37_timeOffset, weatherForecastSamples37_weather, weatherForecastSamples37_trackTemperature, weatherForecastSamples37_trackTemperatureChange, weatherForecastSamples37_airTemperature, weatherForecastSamples37_airTemperatureChange, weatherForecastSamples37_rainPercentage,
                            weatherForecastSamples38_sessionType, weatherForecastSamples38_timeOffset, weatherForecastSamples38_weather, weatherForecastSamples38_trackTemperature, weatherForecastSamples38_trackTemperatureChange, weatherForecastSamples38_airTemperature, weatherForecastSamples38_airTemperatureChange, weatherForecastSamples38_rainPercentage,
                            weatherForecastSamples39_sessionType, weatherForecastSamples39_timeOffset, weatherForecastSamples39_weather, weatherForecastSamples39_trackTemperature, weatherForecastSamples39_trackTemperatureChange, weatherForecastSamples39_airTemperature, weatherForecastSamples39_airTemperatureChange, weatherForecastSamples39_rainPercentage,
                            weatherForecastSamples40_sessionType, weatherForecastSamples40_timeOffset, weatherForecastSamples40_weather, weatherForecastSamples40_trackTemperature, weatherForecastSamples40_trackTemperatureChange, weatherForecastSamples40_airTemperature, weatherForecastSamples40_airTemperatureChange, weatherForecastSamples40_rainPercentage,
                            weatherForecastSamples41_sessionType, weatherForecastSamples41_timeOffset, weatherForecastSamples41_weather, weatherForecastSamples41_trackTemperature, weatherForecastSamples41_trackTemperatureChange, weatherForecastSamples41_airTemperature, weatherForecastSamples41_airTemperatureChange, weatherForecastSamples41_rainPercentage,
                            weatherForecastSamples42_sessionType, weatherForecastSamples42_timeOffset, weatherForecastSamples42_weather, weatherForecastSamples42_trackTemperature, weatherForecastSamples42_trackTemperatureChange, weatherForecastSamples42_airTemperature, weatherForecastSamples42_airTemperatureChange, weatherForecastSamples42_rainPercentage,
                            weatherForecastSamples43_sessionType, weatherForecastSamples43_timeOffset, weatherForecastSamples43_weather, weatherForecastSamples43_trackTemperature, weatherForecastSamples43_trackTemperatureChange, weatherForecastSamples43_airTemperature, weatherForecastSamples43_airTemperatureChange, weatherForecastSamples43_rainPercentage,
                            weatherForecastSamples44_sessionType, weatherForecastSamples44_timeOffset, weatherForecastSamples44_weather, weatherForecastSamples44_trackTemperature, weatherForecastSamples44_trackTemperatureChange, weatherForecastSamples44_airTemperature, weatherForecastSamples44_airTemperatureChange, weatherForecastSamples44_rainPercentage,
                            weatherForecastSamples45_sessionType, weatherForecastSamples45_timeOffset, weatherForecastSamples45_weather, weatherForecastSamples45_trackTemperature, weatherForecastSamples45_trackTemperatureChange, weatherForecastSamples45_airTemperature, weatherForecastSamples45_airTemperatureChange, weatherForecastSamples45_rainPercentage,
                            weatherForecastSamples46_sessionType, weatherForecastSamples46_timeOffset, weatherForecastSamples46_weather, weatherForecastSamples46_trackTemperature, weatherForecastSamples46_trackTemperatureChange, weatherForecastSamples46_airTemperature, weatherForecastSamples46_airTemperatureChange, weatherForecastSamples46_rainPercentage,
                            weatherForecastSamples47_sessionType, weatherForecastSamples47_timeOffset, weatherForecastSamples47_weather, weatherForecastSamples47_trackTemperature, weatherForecastSamples47_trackTemperatureChange, weatherForecastSamples47_airTemperature, weatherForecastSamples47_airTemperatureChange, weatherForecastSamples47_rainPercentage,
                            weatherForecastSamples48_sessionType, weatherForecastSamples48_timeOffset, weatherForecastSamples48_weather, weatherForecastSamples48_trackTemperature, weatherForecastSamples48_trackTemperatureChange, weatherForecastSamples48_airTemperature, weatherForecastSamples48_airTemperatureChange, weatherForecastSamples48_rainPercentage,
                            weatherForecastSamples49_sessionType, weatherForecastSamples49_timeOffset, weatherForecastSamples49_weather, weatherForecastSamples49_trackTemperature, weatherForecastSamples49_trackTemperatureChange, weatherForecastSamples49_airTemperature, weatherForecastSamples49_airTemperatureChange, weatherForecastSamples49_rainPercentage,
                            weatherForecastSamples50_sessionType, weatherForecastSamples50_timeOffset, weatherForecastSamples50_weather, weatherForecastSamples50_trackTemperature, weatherForecastSamples50_trackTemperatureChange, weatherForecastSamples50_airTemperature, weatherForecastSamples50_airTemperatureChange, weatherForecastSamples50_rainPercentage,
                            weatherForecastSamples51_sessionType, weatherForecastSamples51_timeOffset, weatherForecastSamples51_weather, weatherForecastSamples51_trackTemperature, weatherForecastSamples51_trackTemperatureChange, weatherForecastSamples51_airTemperature, weatherForecastSamples51_airTemperatureChange, weatherForecastSamples51_rainPercentage,
                            weatherForecastSamples52_sessionType, weatherForecastSamples52_timeOffset, weatherForecastSamples52_weather, weatherForecastSamples52_trackTemperature, weatherForecastSamples52_trackTemperatureChange, weatherForecastSamples52_airTemperature, weatherForecastSamples52_airTemperatureChange, weatherForecastSamples52_rainPercentage,
                            weatherForecastSamples53_sessionType, weatherForecastSamples53_timeOffset, weatherForecastSamples53_weather, weatherForecastSamples53_trackTemperature, weatherForecastSamples53_trackTemperatureChange, weatherForecastSamples53_airTemperature, weatherForecastSamples53_airTemperatureChange, weatherForecastSamples53_rainPercentage,
                            weatherForecastSamples54_sessionType, weatherForecastSamples54_timeOffset, weatherForecastSamples54_weather, weatherForecastSamples54_trackTemperature, weatherForecastSamples54_trackTemperatureChange, weatherForecastSamples54_airTemperature, weatherForecastSamples54_airTemperatureChange, weatherForecastSamples54_rainPercentage,
                            weatherForecastSamples55_sessionType, weatherForecastSamples55_timeOffset, weatherForecastSamples55_weather, weatherForecastSamples55_trackTemperature, weatherForecastSamples55_trackTemperatureChange, weatherForecastSamples55_airTemperature, weatherForecastSamples55_airTemperatureChange, weatherForecastSamples55_rainPercentage,
                            weatherForecastSamples56_sessionType, weatherForecastSamples56_timeOffset, weatherForecastSamples56_weather, weatherForecastSamples56_trackTemperature, weatherForecastSamples56_trackTemperatureChange, weatherForecastSamples56_airTemperature, weatherForecastSamples56_airTemperatureChange, weatherForecastSamples56_rainPercentage,
                            weatherForecastSamples57_sessionType, weatherForecastSamples57_timeOffset, weatherForecastSamples57_weather, weatherForecastSamples57_trackTemperature, weatherForecastSamples57_trackTemperatureChange, weatherForecastSamples57_airTemperature, weatherForecastSamples57_airTemperatureChange, weatherForecastSamples57_rainPercentage,
                            weatherForecastSamples58_sessionType, weatherForecastSamples58_timeOffset, weatherForecastSamples58_weather, weatherForecastSamples58_trackTemperature, weatherForecastSamples58_trackTemperatureChange, weatherForecastSamples58_airTemperature, weatherForecastSamples58_airTemperatureChange, weatherForecastSamples58_rainPercentage,
                            weatherForecastSamples59_sessionType, weatherForecastSamples59_timeOffset, weatherForecastSamples59_weather, weatherForecastSamples59_trackTemperature, weatherForecastSamples59_trackTemperatureChange, weatherForecastSamples59_airTemperature, weatherForecastSamples59_airTemperatureChange, weatherForecastSamples59_rainPercentage,
                            weatherForecastSamples60_sessionType, weatherForecastSamples60_timeOffset, weatherForecastSamples60_weather, weatherForecastSamples60_trackTemperature, weatherForecastSamples60_trackTemperatureChange, weatherForecastSamples60_airTemperature, weatherForecastSamples60_airTemperatureChange, weatherForecastSamples60_rainPercentage,
                            weatherForecastSamples61_sessionType, weatherForecastSamples61_timeOffset, weatherForecastSamples61_weather, weatherForecastSamples61_trackTemperature, weatherForecastSamples61_trackTemperatureChange, weatherForecastSamples61_airTemperature, weatherForecastSamples61_airTemperatureChange, weatherForecastSamples61_rainPercentage,
                            weatherForecastSamples62_sessionType, weatherForecastSamples62_timeOffset, weatherForecastSamples62_weather, weatherForecastSamples62_trackTemperature, weatherForecastSamples62_trackTemperatureChange, weatherForecastSamples62_airTemperature, weatherForecastSamples62_airTemperatureChange, weatherForecastSamples62_rainPercentage,
                            weatherForecastSamples63_sessionType, weatherForecastSamples63_timeOffset, weatherForecastSamples63_weather, weatherForecastSamples63_trackTemperature, weatherForecastSamples63_trackTemperatureChange, weatherForecastSamples63_airTemperature, weatherForecastSamples63_airTemperatureChange, weatherForecastSamples63_rainPercentage,
                            forecastAccuracy, aiDifficulty, seasonLinkIdentifier, weekendLinkIdentifier, sessionLinkIdentifier, pitStopWindowIdealLap, pitStopWindowLatestLap, pitStopRejoinPosition, steeringAssist, brakingAssist, gearboxAssist, pitAssist,
                            pitReleaseAssist, ersAssist, drsAssist, dynamicRacingLine, dynamicRacingLineType, gameMode, ruleSet, timeOfDay, sessionLength, speedUnitsLeadPlayer, temperatureUnitsLeadPlayer, speedUnitsSecondaryPlayer, temperatureUnitsSecondaryPlayer,
                            numSafetyCarPeriods, numVirtualSafetyCarPeriods, numRedFlagPeriods, equalCarPerformance, recoveryMode, flashbackLimit, surfaceType, lowFuelMode, raceStarts, tyreTemperature, pitLaneTyreSim, carDamage, carDamageRate, collisions,
                            collisionsOffForFirstLapOnly, mpUnsafePitRelease, mpOffForGriefing, cornerCuttingStringency, parcFermeRules, pitStopExperience, safetyCar, safetyCarExperience, formationLap, formationLapExperience, redFlags, affectsLicenceLevelSolo,
                            affectsLicenceLevelMP, numSessionsInWeekend,
                            weekendStructure0,
                            weekendStructure1,
                            weekendStructure2,
                            weekendStructure3,
                            weekendStructure4,
                            weekendStructure5,
                            weekendStructure6,
                            weekendStructure7,
                            weekendStructure8,
                            weekendStructure9,
                            weekendStructure10,
                            weekendStructure11
                        ) VALUES (
                            @weather, @trackTemperature, @airTemperature, @totalLaps, @trackLength, @sessionType, @trackId, @formula, @sessionTimeLeft, @sessionDuration, @pitSpeedLimit, @gamePaused, @isSpectating, @spectatorCarIndex, @sliProNativeSupport,
                            @numMarshalZones,
                            @marshalZones0_zoneStart, @marshalZones0_zoneFlag,
                            @marshalZones1_zoneStart, @marshalZones1_zoneFlag,
                            @marshalZones2_zoneStart, @marshalZones2_zoneFlag,
                            @marshalZones3_zoneStart, @marshalZones3_zoneFlag,
                            @marshalZones4_zoneStart, @marshalZones4_zoneFlag,
                            @marshalZones5_zoneStart, @marshalZones5_zoneFlag,
                            @marshalZones6_zoneStart, @marshalZones6_zoneFlag,
                            @marshalZones7_zoneStart, @marshalZones7_zoneFlag,
                            @marshalZones8_zoneStart, @marshalZones8_zoneFlag,
                            @marshalZones9_zoneStart, @marshalZones9_zoneFlag,
                            @marshalZones10_zoneStart, @marshalZones10_zoneFlag,
                            @marshalZones11_zoneStart, @marshalZones11_zoneFlag,
                            @marshalZones12_zoneStart, @marshalZones12_zoneFlag,
                            @marshalZones13_zoneStart, @marshalZones13_zoneFlag,
                            @marshalZones14_zoneStart, @marshalZones14_zoneFlag,
                            @marshalZones15_zoneStart, @marshalZones15_zoneFlag,
                            @marshalZones16_zoneStart, @marshalZones16_zoneFlag,
                            @marshalZones17_zoneStart, @marshalZones17_zoneFlag,
                            @marshalZones18_zoneStart, @marshalZones18_zoneFlag,
                            @marshalZones19_zoneStart, @marshalZones19_zoneFlag,
                            @marshalZones20_zoneStart, @marshalZones20_zoneFlag,
                            @safetyCarStatus, @networkGame, @numWeatherForecastSamples,
                            @weatherForecastSamples0_sessionType, @weatherForecastSamples0_timeOffset, @weatherForecastSamples0_weather, @weatherForecastSamples0_trackTemperature, @weatherForecastSamples0_trackTemperatureChange, @weatherForecastSamples0_airTemperature, @weatherForecastSamples0_airTemperatureChange, @weatherForecastSamples0_rainPercentage,
                            @weatherForecastSamples1_sessionType, @weatherForecastSamples1_timeOffset, @weatherForecastSamples1_weather, @weatherForecastSamples1_trackTemperature, @weatherForecastSamples1_trackTemperatureChange, @weatherForecastSamples1_airTemperature, @weatherForecastSamples1_airTemperatureChange, @weatherForecastSamples1_rainPercentage,
                            @weatherForecastSamples2_sessionType, @weatherForecastSamples2_timeOffset, @weatherForecastSamples2_weather, @weatherForecastSamples2_trackTemperature, @weatherForecastSamples2_trackTemperatureChange, @weatherForecastSamples2_airTemperature, @weatherForecastSamples2_airTemperatureChange, @weatherForecastSamples2_rainPercentage,
                            @weatherForecastSamples3_sessionType, @weatherForecastSamples3_timeOffset, @weatherForecastSamples3_weather, @weatherForecastSamples3_trackTemperature, @weatherForecastSamples3_trackTemperatureChange, @weatherForecastSamples3_airTemperature, @weatherForecastSamples3_airTemperatureChange, @weatherForecastSamples3_rainPercentage,
                            @weatherForecastSamples4_sessionType, @weatherForecastSamples4_timeOffset, @weatherForecastSamples4_weather, @weatherForecastSamples4_trackTemperature, @weatherForecastSamples4_trackTemperatureChange, @weatherForecastSamples4_airTemperature, @weatherForecastSamples4_airTemperatureChange, @weatherForecastSamples4_rainPercentage,
                            @weatherForecastSamples5_sessionType, @weatherForecastSamples5_timeOffset, @weatherForecastSamples5_weather, @weatherForecastSamples5_trackTemperature, @weatherForecastSamples5_trackTemperatureChange, @weatherForecastSamples5_airTemperature, @weatherForecastSamples5_airTemperatureChange, @weatherForecastSamples5_rainPercentage,
                            @weatherForecastSamples6_sessionType, @weatherForecastSamples6_timeOffset, @weatherForecastSamples6_weather, @weatherForecastSamples6_trackTemperature, @weatherForecastSamples6_trackTemperatureChange, @weatherForecastSamples6_airTemperature, @weatherForecastSamples6_airTemperatureChange, @weatherForecastSamples6_rainPercentage,
                            @weatherForecastSamples7_sessionType, @weatherForecastSamples7_timeOffset, @weatherForecastSamples7_weather, @weatherForecastSamples7_trackTemperature, @weatherForecastSamples7_trackTemperatureChange, @weatherForecastSamples7_airTemperature, @weatherForecastSamples7_airTemperatureChange, @weatherForecastSamples7_rainPercentage,
                            @weatherForecastSamples8_sessionType, @weatherForecastSamples8_timeOffset, @weatherForecastSamples8_weather, @weatherForecastSamples8_trackTemperature, @weatherForecastSamples8_trackTemperatureChange, @weatherForecastSamples8_airTemperature, @weatherForecastSamples8_airTemperatureChange, @weatherForecastSamples8_rainPercentage,
                            @weatherForecastSamples9_sessionType, @weatherForecastSamples9_timeOffset, @weatherForecastSamples9_weather, @weatherForecastSamples9_trackTemperature, @weatherForecastSamples9_trackTemperatureChange, @weatherForecastSamples9_airTemperature, @weatherForecastSamples9_airTemperatureChange, @weatherForecastSamples9_rainPercentage,
                            @weatherForecastSamples10_sessionType, @weatherForecastSamples10_timeOffset, @weatherForecastSamples10_weather, @weatherForecastSamples10_trackTemperature, @weatherForecastSamples10_trackTemperatureChange, @weatherForecastSamples10_airTemperature, @weatherForecastSamples10_airTemperatureChange, @weatherForecastSamples10_rainPercentage,
                            @weatherForecastSamples11_sessionType, @weatherForecastSamples11_timeOffset, @weatherForecastSamples11_weather, @weatherForecastSamples11_trackTemperature, @weatherForecastSamples11_trackTemperatureChange, @weatherForecastSamples11_airTemperature, @weatherForecastSamples11_airTemperatureChange, @weatherForecastSamples11_rainPercentage,
                            @weatherForecastSamples12_sessionType, @weatherForecastSamples12_timeOffset, @weatherForecastSamples12_weather, @weatherForecastSamples12_trackTemperature, @weatherForecastSamples12_trackTemperatureChange, @weatherForecastSamples12_airTemperature, @weatherForecastSamples12_airTemperatureChange, @weatherForecastSamples12_rainPercentage,
                            @weatherForecastSamples13_sessionType, @weatherForecastSamples13_timeOffset, @weatherForecastSamples13_weather, @weatherForecastSamples13_trackTemperature, @weatherForecastSamples13_trackTemperatureChange, @weatherForecastSamples13_airTemperature, @weatherForecastSamples13_airTemperatureChange, @weatherForecastSamples13_rainPercentage,
                            @weatherForecastSamples14_sessionType, @weatherForecastSamples14_timeOffset, @weatherForecastSamples14_weather, @weatherForecastSamples14_trackTemperature, @weatherForecastSamples14_trackTemperatureChange, @weatherForecastSamples14_airTemperature, @weatherForecastSamples14_airTemperatureChange, @weatherForecastSamples14_rainPercentage,
                            @weatherForecastSamples15_sessionType, @weatherForecastSamples15_timeOffset, @weatherForecastSamples15_weather, @weatherForecastSamples15_trackTemperature, @weatherForecastSamples15_trackTemperatureChange, @weatherForecastSamples15_airTemperature, @weatherForecastSamples15_airTemperatureChange, @weatherForecastSamples15_rainPercentage,
                            @weatherForecastSamples16_sessionType, @weatherForecastSamples16_timeOffset, @weatherForecastSamples16_weather, @weatherForecastSamples16_trackTemperature, @weatherForecastSamples16_trackTemperatureChange, @weatherForecastSamples16_airTemperature, @weatherForecastSamples16_airTemperatureChange, @weatherForecastSamples16_rainPercentage,
                            @weatherForecastSamples17_sessionType, @weatherForecastSamples17_timeOffset, @weatherForecastSamples17_weather, @weatherForecastSamples17_trackTemperature, @weatherForecastSamples17_trackTemperatureChange, @weatherForecastSamples17_airTemperature, @weatherForecastSamples17_airTemperatureChange, @weatherForecastSamples17_rainPercentage,
                            @weatherForecastSamples18_sessionType, @weatherForecastSamples18_timeOffset, @weatherForecastSamples18_weather, @weatherForecastSamples18_trackTemperature, @weatherForecastSamples18_trackTemperatureChange, @weatherForecastSamples18_airTemperature, @weatherForecastSamples18_airTemperatureChange, @weatherForecastSamples18_rainPercentage,
                            @weatherForecastSamples19_sessionType, @weatherForecastSamples19_timeOffset, @weatherForecastSamples19_weather, @weatherForecastSamples19_trackTemperature, @weatherForecastSamples19_trackTemperatureChange, @weatherForecastSamples19_airTemperature, @weatherForecastSamples19_airTemperatureChange, @weatherForecastSamples19_rainPercentage,
                            @weatherForecastSamples20_sessionType, @weatherForecastSamples20_timeOffset, @weatherForecastSamples20_weather, @weatherForecastSamples20_trackTemperature, @weatherForecastSamples20_trackTemperatureChange, @weatherForecastSamples20_airTemperature, @weatherForecastSamples20_airTemperatureChange, @weatherForecastSamples20_rainPercentage,
                            @weatherForecastSamples21_sessionType, @weatherForecastSamples21_timeOffset, @weatherForecastSamples21_weather, @weatherForecastSamples21_trackTemperature, @weatherForecastSamples21_trackTemperatureChange, @weatherForecastSamples21_airTemperature, @weatherForecastSamples21_airTemperatureChange, @weatherForecastSamples21_rainPercentage,
                            @weatherForecastSamples22_sessionType, @weatherForecastSamples22_timeOffset, @weatherForecastSamples22_weather, @weatherForecastSamples22_trackTemperature, @weatherForecastSamples22_trackTemperatureChange, @weatherForecastSamples22_airTemperature, @weatherForecastSamples22_airTemperatureChange, @weatherForecastSamples22_rainPercentage,
                            @weatherForecastSamples23_sessionType, @weatherForecastSamples23_timeOffset, @weatherForecastSamples23_weather, @weatherForecastSamples23_trackTemperature, @weatherForecastSamples23_trackTemperatureChange, @weatherForecastSamples23_airTemperature, @weatherForecastSamples23_airTemperatureChange, @weatherForecastSamples23_rainPercentage,
                            @weatherForecastSamples24_sessionType, @weatherForecastSamples24_timeOffset, @weatherForecastSamples24_weather, @weatherForecastSamples24_trackTemperature, @weatherForecastSamples24_trackTemperatureChange, @weatherForecastSamples24_airTemperature, @weatherForecastSamples24_airTemperatureChange, @weatherForecastSamples24_rainPercentage,
                            @weatherForecastSamples25_sessionType, @weatherForecastSamples25_timeOffset, @weatherForecastSamples25_weather, @weatherForecastSamples25_trackTemperature, @weatherForecastSamples25_trackTemperatureChange, @weatherForecastSamples25_airTemperature, @weatherForecastSamples25_airTemperatureChange, @weatherForecastSamples25_rainPercentage,
                            @weatherForecastSamples26_sessionType, @weatherForecastSamples26_timeOffset, @weatherForecastSamples26_weather, @weatherForecastSamples26_trackTemperature, @weatherForecastSamples26_trackTemperatureChange, @weatherForecastSamples26_airTemperature, @weatherForecastSamples26_airTemperatureChange, @weatherForecastSamples26_rainPercentage,
                            @weatherForecastSamples27_sessionType, @weatherForecastSamples27_timeOffset, @weatherForecastSamples27_weather, @weatherForecastSamples27_trackTemperature, @weatherForecastSamples27_trackTemperatureChange, @weatherForecastSamples27_airTemperature, @weatherForecastSamples27_airTemperatureChange, @weatherForecastSamples27_rainPercentage,
                            @weatherForecastSamples28_sessionType, @weatherForecastSamples28_timeOffset, @weatherForecastSamples28_weather, @weatherForecastSamples28_trackTemperature, @weatherForecastSamples28_trackTemperatureChange, @weatherForecastSamples28_airTemperature, @weatherForecastSamples28_airTemperatureChange, @weatherForecastSamples28_rainPercentage,
                            @weatherForecastSamples29_sessionType, @weatherForecastSamples29_timeOffset, @weatherForecastSamples29_weather, @weatherForecastSamples29_trackTemperature, @weatherForecastSamples29_trackTemperatureChange, @weatherForecastSamples29_airTemperature, @weatherForecastSamples29_airTemperatureChange, @weatherForecastSamples29_rainPercentage,
                            @weatherForecastSamples30_sessionType, @weatherForecastSamples30_timeOffset, @weatherForecastSamples30_weather, @weatherForecastSamples30_trackTemperature, @weatherForecastSamples30_trackTemperatureChange, @weatherForecastSamples30_airTemperature, @weatherForecastSamples30_airTemperatureChange, @weatherForecastSamples30_rainPercentage,
                            @weatherForecastSamples31_sessionType, @weatherForecastSamples31_timeOffset, @weatherForecastSamples31_weather, @weatherForecastSamples31_trackTemperature, @weatherForecastSamples31_trackTemperatureChange, @weatherForecastSamples31_airTemperature, @weatherForecastSamples31_airTemperatureChange, @weatherForecastSamples31_rainPercentage,
                            @weatherForecastSamples32_sessionType, @weatherForecastSamples32_timeOffset, @weatherForecastSamples32_weather, @weatherForecastSamples32_trackTemperature, @weatherForecastSamples32_trackTemperatureChange, @weatherForecastSamples32_airTemperature, @weatherForecastSamples32_airTemperatureChange, @weatherForecastSamples32_rainPercentage,
                            @weatherForecastSamples33_sessionType, @weatherForecastSamples33_timeOffset, @weatherForecastSamples33_weather, @weatherForecastSamples33_trackTemperature, @weatherForecastSamples33_trackTemperatureChange, @weatherForecastSamples33_airTemperature, @weatherForecastSamples33_airTemperatureChange, @weatherForecastSamples33_rainPercentage,
                            @weatherForecastSamples34_sessionType, @weatherForecastSamples34_timeOffset, @weatherForecastSamples34_weather, @weatherForecastSamples34_trackTemperature, @weatherForecastSamples34_trackTemperatureChange, @weatherForecastSamples34_airTemperature, @weatherForecastSamples34_airTemperatureChange, @weatherForecastSamples34_rainPercentage,
                            @weatherForecastSamples35_sessionType, @weatherForecastSamples35_timeOffset, @weatherForecastSamples35_weather, @weatherForecastSamples35_trackTemperature, @weatherForecastSamples35_trackTemperatureChange, @weatherForecastSamples35_airTemperature, @weatherForecastSamples35_airTemperatureChange, @weatherForecastSamples35_rainPercentage,
                            @weatherForecastSamples36_sessionType, @weatherForecastSamples36_timeOffset, @weatherForecastSamples36_weather, @weatherForecastSamples36_trackTemperature, @weatherForecastSamples36_trackTemperatureChange, @weatherForecastSamples36_airTemperature, @weatherForecastSamples36_airTemperatureChange, @weatherForecastSamples36_rainPercentage,
                            @weatherForecastSamples37_sessionType, @weatherForecastSamples37_timeOffset, @weatherForecastSamples37_weather, @weatherForecastSamples37_trackTemperature, @weatherForecastSamples37_trackTemperatureChange, @weatherForecastSamples37_airTemperature, @weatherForecastSamples37_airTemperatureChange, @weatherForecastSamples37_rainPercentage,
                            @weatherForecastSamples38_sessionType, @weatherForecastSamples38_timeOffset, @weatherForecastSamples38_weather, @weatherForecastSamples38_trackTemperature, @weatherForecastSamples38_trackTemperatureChange, @weatherForecastSamples38_airTemperature, @weatherForecastSamples38_airTemperatureChange, @weatherForecastSamples38_rainPercentage,
                            @weatherForecastSamples39_sessionType, @weatherForecastSamples39_timeOffset, @weatherForecastSamples39_weather, @weatherForecastSamples39_trackTemperature, @weatherForecastSamples39_trackTemperatureChange, @weatherForecastSamples39_airTemperature, @weatherForecastSamples39_airTemperatureChange, @weatherForecastSamples39_rainPercentage,
                            @weatherForecastSamples40_sessionType, @weatherForecastSamples40_timeOffset, @weatherForecastSamples40_weather, @weatherForecastSamples40_trackTemperature, @weatherForecastSamples40_trackTemperatureChange, @weatherForecastSamples40_airTemperature, @weatherForecastSamples40_airTemperatureChange, @weatherForecastSamples40_rainPercentage,
                            @weatherForecastSamples41_sessionType, @weatherForecastSamples41_timeOffset, @weatherForecastSamples41_weather, @weatherForecastSamples41_trackTemperature, @weatherForecastSamples41_trackTemperatureChange, @weatherForecastSamples41_airTemperature, @weatherForecastSamples41_airTemperatureChange, @weatherForecastSamples41_rainPercentage,
                            @weatherForecastSamples42_sessionType, @weatherForecastSamples42_timeOffset, @weatherForecastSamples42_weather, @weatherForecastSamples42_trackTemperature, @weatherForecastSamples42_trackTemperatureChange, @weatherForecastSamples42_airTemperature, @weatherForecastSamples42_airTemperatureChange, @weatherForecastSamples42_rainPercentage,
                            @weatherForecastSamples43_sessionType, @weatherForecastSamples43_timeOffset, @weatherForecastSamples43_weather, @weatherForecastSamples43_trackTemperature, @weatherForecastSamples43_trackTemperatureChange, @weatherForecastSamples43_airTemperature, @weatherForecastSamples43_airTemperatureChange, @weatherForecastSamples43_rainPercentage,
                            @weatherForecastSamples44_sessionType, @weatherForecastSamples44_timeOffset, @weatherForecastSamples44_weather, @weatherForecastSamples44_trackTemperature, @weatherForecastSamples44_trackTemperatureChange, @weatherForecastSamples44_airTemperature, @weatherForecastSamples44_airTemperatureChange, @weatherForecastSamples44_rainPercentage,
                            @weatherForecastSamples45_sessionType, @weatherForecastSamples45_timeOffset, @weatherForecastSamples45_weather, @weatherForecastSamples45_trackTemperature, @weatherForecastSamples45_trackTemperatureChange, @weatherForecastSamples45_airTemperature, @weatherForecastSamples45_airTemperatureChange, @weatherForecastSamples45_rainPercentage,
                            @weatherForecastSamples46_sessionType, @weatherForecastSamples46_timeOffset, @weatherForecastSamples46_weather, @weatherForecastSamples46_trackTemperature, @weatherForecastSamples46_trackTemperatureChange, @weatherForecastSamples46_airTemperature, @weatherForecastSamples46_airTemperatureChange, @weatherForecastSamples46_rainPercentage,
                            @weatherForecastSamples47_sessionType, @weatherForecastSamples47_timeOffset, @weatherForecastSamples47_weather, @weatherForecastSamples47_trackTemperature, @weatherForecastSamples47_trackTemperatureChange, @weatherForecastSamples47_airTemperature, @weatherForecastSamples47_airTemperatureChange, @weatherForecastSamples47_rainPercentage,
                            @weatherForecastSamples48_sessionType, @weatherForecastSamples48_timeOffset, @weatherForecastSamples48_weather, @weatherForecastSamples48_trackTemperature, @weatherForecastSamples48_trackTemperatureChange, @weatherForecastSamples48_airTemperature, @weatherForecastSamples48_airTemperatureChange, @weatherForecastSamples48_rainPercentage,
                            @weatherForecastSamples49_sessionType, @weatherForecastSamples49_timeOffset, @weatherForecastSamples49_weather, @weatherForecastSamples49_trackTemperature, @weatherForecastSamples49_trackTemperatureChange, @weatherForecastSamples49_airTemperature, @weatherForecastSamples49_airTemperatureChange, @weatherForecastSamples49_rainPercentage,
                            @weatherForecastSamples50_sessionType, @weatherForecastSamples50_timeOffset, @weatherForecastSamples50_weather, @weatherForecastSamples50_trackTemperature, @weatherForecastSamples50_trackTemperatureChange, @weatherForecastSamples50_airTemperature, @weatherForecastSamples50_airTemperatureChange, @weatherForecastSamples50_rainPercentage,
                            @weatherForecastSamples51_sessionType, @weatherForecastSamples51_timeOffset, @weatherForecastSamples51_weather, @weatherForecastSamples51_trackTemperature, @weatherForecastSamples51_trackTemperatureChange, @weatherForecastSamples51_airTemperature, @weatherForecastSamples51_airTemperatureChange, @weatherForecastSamples51_rainPercentage,
                            @weatherForecastSamples52_sessionType, @weatherForecastSamples52_timeOffset, @weatherForecastSamples52_weather, @weatherForecastSamples52_trackTemperature, @weatherForecastSamples52_trackTemperatureChange, @weatherForecastSamples52_airTemperature, @weatherForecastSamples52_airTemperatureChange, @weatherForecastSamples52_rainPercentage,
                            @weatherForecastSamples53_sessionType, @weatherForecastSamples53_timeOffset, @weatherForecastSamples53_weather, @weatherForecastSamples53_trackTemperature, @weatherForecastSamples53_trackTemperatureChange, @weatherForecastSamples53_airTemperature, @weatherForecastSamples53_airTemperatureChange, @weatherForecastSamples53_rainPercentage,
                            @weatherForecastSamples54_sessionType, @weatherForecastSamples54_timeOffset, @weatherForecastSamples54_weather, @weatherForecastSamples54_trackTemperature, @weatherForecastSamples54_trackTemperatureChange, @weatherForecastSamples54_airTemperature, @weatherForecastSamples54_airTemperatureChange, @weatherForecastSamples54_rainPercentage,
                            @weatherForecastSamples55_sessionType, @weatherForecastSamples55_timeOffset, @weatherForecastSamples55_weather, @weatherForecastSamples55_trackTemperature, @weatherForecastSamples55_trackTemperatureChange, @weatherForecastSamples55_airTemperature, @weatherForecastSamples55_airTemperatureChange, @weatherForecastSamples55_rainPercentage,
                            @weatherForecastSamples56_sessionType, @weatherForecastSamples56_timeOffset, @weatherForecastSamples56_weather, @weatherForecastSamples56_trackTemperature, @weatherForecastSamples56_trackTemperatureChange, @weatherForecastSamples56_airTemperature, @weatherForecastSamples56_airTemperatureChange, @weatherForecastSamples56_rainPercentage,
                            @weatherForecastSamples57_sessionType, @weatherForecastSamples57_timeOffset, @weatherForecastSamples57_weather, @weatherForecastSamples57_trackTemperature, @weatherForecastSamples57_trackTemperatureChange, @weatherForecastSamples57_airTemperature, @weatherForecastSamples57_airTemperatureChange, @weatherForecastSamples57_rainPercentage,
                            @weatherForecastSamples58_sessionType, @weatherForecastSamples58_timeOffset, @weatherForecastSamples58_weather, @weatherForecastSamples58_trackTemperature, @weatherForecastSamples58_trackTemperatureChange, @weatherForecastSamples58_airTemperature, @weatherForecastSamples58_airTemperatureChange, @weatherForecastSamples58_rainPercentage,
                            @weatherForecastSamples59_sessionType, @weatherForecastSamples59_timeOffset, @weatherForecastSamples59_weather, @weatherForecastSamples59_trackTemperature, @weatherForecastSamples59_trackTemperatureChange, @weatherForecastSamples59_airTemperature, @weatherForecastSamples59_airTemperatureChange, @weatherForecastSamples59_rainPercentage,
                            @weatherForecastSamples60_sessionType, @weatherForecastSamples60_timeOffset, @weatherForecastSamples60_weather, @weatherForecastSamples60_trackTemperature, @weatherForecastSamples60_trackTemperatureChange, @weatherForecastSamples60_airTemperature, @weatherForecastSamples60_airTemperatureChange, @weatherForecastSamples60_rainPercentage,
                            @weatherForecastSamples61_sessionType, @weatherForecastSamples61_timeOffset, @weatherForecastSamples61_weather, @weatherForecastSamples61_trackTemperature, @weatherForecastSamples61_trackTemperatureChange, @weatherForecastSamples61_airTemperature, @weatherForecastSamples61_airTemperatureChange, @weatherForecastSamples61_rainPercentage,
                            @weatherForecastSamples62_sessionType, @weatherForecastSamples62_timeOffset, @weatherForecastSamples62_weather, @weatherForecastSamples62_trackTemperature, @weatherForecastSamples62_trackTemperatureChange, @weatherForecastSamples62_airTemperature, @weatherForecastSamples62_airTemperatureChange, @weatherForecastSamples62_rainPercentage,
                            @weatherForecastSamples63_sessionType, @weatherForecastSamples63_timeOffset, @weatherForecastSamples63_weather, @weatherForecastSamples63_trackTemperature, @weatherForecastSamples63_trackTemperatureChange, @weatherForecastSamples63_airTemperature, @weatherForecastSamples63_airTemperatureChange, @weatherForecastSamples63_rainPercentage,
                            @forecastAccuracy,
                            @aiDifficulty, @seasonLinkIdentifier, @weekendLinkIdentifier, @sessionLinkIdentifier, @pitStopWindowIdealLap, @pitStopWindowLatestLap, @pitStopRejoinPosition, @steeringAssist, @brakingAssist, @gearboxAssist, @pitAssist, @pitReleaseAssist,
                            @ersAssist, @drsAssist, @dynamicRacingLine, @dynamicRacingLineType, @gameMode, @ruleSet, @timeOfDay, @sessionLength, @speedUnitsLeadPlayer, @temperatureUnitsLeadPlayer, @speedUnitsSecondaryPlayer, @temperatureUnitsSecondaryPlayer,
                            @numSafetyCarPeriods, @numVirtualSafetyCarPeriods, @numRedFlagPeriods, @equalCarPerformance, @recoveryMode, @flashbackLimit, @surfaceType, @lowFuelMode, @raceStarts, @tyreTemperature, @pitLaneTyreSim, @carDamage, @carDamageRate,
                            @collisions, @collisionsOffForFirstLapOnly, @mpUnsafePitRelease, @mpOffForGriefing, @cornerCuttingStringency, @parcFermeRules, @pitStopExperience, @safetyCar, @safetyCarExperience, @formationLap, @formationLapExperience, @redFlags,
                            @affectsLicenceLevelSolo, @affectsLicenceLevelMP, @numSessionsInWeekend,
                            @weekendStructure0,
                            @weekendStructure1,
                            @weekendStructure2,
                            @weekendStructure3,
                            @weekendStructure4,
                            @weekendStructure5,
                            @weekendStructure6,
                            @weekendStructure7,
                            @weekendStructure8,
                            @weekendStructure9,
                            @weekendStructure10,
                            @weekendStructure11
                            )", conn
                    );
                    cmd.Parameters.AddWithValue("@weather", dataPacketSession.weather);
                    cmd.Parameters.AddWithValue("@trackTemperature", dataPacketSession.trackTemperature);
                    cmd.Parameters.AddWithValue("@airTemperature", dataPacketSession.airTemperature);
                    cmd.Parameters.AddWithValue("@totalLaps", dataPacketSession.totalLaps);
                    cmd.Parameters.AddWithValue("@trackLength", dataPacketSession.trackLength);
                    cmd.Parameters.AddWithValue("@sessionType", dataPacketSession.sessionType);
                    cmd.Parameters.AddWithValue("@trackId", dataPacketSession.trackId);
                    cmd.Parameters.AddWithValue("@formula", dataPacketSession.formula);
                    cmd.Parameters.AddWithValue("@sessionTimeLeft", dataPacketSession.sessionTimeLeft);
                    cmd.Parameters.AddWithValue("@sessionDuration", dataPacketSession.sessionDuration);
                    cmd.Parameters.AddWithValue("@pitSpeedLimit", dataPacketSession.pitSpeedLimit);
                    cmd.Parameters.AddWithValue("@gamePaused", dataPacketSession.gamePaused);
                    cmd.Parameters.AddWithValue("@isSpectating", dataPacketSession.isSpectating);
                    cmd.Parameters.AddWithValue("@spectatorCarIndex", dataPacketSession.spectatorCarIndex);
                    cmd.Parameters.AddWithValue("@sliProNativeSupport", dataPacketSession.sliProNativeSupport);
                    cmd.Parameters.AddWithValue("@numMarshalZones", dataPacketSession.numMarshalZones);
                    for (int i = 0; i < 21; i++)
                    {
                        cmd.Parameters.AddWithValue($"@marshalZones{i}_zoneStart", dataPacketSession.marshalZones[i].zoneStart);
                        cmd.Parameters.AddWithValue($"@marshalZones{i}_zoneFlag", dataPacketSession.marshalZones[i].zoneFlag);
                    }
                    cmd.Parameters.AddWithValue("@safetyCarStatus", dataPacketSession.safetyCarStatus);
                    cmd.Parameters.AddWithValue("@networkGame", dataPacketSession.networkGame);
                    cmd.Parameters.AddWithValue("@numWeatherForecastSamples", dataPacketSession.numWeatherForecastSamples);
                    for (int i = 0; i < 64; i++)
                    {
                        cmd.Parameters.AddWithValue($"@weatherForecastSamples{i}_sessionType", dataPacketSession.weatherForecastSamples[i].sessionType);
                        cmd.Parameters.AddWithValue($"@weatherForecastSamples{i}_timeOffset", dataPacketSession.weatherForecastSamples[i].timeOffset);
                        cmd.Parameters.AddWithValue($"@weatherForecastSamples{i}_weather", dataPacketSession.weatherForecastSamples[i].weather);
                        cmd.Parameters.AddWithValue($"@weatherForecastSamples{i}_trackTemperature", dataPacketSession.weatherForecastSamples[i].trackTemperature);
                        cmd.Parameters.AddWithValue($"@weatherForecastSamples{i}_trackTemperatureChange", dataPacketSession.weatherForecastSamples[i].trackTemperatureChange);
                        cmd.Parameters.AddWithValue($"@weatherForecastSamples{i}_airTemperature", dataPacketSession.weatherForecastSamples[i].airTemperature);
                        cmd.Parameters.AddWithValue($"@weatherForecastSamples{i}_airTemperatureChange", dataPacketSession.weatherForecastSamples[i].airTemperatureChange);
                        cmd.Parameters.AddWithValue($"@weatherForecastSamples{i}_rainPercentage", dataPacketSession.weatherForecastSamples[i].rainPercentage);
                    }
                    cmd.Parameters.AddWithValue("@forecastAccuracy", dataPacketSession.forecastAccuracy);
                    cmd.Parameters.AddWithValue("@aiDifficulty", dataPacketSession.aiDifficulty);
                    cmd.Parameters.AddWithValue("@seasonLinkIdentifier", dataPacketSession.seasonLinkIdentifier);
                    cmd.Parameters.AddWithValue("@weekendLinkIdentifier", dataPacketSession.weekendLinkIdentifier);
                    cmd.Parameters.AddWithValue("@sessionLinkIdentifier", dataPacketSession.sessionLinkIdentifier);
                    cmd.Parameters.AddWithValue("@pitStopWindowIdealLap", dataPacketSession.pitStopWindowIdealLap);
                    cmd.Parameters.AddWithValue("@pitStopWindowLatestLap", dataPacketSession.pitStopWindowLatestLap);
                    cmd.Parameters.AddWithValue("@pitStopRejoinPosition", dataPacketSession.pitStopRejoinPosition);
                    cmd.Parameters.AddWithValue("@steeringAssist", dataPacketSession.steeringAssist);
                    cmd.Parameters.AddWithValue("@brakingAssist", dataPacketSession.brakingAssist);
                    cmd.Parameters.AddWithValue("@gearboxAssist", dataPacketSession.gearboxAssist);
                    cmd.Parameters.AddWithValue("@pitAssist", dataPacketSession.pitAssist);
                    cmd.Parameters.AddWithValue("@pitReleaseAssist", dataPacketSession.pitReleaseAssist);
                    cmd.Parameters.AddWithValue("@ERSAssist", dataPacketSession.ERSAssist);
                    cmd.Parameters.AddWithValue("@DRSAssist", dataPacketSession.DRSAssist);
                    cmd.Parameters.AddWithValue("@dynamicRacingLine", dataPacketSession.dynamicRacingLine);
                    cmd.Parameters.AddWithValue("@dynamicRacingLineType", dataPacketSession.dynamicRacingLineType);
                    cmd.Parameters.AddWithValue("@gameMode", dataPacketSession.gameMode);
                    cmd.Parameters.AddWithValue("@ruleSet", dataPacketSession.ruleSet);
                    cmd.Parameters.AddWithValue("@timeOfDay", dataPacketSession.timeOfDay);
                    cmd.Parameters.AddWithValue("@sessionLength", dataPacketSession.sessionLength);
                    cmd.Parameters.AddWithValue("@speedUnitsLeadPlayer", dataPacketSession.speedUnitsLeadPlayer);
                    cmd.Parameters.AddWithValue("@temperatureUnitsLeadPlayer", dataPacketSession.temperatureUnitsLeadPlayer);
                    cmd.Parameters.AddWithValue("@speedUnitsSecondaryPlayer", dataPacketSession.speedUnitsSecondaryPlayer);
                    cmd.Parameters.AddWithValue("@temperatureUnitsSecondaryPlayer", dataPacketSession.temperatureUnitsSecondaryPlayer);
                    cmd.Parameters.AddWithValue("@numSafetyCarPeriods", dataPacketSession.numSafetyCarPeriods);
                    cmd.Parameters.AddWithValue("@numVirtualSafetyCarPeriods", dataPacketSession.numVirtualSafetyCarPeriods);
                    cmd.Parameters.AddWithValue("@numRedFlagPeriods", dataPacketSession.numRedFlagPeriods);
                    cmd.Parameters.AddWithValue("@equalCarPerformance", dataPacketSession.equalCarPerformance);
                    cmd.Parameters.AddWithValue("@recoveryMode", dataPacketSession.recoveryMode);
                    cmd.Parameters.AddWithValue("@flashbackLimit", dataPacketSession.flashbackLimit);
                    cmd.Parameters.AddWithValue("@surfaceType", dataPacketSession.surfaceType);
                    cmd.Parameters.AddWithValue("@lowFuelMode", dataPacketSession.lowFuelMode);
                    cmd.Parameters.AddWithValue("@raceStarts", dataPacketSession.raceStarts);
                    cmd.Parameters.AddWithValue("@tyreTemperature", dataPacketSession.tyreTemperature);
                    cmd.Parameters.AddWithValue("@pitLaneTyreSim", dataPacketSession.pitLaneTyreSim);
                    cmd.Parameters.AddWithValue("@carDamage", dataPacketSession.carDamage);
                    cmd.Parameters.AddWithValue("@carDamageRate", dataPacketSession.carDamageRate);
                    cmd.Parameters.AddWithValue("@collisions", dataPacketSession.collisions);
                    cmd.Parameters.AddWithValue("@collisionsOffForFirstLapOnly", dataPacketSession.collisionsOffForFirstLapOnly);
                    cmd.Parameters.AddWithValue("@mpUnsafePitRelease", dataPacketSession.mpUnsafePitRelease);
                    cmd.Parameters.AddWithValue("@mpOffForGriefing", dataPacketSession.mpOffForGriefing);
                    cmd.Parameters.AddWithValue("@cornerCuttingStringency", dataPacketSession.cornerCuttingStringency);
                    cmd.Parameters.AddWithValue("@parcFermeRules", dataPacketSession.parcFermeRules);
                    cmd.Parameters.AddWithValue("@pitStopExperience", dataPacketSession.pitStopExperience);
                    cmd.Parameters.AddWithValue("@safetyCar", dataPacketSession.safetyCar);
                    cmd.Parameters.AddWithValue("@safetyCarExperience", dataPacketSession.safetyCarExperience);
                    cmd.Parameters.AddWithValue("@formationLap", dataPacketSession.formationLap);
                    cmd.Parameters.AddWithValue("@formationLapExperience", dataPacketSession.formationLapExperience);
                    cmd.Parameters.AddWithValue("@redFlags", dataPacketSession.redFlags);
                    cmd.Parameters.AddWithValue("@affectsLicenceLevelSolo", dataPacketSession.affectsLicenceLevelSolo);
                    cmd.Parameters.AddWithValue("@affectsLicenceLevelMP", dataPacketSession.affectsLicenceLevelMP);
                    cmd.Parameters.AddWithValue("@numSessionsInWeekend", dataPacketSession.numSessionsInWeekend);
                    for (int i = 0; i < 12; i++)
                    {
                        cmd.Parameters.AddWithValue($"@weekendStructure{i}", dataPacketSession.weekendStructure[i]);
                    }
                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"telemetrySessionData sql: {ex}");
                        continue;
                    }
                    continue;
                }
            }
        }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------Readers
        private CarTelemetryData ReadCarTelemetryData(BinaryReader br)
        {
            CarTelemetryData carTelemetryData = new CarTelemetryData();
            try
            {
                carTelemetryData.speed = br.ReadUInt16();
                carTelemetryData.throttle = br.ReadSingle();
                carTelemetryData.steer = br.ReadSingle();
                carTelemetryData.brake = br.ReadSingle();
                carTelemetryData.clutch = br.ReadByte();
                carTelemetryData.gear = br.ReadSByte();
                carTelemetryData.engineRPM = br.ReadUInt16();
                carTelemetryData.drs = br.ReadByte();
                carTelemetryData.revLightsPercent = br.ReadByte();
                carTelemetryData.revLightsBitValue = br.ReadUInt16();
                carTelemetryData.brakesTemperature = new ushort[]
                {
                    br.ReadUInt16(),
                    br.ReadUInt16(),
                    br.ReadUInt16(),
                    br.ReadUInt16()
                };
                carTelemetryData.tyresSurfaceTemperature = new byte[]
                {
                    br.ReadByte(),
                    br.ReadByte(),
                    br.ReadByte(),
                    br.ReadByte()
                };
                carTelemetryData.tyresInnerTemperature = new byte[]
                {
                    br.ReadByte(),
                    br.ReadByte(),
                    br.ReadByte(),
                    br.ReadByte()
                };
                carTelemetryData.engineTemperature = br.ReadUInt16();
                carTelemetryData.tyresPressure = new float[]
                {
                    br.ReadSingle(),
                    br.ReadSingle(),
                    br.ReadSingle(),
                    br.ReadSingle()
                };
                carTelemetryData.surfaceType = new byte[]
                {
                    br.ReadByte(),
                    br.ReadByte(),
                    br.ReadByte(),
                    br.ReadByte()
                };
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("CarTelemetryData unreadable");
            }
            return carTelemetryData;
        }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private PacketSessionData ReadPacketSessionData(BinaryReader br)
        {
            PacketSessionData packetSessionData = new PacketSessionData();
            try
            {
                packetSessionData.weather = br.ReadByte();
                packetSessionData.trackTemperature = br.ReadSByte();
                packetSessionData.airTemperature = br.ReadSByte();
                packetSessionData.totalLaps = br.ReadByte();
                packetSessionData.trackLength = br.ReadUInt16();
                packetSessionData.sessionType = br.ReadByte();
                packetSessionData.trackId = br.ReadSByte();
                packetSessionData.formula = br.ReadByte();
                packetSessionData.sessionTimeLeft = br.ReadUInt16();
                packetSessionData.sessionDuration = br.ReadUInt16();
                packetSessionData.pitSpeedLimit = br.ReadByte();
                packetSessionData.gamePaused = br.ReadByte();
                packetSessionData.isSpectating = br.ReadByte();
                packetSessionData.spectatorCarIndex = br.ReadByte();
                packetSessionData.sliProNativeSupport = br.ReadByte();
                packetSessionData.numMarshalZones = br.ReadByte();
                packetSessionData.marshalZones = new MarshalZone[21];
                for (int i = 0; i < 21; i++)
                {
                    packetSessionData.marshalZones[i].zoneStart = br.ReadSingle();
                    packetSessionData.marshalZones[i].zoneFlag = br.ReadSByte();
                }

                ;
                packetSessionData.safetyCarStatus = br.ReadByte();
                packetSessionData.networkGame = br.ReadByte();
                packetSessionData.numWeatherForecastSamples = br.ReadByte();
                packetSessionData.weatherForecastSamples = new WeatherForecastSample[64];
                for (int i = 0; i < 64; i++)
                {
                    packetSessionData.weatherForecastSamples[i].sessionType = br.ReadByte();
                    packetSessionData.weatherForecastSamples[i].timeOffset = br.ReadByte();
                    packetSessionData.weatherForecastSamples[i].weather = br.ReadByte();
                    packetSessionData.weatherForecastSamples[i].trackTemperature = br.ReadSByte();
                    packetSessionData.weatherForecastSamples[i].trackTemperatureChange = br.ReadSByte();
                    packetSessionData.weatherForecastSamples[i].airTemperature = br.ReadSByte();
                    packetSessionData.weatherForecastSamples[i].airTemperatureChange = br.ReadSByte();
                    packetSessionData.weatherForecastSamples[i].rainPercentage = br.ReadByte();
                }

                ;
                packetSessionData.forecastAccuracy = br.ReadByte();
                packetSessionData.aiDifficulty = br.ReadByte();
                packetSessionData.seasonLinkIdentifier = br.ReadUInt32();
                packetSessionData.weekendLinkIdentifier = br.ReadUInt32();
                packetSessionData.sessionLinkIdentifier = br.ReadUInt32();
                packetSessionData.pitStopWindowIdealLap = br.ReadByte();
                packetSessionData.pitStopWindowLatestLap = br.ReadByte();
                packetSessionData.pitStopRejoinPosition = br.ReadByte();
                packetSessionData.steeringAssist = br.ReadByte();
                packetSessionData.brakingAssist = br.ReadByte();
                packetSessionData.gearboxAssist = br.ReadByte();
                packetSessionData.pitAssist = br.ReadByte();
                packetSessionData.pitReleaseAssist = br.ReadByte();
                packetSessionData.ERSAssist = br.ReadByte();
                packetSessionData.DRSAssist = br.ReadByte();
                packetSessionData.dynamicRacingLine = br.ReadByte();
                packetSessionData.dynamicRacingLineType = br.ReadByte();
                packetSessionData.gameMode = br.ReadByte();
                packetSessionData.ruleSet = br.ReadByte();
                packetSessionData.timeOfDay = br.ReadUInt32();
                packetSessionData.sessionLength = br.ReadByte();
                packetSessionData.speedUnitsLeadPlayer = br.ReadByte();
                packetSessionData.temperatureUnitsLeadPlayer = br.ReadByte();
                packetSessionData.speedUnitsSecondaryPlayer = br.ReadByte();
                packetSessionData.temperatureUnitsSecondaryPlayer = br.ReadByte();
                packetSessionData.numSafetyCarPeriods = br.ReadByte();
                packetSessionData.numVirtualSafetyCarPeriods = br.ReadByte();
                packetSessionData.numRedFlagPeriods = br.ReadByte();
                packetSessionData.equalCarPerformance = br.ReadByte();
                packetSessionData.recoveryMode = br.ReadByte();
                packetSessionData.flashbackLimit = br.ReadByte();
                packetSessionData.surfaceType = br.ReadByte();
                packetSessionData.lowFuelMode = br.ReadByte();
                packetSessionData.raceStarts = br.ReadByte();
                packetSessionData.tyreTemperature = br.ReadByte();
                packetSessionData.pitLaneTyreSim = br.ReadByte();
                packetSessionData.carDamage = br.ReadByte();
                packetSessionData.carDamageRate = br.ReadByte();
                packetSessionData.collisions = br.ReadByte();
                packetSessionData.collisionsOffForFirstLapOnly = br.ReadByte();
                packetSessionData.mpUnsafePitRelease = br.ReadByte();
                packetSessionData.mpOffForGriefing = br.ReadByte();
                packetSessionData.cornerCuttingStringency = br.ReadByte();
                packetSessionData.parcFermeRules = br.ReadByte();
                packetSessionData.pitStopExperience = br.ReadByte();
                packetSessionData.safetyCar = br.ReadByte();
                packetSessionData.safetyCarExperience = br.ReadByte();
                packetSessionData.formationLap = br.ReadByte();
                packetSessionData.formationLapExperience = br.ReadByte();
                packetSessionData.redFlags = br.ReadByte();
                packetSessionData.affectsLicenceLevelSolo = br.ReadByte();
                packetSessionData.affectsLicenceLevelMP = br.ReadByte();
                packetSessionData.numSessionsInWeekend = br.ReadByte();
                packetSessionData.weekendStructure = new byte[]
                {
                    br.ReadByte(),
                    br.ReadByte(),
                    br.ReadByte(),
                    br.ReadByte(),
                    br.ReadByte(),
                    br.ReadByte(),
                    br.ReadByte(),
                    br.ReadByte(),
                    br.ReadByte(),
                    br.ReadByte(),
                    br.ReadByte(),
                    br.ReadByte(),

                };
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("PacketSessionData unreadable");
            }
            return packetSessionData;
        }
    }
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------Structs
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketHeader
    {//29B
        public ushort packetFormat;
        public byte gameYear;
        public byte gameMajorVersion;
        public byte gameMinorVersion;
        public byte packetVersion;
        public byte packetId;
        public ulong sessionUID;
        public float sessionTime;
        public uint frameIdentifier;
        public uint overallFrameIdentifier;
        public byte playerCarIndex;
        public byte secondaryPlayerCarIndex;
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CarMotionData
    {//part of PacketMotionData
        public float worldPositionX; // World space X position - metres
        public float worldPositionY; // World space Y position
        public float worldPositionZ; // World space Z position
        public float worldVelocityX; // Velocity in world space X � metres/s
        public float worldVelocityY; // Velocity in world space Y
        public float worldVelocityZ; // Velocity in world space Z
        public short worldForwardDirX; // World space forward X direction (normalised)
        public short worldForwardDirY; // World space forward Y direction (normalised)
        public short worldForwardDirZ; // World space forward Z direction (normalised)
        public short worldRightDirX; // World space right X direction (normalised)
        public short worldRightDirY; // World space right Y direction (normalised)
        public short worldRightDirZ; // World space right Z direction (normalised)
        public float gForceLateral; // Lateral G-Force component
        public float gForceLongitudinal; // Longitudinal G-Force component
        public float gForceVertical; // Vertical G-Force component
        public float yaw; // Yaw angle in radians
        public float pitch; // Pitch angle in radians
        public float roll; // Roll angle in radians
    };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PacketMotionData
    {//1349B
        public PacketHeader header; // Header
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public CarMotionData[] carMotionData; // Data for all cars on track
    };
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MarshalZone
    {//part of PacketSessionData
        public float zoneStart; // Fraction (0..1) of way through the lap the marshal zone starts
        public sbyte zoneFlag; // -1 = invalid/unknown, 0 = none, 1 = green, 2 = blue, 3 = yellow
    };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WeatherForecastSample
    {// part of PacketSessionData
        public byte sessionType; // 0 = unknown, see appendix
        public byte timeOffset; // Time in minutes the forecast is for
        public byte weather; // Weather - 0 = clear, 1 = light cloud, 2 = overcast, 3 = light rain, 4 = heavy rain, 5 = storm
        public sbyte trackTemperature; // Track temp. in degrees Celsius
        public sbyte trackTemperatureChange; // Track temp. change � 0 = up, 1 = down, 2 = no change
        public sbyte airTemperature; // Air temp. in degrees celsius
        public sbyte airTemperatureChange; // Air temp. change � 0 = up, 1 = down, 2 = no change
        public byte rainPercentage; // Rain percentage (0-100)
    };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PacketSessionData
    {//753B
        // Header: public PacketHeader packetHeader;
        public byte weather; // Weather - 0 = clear, 1 = light cloud, 2 = overcast, 3 = light rain, 4 = heavy rain, 5 = storm
        public sbyte trackTemperature; // Track temp. in degrees celsius
        public sbyte airTemperature; // Air temp. in degrees celsius
        public byte totalLaps; // Total number of laps in this race
        public ushort trackLength; // Track length in metres
        public byte sessionType; // 0 = unknown, see appendix
        public sbyte trackId; // -1 for unknown, see appendix
        public byte formula; // Formula, 0 = F1 Modern, 1 = F1 Classic, 2 = F2, 3 = F1 Generic, 4 = Beta, 6 = Esports, 8 = F1 World, 9 = F1 Elimination
        public ushort sessionTimeLeft; // Time left in session in seconds
        public ushort sessionDuration; // Session duration in seconds
        public byte pitSpeedLimit; // Pit speed limit in kilometres per hour
        public byte gamePaused; // Whether the game is paused � network game only
        public byte isSpectating; // Whether the player is spectating
        public byte spectatorCarIndex; // Index of the car being spectated
        public byte sliProNativeSupport; // SLI Pro support, 0 = inactive, 1 = active
        public byte numMarshalZones; // Number of marshal zones to follow
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
        public MarshalZone[] marshalZones; // List of marshal zones � max 21
        public byte safetyCarStatus; // 0 = no safety car, 1 = full, 2 = virtual, 3 = formation lap
        public byte networkGame; // 0 = offline, 1 = online
        public byte numWeatherForecastSamples; // Number of weather samples to follow
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public WeatherForecastSample[] weatherForecastSamples; // Array of weather forecast samples
        public byte forecastAccuracy; // 0 = Perfect, 1 = Approximate
        public byte aiDifficulty; // AI Difficulty rating � 0-110
        public uint seasonLinkIdentifier; // Identifier for season - persists across saves
        public uint weekendLinkIdentifier; // Identifier for weekend - persists across saves
        public uint sessionLinkIdentifier; // Identifier for session - persists across saves
        public byte pitStopWindowIdealLap; // Ideal lap to pit on for current strategy (player)
        public byte pitStopWindowLatestLap; // Latest lap to pit on for current strategy (player)
        public byte pitStopRejoinPosition; // Predicted position to rejoin at (player)
        public byte steeringAssist; // 0 = off, 1 = on
        public byte brakingAssist; // 0 = off, 1 = low, 2 = medium, 3 = high
        public byte gearboxAssist; // 1 = manual, 2 = manual & suggested gear, 3 = auto
        public byte pitAssist; // 0 = off, 1 = on
        public byte pitReleaseAssist; // 0 = off, 1 = on
        public byte ERSAssist; // 0 = off, 1 = on
        public byte DRSAssist; // 0 = off, 1 = on
        public byte dynamicRacingLine; // 0 = off, 1 = corners only, 2 = full
        public byte dynamicRacingLineType; // 0 = 2D, 1 = 3D
        public byte gameMode; // Game mode id - see appendix
        public byte ruleSet; // Ruleset - see appendix
        public uint timeOfDay; // Local time of day - minutes since midnight
        public byte sessionLength; // 0 = None, 2 = Very Short, 3 = Short, 4 = Medium, 5 = Medium Long, 6 = Long, 7 = Full
        public byte speedUnitsLeadPlayer; // 0 = MPH, 1 = KPH
        public byte temperatureUnitsLeadPlayer; // 0 = Celsius, 1 = Fahrenheit
        public byte speedUnitsSecondaryPlayer; // 0 = MPH, 1 = KPH
        public byte temperatureUnitsSecondaryPlayer; // 0 = Celsius, 1 = Fahrenheit
        public byte numSafetyCarPeriods; // Number of safety cars called during session
        public byte numVirtualSafetyCarPeriods; // Number of virtual safety cars called
        public byte numRedFlagPeriods; // Number of red flags called during session
        public byte equalCarPerformance; // 0 = Off, 1 = On
        public byte recoveryMode; // 0 = None, 1 = Flashbacks, 2 = Auto-recovery
        public byte flashbackLimit; // 0 = Low, 1 = Medium, 2 = High, 3 = Unlimited
        public byte surfaceType; // 0 = Simplified, 1 = Realistic
        public byte lowFuelMode; // 0 = Easy, 1 = Hard
        public byte raceStarts; // 0 = Manual, 1 = Assisted
        public byte tyreTemperature; // 0 = Surface only, 1 = Surface & Carcass
        public byte pitLaneTyreSim; // 0 = On, 1 = Off
        public byte carDamage; // 0 = Off, 1 = Reduced, 2 = Standard, 3 = Simulation
        public byte carDamageRate; // 0 = Reduced, 1 = Standard, 2 = Simulation
        public byte collisions; // 0 = Off, 1 = Player-to-Player Off, 2 = On
        public byte collisionsOffForFirstLapOnly; // 0 = Disabled, 1 = Enabled
        public byte mpUnsafePitRelease; // 0 = On, 1 = Off (Multiplayer)
        public byte mpOffForGriefing; // 0 = Disabled, 1 = Enabled (Multiplayer)
        public byte cornerCuttingStringency; // 0 = Regular, 1 = Strict
        public byte parcFermeRules; // 0 = Off, 1 = On
        public byte pitStopExperience; // 0 = Automatic, 1 = Broadcast, 2 = Immersive
        public byte safetyCar; // 0 = Off, 1 = Reduced, 2 = Standard, 3 = Increased
        public byte safetyCarExperience; // 0 = Broadcast, 1 = Immersive
        public byte formationLap; // 0 = Off, 1 = On
        public byte formationLapExperience; // 0 = Broadcast, 1 = Immersive
        public byte redFlags; // 0 = Off, 1 = Reduced, 2 = Standard, 3 = Increased
        public byte affectsLicenceLevelSolo; // 0 = Off, 1 = On
        public byte affectsLicenceLevelMP; // 0 = Off, 1 = On
        public byte numSessionsInWeekend; // Number of session in following array
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] weekendStructure; // List of session types to show weekend; structure - see appendix for types
        public float sector2LapDistanceStart; // Distance in m around track where sector 2 starts
        public float sector3LapDistanceStart; // Distance in m around track where sector 3 starts
    };
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LapData
    {//part of PacketLapData
        public uint lastLapTimeInMS; // Last lap time in milliseconds
        public uint currentLapTimeInMS; // Current time around the lap in milliseconds
        public ushort sector1TimeMSPart; // Sector 1 time milliseconds part
        public byte sector1TimeMinutesPart; // Sector 1 whole minute part
        public ushort sector2TimeMSPart; // Sector 2 time milliseconds part
        public byte sector2TimeMinutesPart; // Sector 2 whole minute part
        public ushort deltaToCarInFrontMSPart; // Time delta to car in front milliseconds part
        public byte deltaToCarInFrontMinutesPart; // Time delta to car in front whole minute part
        public ushort deltaToRaceLeaderMSPart; // Time delta to race leader milliseconds part
        public byte deltaToRaceLeaderMinutesPart; // Time delta to race leader whole minute part
        public float lapDistance; // Distance vehicle is around current lap in metres � could be negative if line hasn�t been crossed yet
        public float totalDistance; // Total distance travelled in session in metres � could be negative if line hasn�t been crossed yet
        public float safetyCarDelta; // Delta in seconds for safety car
        public byte carPosition; // Car race position
        public byte currentLapNum; // Current lap number
        public byte pitStatus; // 0 = none, 1 = pitting, 2 = in pit area
        public byte numPitStops; // Number of pit stops taken in this race
        public byte sector; // 0 = sector1, 1 = sector2, 2 = sector3
        public byte currentLapInvalid; // Current lap invalid - 0 = valid, 1 = invalid
        public byte penalties; // Accumulated time penalties in seconds to be added
        public byte totalWarnings; // Accumulated number of warnings issued
        public byte cornerCuttingWarnings; // Accumulated number of corner cutting warnings issued
        public byte numUnservedDriveThroughPens; // Num drive through pens left to serve
        public byte numUnservedStopGoPens; // Num stop go pens left to serve
        public byte gridPosition; // Grid position the vehicle started the race in
        public byte driverStatus; // Status of driver - 0 = in garage, 1 = flying lap, 2 = in lap, 3 = out lap, 4 = on track
        public byte resultStatus; // Result status - 0 = invalid, 1 = inactive, 2 = active, 3 = finished, 4 = didnotfinish, 5 = disqualified, 6 = not classified, 7 = retired
        public byte pitLaneTimerActive; // Pit lane timing, 0 = inactive, 1 = active
        public ushort pitLaneTimeInLaneInMS; // If active, the current time spent in the pit lane in ms
        public ushort pitStopTimerInMS; // Time of the actual pit stop in ms
        public byte pitStopShouldServePen; // Whether the car should serve a penalty at this stop
        public float speedTrapFastestSpeed; // Fastest speed through speed trap for this car in kmph
        public byte speedTrapFastestLap; // Lap no the fastest speed was achieved, 255 = not set
    };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PacketLapData
    {//1285B
        public PacketHeader header; // Header
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public LapData[] lapData; // Lap data for all cars on track
        public byte timeTrialPBCarIdx; // Index of Personal Best car in time trial (255 if invalid)
        public byte timeTrialRivalCarIdx; // Index of Rival car in time trial (255 if invalid)
    };
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketEventData
    {//45B
        public PacketHeader header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] eventStringCode;
        public EventDataDetails eventDetails;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct EventDataDetails
    {// part of PacketEventData
        [FieldOffset(0)] public FastestLap FastestLap;
        [FieldOffset(0)] public Retirement Retirement;
        [FieldOffset(0)] public TeamMateInPits TeamMateInPits;
        [FieldOffset(0)] public RaceWinner RaceWinner;
        [FieldOffset(0)] public Penalty Penalty;
        [FieldOffset(0)] public SpeedTrap SpeedTrap;
        [FieldOffset(0)] public StartLights StartLights;
        [FieldOffset(0)] public DriveThroughPenaltyServed DriveThroughPenaltyServed;
        [FieldOffset(0)] public StopGoPenaltyServed StopGoPenaltyServed;
        [FieldOffset(0)] public Flashback Flashback;
        [FieldOffset(0)] public Buttons Buttons;
        [FieldOffset(0)] public Overtake Overtake;
        [FieldOffset(0)] public SafetyCar SafetyCar;
        [FieldOffset(0)] public Collision Collision;
        /*
         Session Started	�SSTA�	Sent when the session starts
            Session Ended	�SEND�	Sent when the session ends
            Fastest Lap	�FTLP�	When a driver achieves the fastest lap
            Retirement	�RTMT�	When a driver retires
            DRS enabled	�DRSE�	Race control have enabled DRS
            DRS disabled	�DRSD�	Race control have disabled DRS
            Team mate in pits	�TMPT�	Your team mate has entered the pits
            Chequered flag	�CHQF�	The chequered flag has been waved
            Race Winner	�RCWN�	The race winner is announced
            Penalty Issued	�PENA�	A penalty has been issued � details in event
            Speed Trap Triggered	�SPTP�	Speed trap has been triggered by fastest speed
            Start lights	�STLG�	Start lights � number shown
            Lights out	�LGOT�	Lights out
            Drive through served	�DTSV�	Drive through penalty served
            Stop go served	�SGSV�	Stop go penalty served
            Flashback	�FLBK�	Flashback activated
            Button status	�BUTN�	Button status changed
            Red Flag	�RDFL�	Red flag shown
            Overtake	�OVTK�	Overtake occurred
            Safety Car	"SCAR"	Safety car event - details in event
            Collision	"COLL"	Collision between two vehicles has occurred
        */
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FastestLap
    {// part of EventDataDetails
        public byte vehicleIdx;
        public float lapTime;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Retirement
    {// part of EventDataDetails
        public byte vehicleIdx;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TeamMateInPits
    {// part of EventDataDetails
        public byte vehicleIdx;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RaceWinner
    {// part of EventDataDetails
        public byte vehicleIdx;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Penalty
    {// part of EventDataDetails
        public byte penaltyType;
        public byte infringementType;
        public byte vehicleIdx;
        public byte otherVehicleIdx;
        public byte time;
        public byte lapNum;
        public byte placesGained;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SpeedTrap
    {// part of EventDataDetails
        public byte vehicleIdx;
        public float speed;
        public byte isOverallFastestInSession;
        public byte isDriverFastestInSession;
        public byte fastestVehicleIdxInSession;
        public float fastestSpeedInSession;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StartLights
    {// part of EventDataDetails
        public byte numLights;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DriveThroughPenaltyServed
    {// part of EventDataDetails
        public byte vehicleIdx;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StopGoPenaltyServed
    {// part of EventDataDetails
        public byte vehicleIdx;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Flashback
    {// part of EventDataDetails
        public uint flashbackFrameIdentifier;
        public float flashbackSessionTime;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Buttons
    {// part of EventDataDetails
        public uint buttonStatus;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Overtake
    {// part of EventDataDetails
        public byte overtakingVehicleIdx;
        public byte beingOvertakenVehicleIdx;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SafetyCar
    {// part of EventDataDetails
        public byte safetyCarType;
        public byte eventType;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Collision
    {// part of EventDataDetails
        public byte vehicle1Idx;
        public byte vehicle2Idx;
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ParticipantData
    {//part of PacketParticipantsData
        public byte aiControlled;         // 1 = AI, 0 = Human
        public byte driverId;             // Driver ID
        public byte networkId;            // Unique network ID
        public byte teamId;               // Team ID
        public byte myTeam;               // 1 = My Team
        public byte raceNumber;           // Car number
        public byte nationality;          // Nationality
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        public byte[] name;               // UTF-8 null-terminated name
        public byte yourTelemetry;        // 0 = restricted, 1 = public
        public byte showOnlineNames;      // 0 = off, 1 = on
        public ushort techLevel;          // F1 World tech level
        public byte platform;             // Platform ID
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketParticipantsData
    {//1350B
        public PacketHeader header;       // Header
        public byte numActiveCars;        // Number of active cars
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public ParticipantData[] participants; // Array of 22 participants
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CarSetupData
    {//part of PacketCarSetupData
        public byte frontWing;             // Front wing aero
        public byte rearWing;              // Rear wing aero
        public byte onThrottle;            // Diff on throttle
        public byte offThrottle;           // Diff off throttle
        public float frontCamber;          // Front camber angle
        public float rearCamber;           // Rear camber angle
        public float frontToe;             // Front toe angle
        public float rearToe;              // Rear toe angle
        public byte frontSuspension;       // Front suspension
        public byte rearSuspension;        // Rear suspension
        public byte frontAntiRollBar;      // Front anti-roll bar
        public byte rearAntiRollBar;       // Rear anti-roll bar
        public byte frontSuspensionHeight; // Front ride height
        public byte rearSuspensionHeight;  // Rear ride height
        public byte brakePressure;         // Brake pressure %
        public byte brakeBias;             // Brake bias %
        public byte engineBraking;         // Engine braking %
        public float rearLeftTyrePressure; // Rear left tyre pressure
        public float rearRightTyrePressure;// Rear right tyre pressure
        public float frontLeftTyrePressure;// Front left tyre pressure
        public float frontRightTyrePressure;// Front right tyre pressure
        public byte ballast;               // Ballast
        public float fuelLoad;             // Fuel load
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketCarSetupData
    {//113B
        public PacketHeader header;       // Header
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public CarSetupData[] carSetups;  // 22 cars' setups
        public float nextFrontWingValue;  // Front wing value after next pit stop
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CarTelemetryData
    {//part of PacketCarTelemetryData
        public ushort speed;                    // Speed of car in kilometres per hour
        public float throttle;                  // Amount of throttle applied (0.0 to 1.0)
        public float steer;                     // Steering (-1.0 (full lock left) to 1.0 (full lock right))
        public float brake;                     // Amount of brake applied (0.0 to 1.0)
        public byte clutch;                     // Amount of clutch applied (0 to 100)
        public sbyte gear;                      // Gear selected (1-8, N=0, R=-1)
        public ushort engineRPM;                // Engine RPM
        public byte drs;                        // 0 = off, 1 = on
        public byte revLightsPercent;           // Rev lights indicator (percentage)
        public ushort revLightsBitValue;        // Rev lights (bit 0 = leftmost LED, bit 14 = rightmost LED)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] brakesTemperature;      // Brakes temperature (celsius)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] tyresSurfaceTemperature;  // Tyres surface temperature (celsius)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] tyresInnerTemperature;    // Tyres inner temperature (celsius)
        public ushort engineTemperature;        // Engine temperature (celsius)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] tyresPressure;           // Tyres pressure (PSI)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] surfaceType;              // Driving surface, see appendices
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CarStatusData
    {//part of PacketCarStatusData
        public byte tractionControl;              // 0 = off, 1 = medium, 2 = full
        public byte antiLockBrakes;               // 0 = off, 1 = on
        public byte fuelMix;                      // 0 = lean, 1 = standard, 2 = rich, 3 = max
        public byte frontBrakeBias;               // percentage
        public byte pitLimiterStatus;             // 0 = off, 1 = on
        public float fuelInTank;                  // fuel mass
        public float fuelCapacity;                // fuel capacity
        public float fuelRemainingLaps;           // laps of fuel remaining
        public ushort maxRPM;                     // max engine RPM
        public ushort idleRPM;                    // idle RPM
        public byte maxGears;                     // max gear count
        public byte drsAllowed;                   // 0 = not allowed, 1 = allowed
        public ushort drsActivationDistance;      // 0 = not available, else in meters
        public byte actualTyreCompound;           // compound code
        public byte visualTyreCompound;           // visual compound
        public byte tyresAgeLaps;                 // age of tyres in laps
        public sbyte vehicleFiaFlags;             // -1 = invalid, 0 = none, 1 = green, 2 = blue, 3 = yellow
        public float enginePowerICE;              // W
        public float enginePowerMGUK;             // W
        public float ersStoreEnergy;              // ERS in Joules
        public byte ersDeployMode;                // 0 = none, 1 = medium, 2 = hotlap, 3 = overtake
        public float ersHarvestedThisLapMGUK;     // harvested this lap by MGU-K
        public float ersHarvestedThisLapMGUH;     // harvested this lap by MGU-H
        public float ersDeployedThisLap;          // deployed this lap
        public byte networkPaused;                // 1 = paused
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketCarStatusData
    {//1239B
        public PacketHeader header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public CarStatusData[] carStatusData;
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FinalClassificationData
    {//part of PacketFinalClassificationData
        public byte position;                      // Finishing position
        public byte numLaps;                       // Laps completed
        public byte gridPosition;                  // Starting grid position
        public byte points;                        // Points scored
        public byte numPitStops;                   // Number of pit stops
        public byte resultStatus;                  // Status of result (0-7)
        public uint bestLapTimeInMS;               // Best lap in ms
        public double totalRaceTime;               // Total race time (s)
        public byte penaltiesTime;                 // Total penalties (s)
        public byte numPenalties;                  // Count of penalties
        public byte numTyreStints;                 // Total tyre stints
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] tyreStintsActual;            // Actual tyre types used
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] tyreStintsVisual;            // Visual tyre types
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] tyreStintsEndLaps;           // Lap each stint ended on
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketFinalClassificationData
    {//1020B
        public PacketHeader header;
        public byte numCars; // Number of cars classified
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public FinalClassificationData[] classificationData;
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LobbyInfoData
    {//part of PacketLobbyInfoData
        public byte aiControlled;             // 1 = AI, 0 = Human
        public byte teamId;                   // 255 = no team
        public byte nationality;              // Nationality ID
        public byte platform;                 // Platform ID
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        public byte[] name;                   // UTF-8 name, null-terminated
        public byte carNumber;                // Car number
        public byte yourTelemetry;           // 0 = restricted, 1 = public
        public byte showOnlineNames;         // 0 = off, 1 = on
        public ushort techLevel;             // F1 World tech level
        public byte readyStatus;             // 0 = not ready, 1 = ready, 2 = spectating
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketLobbyInfoData
    {//1306B
        public PacketHeader header;
        public byte numPlayers; // Number of players in lobby
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public LobbyInfoData[] lobbyPlayers;
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CarDamageData
    {//part of PacketCarDamageData
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] tyresWear;             // Percentage (0.0 to 1.0)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] tyresDamage;            // Percentage (0 to 100)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] brakesDamage;           // Percentage (0 to 100)
        public byte frontLeftWingDamage;      // Percentage
        public byte frontRightWingDamage;     // Percentage
        public byte rearWingDamage;           // Percentage
        public byte floorDamage;              // Percentage
        public byte diffuserDamage;           // Percentage
        public byte sidepodDamage;            // Percentage
        public byte drsFault;                 // 0 = OK, 1 = fault
        public byte ersFault;                 // 0 = OK, 1 = fault
        public byte gearBoxDamage;            // Percentage
        public byte engineDamage;             // Percentage
        public byte engineMGUHWear;           // Percentage
        public byte engineESWear;             // Percentage
        public byte engineCEWear;             // Percentage
        public byte engineICEWear;            // Percentage
        public byte engineMGUKWear;           // Percentage
        public byte engineTCWear;             // Percentage
        public byte engineBlown;              // 0 = OK, 1 = fault
        public byte engineSeized;             // 0 = OK, 1 = fault
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketCarDamageData
    {//953B
        public PacketHeader header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public CarDamageData[] carDamageData;
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LapHistoryData
    {//part of PacketSessionHistoryData
        public uint lapTimeInMS;               // Lap time in milliseconds
        public ushort sector1TimeInMS;         // Sector 1 time in ms
        public byte sector1TimeMinutes;        // Sector 1 whole minutes
        public ushort sector2TimeInMS;         // Sector 2 time in ms
        public byte sector2TimeMinutes;        // Sector 2 whole minutes
        public ushort sector3TimeInMS;         // Sector 3 time in ms
        public byte sector3TimeMinutes;        // Sector 3 whole minutes
        public byte lapValidBitFlags;          // Bit flags for valid lap/sector
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TyreStintHistoryData
    {//part of PacketSessionHistoryData
        public byte endLap;                    // 255 = current tyre
        public byte tyreActualCompound;        // Actual compound
        public byte tyreVisualCompound;        // Visual compound
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketSessionHistoryData
    {//1460B
        public PacketHeader header;
        public byte carIdx;                    // Index of car
        public byte numLaps;                   // Total laps (including current)
        public byte numTyreStints;             // Total stints
        public byte bestLapTimeLapNum;         // Lap of best lap
        public byte bestSector1LapNum;         // Lap of best sector 1
        public byte bestSector2LapNum;         // Lap of best sector 2
        public byte bestSector3LapNum;         // Lap of best sector 3
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public LapHistoryData[] lapHistoryData;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public TyreStintHistoryData[] tyreStintsHistoryData;
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TyreSetData
    {//part of PacketTyreSetsData
        public byte actualTyreCompound;         // Actual compound
        public byte visualTyreCompound;         // Visual compound
        public byte wear;                       // Wear percentage
        public byte available;                  // 1 = available
        public byte recommendedSession;         // Recommended session
        public byte lifeSpan;                   // Laps remaining
        public byte usableLife;                 // Max laps suggested
        public short lapDeltaTime;              // Delta time (ms)
        public byte fitted;                     // 1 = fitted
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketTyreSetsData
    {//231B
        public PacketHeader header;
        public byte carIdx;                     // Car index
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public TyreSetData[] tyreSetData;       // 20 tyre sets
        public byte fittedIdx;                  // Index of fitted set
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketMotionExData
    {//237B
        public PacketHeader header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] suspensionPosition;        // RL, RR, FL, FR
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] suspensionVelocity;        // RL, RR, FL, FR
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] suspensionAcceleration;    // RL, RR, FL, FR
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] wheelSpeed;                // Speed of each wheel
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] wheelSlipRatio;            // Slip ratio of each wheel
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] wheelSlipAngle;            // Slip angle of each wheel
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] wheelLatForce;             // Lateral force of each wheel
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] wheelLongForce;            // Longitudinal force of each wheel
        public float heightOfCOGAboveGround;
        public float localVelocityX;
        public float localVelocityY;
        public float localVelocityZ;
        public float angularVelocityX;
        public float angularVelocityY;
        public float angularVelocityZ;
        public float angularAccelerationX;
        public float angularAccelerationY;
        public float angularAccelerationZ;
        public float frontWheelsAngle;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] wheelVertForce;            // Vertical force per wheel
        public float frontAeroHeight;
        public float rearAeroHeight;
        public float frontRollAngle;
        public float rearRollAngle;
        public float chassisYaw;
    }
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TimeTrialDataSet
    {//part of PacketTimeTrialData
        public byte carIdx;                   // Car index
        public byte teamId;                   // Team ID
        public uint lapTimeInMS;              // Lap time in milliseconds
        public uint sector1TimeInMS;          // Sector 1 time
        public uint sector2TimeInMS;          // Sector 2 time
        public uint sector3TimeInMS;          // Sector 3 time
        public byte tractionControl;          // 0 = off, 1 = medium, 2 = full
        public byte gearboxAssist;            // 1 = manual, 2 = manual+suggested, 3 = auto
        public byte antiLockBrakes;           // 0 = off, 1 = on
        public byte equalCarPerformance;      // 0 = Realistic, 1 = Equal
        public byte customSetup;              // 0 = No, 1 = Yes
        public byte valid;                    // 0 = invalid, 1 = valid
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketTimeTrialData
    {//101B
        public PacketHeader header;
        public TimeTrialDataSet playerSessionBestDataSet;
        public TimeTrialDataSet personalBestDataSet;
        public TimeTrialDataSet rivalDataSet;
    }
}