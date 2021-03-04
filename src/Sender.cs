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
            byte[] rsaKey = Program.ReadAll(stream);

            //Generate a key for AES256
            new Random().NextBytes(key);

            Program.WriteAll(stream, Crypto.RSAEncrypt(Encoding.UTF8.GetString(rsaKey), key));

            //Register
            Program.WriteAll(stream, Crypto.AES256Encrypt(key, $"{Program.settings.Name} {Program.settings.Port}"));
        }
        
        public string Send(string data)
        {
            Program.WriteAll(stream, Crypto.AES256Encrypt(key, data));
            return Crypto.AES256Decrypt(key, Program.ReadAll(stream));
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