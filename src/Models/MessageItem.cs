using System;

namespace ElectronChat.Models
{
    struct MessageItem
    {
        public string SenderName {get; set;}
        public string MessageText {get; set;}
        public DateTime Date {get; set;}
        public string DateString
        {
            get => Date.ToString();
        }
    } 
}