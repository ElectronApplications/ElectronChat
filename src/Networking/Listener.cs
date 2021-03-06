using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace ElectronChat.Networking
{
    class Listener
    {
        private readonly TcpListener tcpListener;

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
                NetworkStream stream = client.GetStream();

                //Establish secure connection
                byte[] key = new byte[32];
                var keysRSA = Crypto.RSACreate();

                NetUtils.Write(stream, Encoding.UTF8.GetBytes(keysRSA.publicKey));
                key = Crypto.RSADecrypt(keysRSA.privateKey, NetUtils.Read(stream));

                //Register node
                string[] requestReg = Crypto.AES256Decrypt(key, NetUtils.Read(stream)).Split(" ");
                Node node;
                string ip = ((IPEndPoint) client.Client.RemoteEndPoint).Address.ToString();
                if (Utils.ContainsNode(requestReg[0], ip, int.Parse(requestReg[1])))
                    node = Utils.GetNode(requestReg[0], ip, int.Parse(requestReg[1]));
                else
                {
                    node = new Node() { IP = ip, Name = requestReg[0], Port = int.Parse(requestReg[1]) };
                    new Sender(node).Send("Ping"); //if it throws an exception, the connection will close
                    Program.settings.Nodes.Add(node);
                    Utils.SaveSettings();
                }

                while(true)
                {
                    try
                    {
                        String msg = Crypto.AES256Decrypt(key, NetUtils.Read(stream));
                        if (msg == "Chat")
                        {
                            Chat chat = new Chat(stream, key, node);
                            Program.chats.Add(chat);
                            break;
                        }
                        else
                            NetUtils.Write(stream, Crypto.AES256Encrypt(key, ParseRequest(msg, client))); 
                    }
                    catch (Exception) { break; }
                }
            }
            catch (Exception) {
                client.Close();
            }
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