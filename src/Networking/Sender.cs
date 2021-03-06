using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ElectronChat.Networking
{
    class Sender
    {
        private readonly TcpClient tcpClient;
        private readonly NetworkStream stream;
        private readonly Node node;
        private readonly byte[] key = new byte[32];

        public Sender(Node node, int receiveTimeOut = 120000) //120 sec
        {
            tcpClient = new TcpClient();
            tcpClient.ReceiveTimeout = receiveTimeOut;
            tcpClient.Connect(IPAddress.Parse(node.IP), node.Port);
            stream = tcpClient.GetStream();
            this.node = node;

            //Establish secure connection
            byte[] rsaKey = NetUtils.Read(stream);

            //Generate a key for AES256
            new Random().NextBytes(key);

            NetUtils.Write(stream, Crypto.RSAEncrypt(Encoding.UTF8.GetString(rsaKey), key));

            //Register
            NetUtils.Write(stream, Crypto.AES256Encrypt(key, $"{Program.settings.Name} {Program.settings.Port}"));
        }
        
        public string Send(string data)
        {
            NetUtils.Write(stream, Crypto.AES256Encrypt(key, data));
            return Crypto.AES256Decrypt(key, NetUtils.Read(stream));
        }

        public void Close()
        {
            stream.Close();
            tcpClient.Close();
        }

        public Chat MakeChat()
        {
            NetUtils.Write(stream, Crypto.AES256Encrypt(key, "Chat"));
            Chat chat = new Chat(stream, key, node);
            Program.chats.Add(chat);
            return chat;
        }

    }
}