using System;
using System.Net;
using System.Net.Sockets;

namespace UDPTester
{
    class Program
    {
        static void Main()
        {
            ushort speed = 300;
            byte throttle = 80;
            byte steer = 0;
            byte brake = 0;
            byte gear = 3;

            byte[] packet = new byte[6];
            packet[0] = (byte)(speed & 0xff);
            packet[1] = (byte)((speed >> 8) & 0xff);
            packet[2] = throttle;
            packet[3] = steer;
            packet[4] = brake;
            packet[5] = gear;

            using (UdpClient client = new UdpClient())
            {
                client.Send(packet, packet.Length, "listener", 20777);
            }

            Console.WriteLine("Telemetry packet sent!");
        }
    }
}