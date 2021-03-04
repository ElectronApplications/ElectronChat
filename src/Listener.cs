using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace ElectronChat
{
    class Listener
    {
        TcpListener tcpListener;

        public Listener(IPAddress ip, int port)
        {
            tcpListener = new TcpListener(ip, port);
            tcpListener.Start();
            
            new Thread(() => ListenRequests()).Start();
        }

        void ListenRequests()
        {
            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                new Thread(() => AcceptClient(client)).Start();
            }
        }

        void AcceptClient(TcpClient client)
        {
            try
            {
                byte[] bytes = new byte[Program.PacketSize];
                String data = null;

                NetworkStream stream = client.GetStream();

                //Establish secure connection
                byte[] key = new byte[32];
                var keysRSA = Crypto.RSACreate();

                Program.WriteAll(stream, Encoding.UTF8.GetBytes(keysRSA.publicKey));
                key = Crypto.RSADecrypt(keysRSA.privateKey, Program.ReadAll(stream));

                //Register node
                string[] requestReg = Crypto.AES256Decrypt(key, Program.ReadAll(stream)).Split(" ");
                Node node = new Node() { IP = ((IPEndPoint) client.Client.RemoteEndPoint).Address.ToString(), Name = requestReg[0], Port = int.Parse(requestReg[1]) };
                if (!Program.ContainsNode(node) && new Sender(node).Send("Ping") == "Pong")
                {
                    Program.settings.Nodes.Add(node);
                    Program.SaveSettings();
                }

                while(true)
                {
                    try
                    {
                        data = Crypto.AES256Decrypt(key, Program.ReadAll(stream));
                        if (data == "Chat")
                        {
                            Chat chat = new Chat(stream, key, node);
                            Program.chats.Add(chat);
                            break;
                        }
                        else
                            Program.WriteAll(stream, Crypto.AES256Encrypt(key, ParseRequest(data, client))); 
                    }
                    catch (Exception) { break; }
                }
            }
            catch (Exception) {}
        }

        string ParseRequest(string request, TcpClient client)
        {
            string[] requestArr = request.Split();

            if (requestArr[0] == "Ping")
                return "Pong";
            else if (requestArr[0] == "GetNodes")
                return JsonSerializer.Serialize(Program.settings.Nodes);
            else
                return "Error";
        }

    }
}