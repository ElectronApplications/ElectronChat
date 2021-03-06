using System;
using System.Net.Sockets;
using System.Timers;

namespace ElectronChat.Networking
{
    class NetUtils
    {
        public const int MaxPacketSize = 8388608; //8 MB

        public static void Write(NetworkStream stream, byte[] msg)
        {
            stream.Write(BitConverter.GetBytes(msg.Length), 0, 4);
            stream.Write(msg, 0, msg.Length);
        }

        public static byte[] Read(NetworkStream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int length = BitConverter.ToInt32(buffer);

            if (length <= 0 || length > MaxPacketSize)
                throw new Exception("Invalid packet size!");

            buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }

    }
}