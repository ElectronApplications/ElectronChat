using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using ElectronChat.Models;
using ElectronChat.Networking;
using ReactiveUI;

namespace ElectronChat.ViewModels
{
    class ChatViewModel : ViewModelBase
    {
        public ObservableCollection<MessageItem> Messages
        {
            get => messages;
            set => this.RaiseAndSetIfChanged(ref messages, value);
        }
        public string MessageText
        {
            get => messageText;
            set => this.RaiseAndSetIfChanged(ref messageText, value);
        }
        
        private ObservableCollection<MessageItem> messages;
        private string messageText;
        private Chat chat;

        public void ChangeChat(Chat chat)
        {
            this.chat = chat;
            Messages = chat.Messages;
        }

        public void SendMessage()
        {
            if (MessageText.Length > 0)
            {
                chat.SendMessage(MessageText);
                MessageText = "";
            }
        }
    }
}