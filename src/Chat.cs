using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace ElectronChat
{
    class Chat
    {
        NetworkStream stream;
        public List<string> messages = new List<string>();
        public Node opponent;
        private byte[] key;

        public Chat(NetworkStream stream, byte[] key, Node node)
        {
            this.stream = stream;
            this.key = key;
            opponent = node;
            new Thread(() => ListenMessages()).Start();
        }

        public void ListenMessages()
        {
            try
            {
                while (true)
                {
                    string message = Crypto.AES256Decrypt(key, Program.ReadAll(stream));
                    messages.Add(message);
                }
            }
            catch (Exception)
            {
                Close();
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                Program.WriteAll(stream, Crypto.AES256Encrypt(key, message));
            }
            catch (Exception)
            {
                Close();
            }
        }

        public void Close()
        {
            stream.Close();
            Program.chats.Remove(this);
        }

    }
}