using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ElectronChat
{
    class Sender
    {
        TcpClient tcpClient;
        NetworkStream stream;
        Node node;
        byte[] key = new byte[32];
        public Sender(Node node)
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Parse(node.IP), node.Port);
            stream = tcpClient.GetStream();
            this.node = node;

            //Establish secure connection
            byte[] rsaKey = Program.Read(stream);

            //Generate a key for AES256
            new Random().NextBytes(key);

            Program.Write(stream, Crypto.RSAEncrypt(Encoding.UTF8.GetString(rsaKey), key));

            //Register
            Program.Write(stream, Crypto.AES256Encrypt(key, $"{Program.settings.Name} {Program.settings.Port}"));
        }
        
        public string Send(string data)
        {
            Program.Write(stream, Crypto.AES256Encrypt(key, data));
            return Crypto.AES256Decrypt(key, Program.Read(stream));
        }

        public void Close()
        {
            stream.Close();
            tcpClient.Close();
        }

        public Chat MakeChat()
        {
            Send("Chat");
            Chat chat = new Chat(stream, key, node);
            Program.chats.Add(chat);
            return chat;
        }

    }
}