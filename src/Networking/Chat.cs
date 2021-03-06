using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Threading;
using ElectronChat.Models;

namespace ElectronChat.Networking
{
    class Chat
    {
        private readonly NetworkStream stream;
        private ObservableCollection<MessageItem> messages = new ObservableCollection<MessageItem>();
        public readonly Node opponent;

        public ObservableCollection<MessageItem> Messages {
            get => messages;
        }

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
                    string messageText = Crypto.AES256Decrypt(key, NetUtils.Read(stream));
                    DateTime time = DateTime.Now;
                    messages.Add(new MessageItem() { SenderName = opponent.Name, MessageText = messageText, Date = time });
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
                NetUtils.Write(stream, Crypto.AES256Encrypt(key, message));
                messages.Add(new MessageItem(){ Date = DateTime.Now, MessageText = message, SenderName = Program.settings.Name });
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