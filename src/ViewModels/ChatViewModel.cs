using System;
using System.Collections.ObjectModel;
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
            set { this.RaiseAndSetIfChanged(ref messageText, value); IsEnabled = (!string.IsNullOrWhiteSpace(MessageText) && chat != null && !chat.IsClosed); }
        }
        public bool IsEnabled
        {
            get => isEnabled;
            set => this.RaiseAndSetIfChanged(ref isEnabled, value);
        }
        
        private bool isEnabled;
        private ObservableCollection<MessageItem> messages;
        private string messageText;
        private Chat chat;

        public void ChangeChat(Chat chat)
        {
            this.chat = chat;
            Messages = chat.Messages;
            MessageText = "";
        }

        public void RemoveChat()
        {
            this.chat = null;
            Messages = new ObservableCollection<MessageItem>();
            MessageText = "";
        }

        public void SendMessage()
        {
            if (chat.IsClosed)
                RemoveChat();
            else if (MessageText.Length > 0)
            {
                chat.SendMessage(MessageText);
                MessageText = "";
            }
        }
    }
}