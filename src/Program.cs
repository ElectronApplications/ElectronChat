using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using ElectronChat.Networking;
using ElectronChat.Views;

namespace ElectronChat
{
    class Program
    {
        
        public static Settings settings;
        public static List<Chat> chats = new List<Chat>();
        
        static void Main(string[] args)
        {
            settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText("settings.json"));
            Listener listener = new Listener(IPAddress.Parse(settings.IP), settings.Port);
            
            try
            {
                Utils.RefreshNodes();
            }
            catch (Exception) {}
            
            App.Create(args);
        }

    }
}
