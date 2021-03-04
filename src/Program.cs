﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

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
            
            new Thread(() =>
            {
                try
                {
                    RefreshNodes();
                }
                catch (Exception) {}
            }).Start();
            
            Thread.Sleep(3000);
            InterfaceCLI();
        }

        public static void InterfaceCLI()
        {
            Console.WriteLine("Available nodes: ");
            for (int i = 0; i < settings.Nodes.Count; i++) 
            {
                Console.WriteLine($"ID: {i}, Name: {settings.Nodes[i].Name}, IP: {settings.Nodes[i].IP}, Port: {settings.Nodes[i].Port}");
            }
            Console.WriteLine("\nActive chats: ");
            for (int i = 0; i < chats.Count; i++)
            {
                Console.WriteLine($"ID: {i}, Name: {chats[i].opponent.Name}, IP: {chats[i].opponent.IP}");
            }

            Console.WriteLine("\nUse Ajoin {ID} to join an active chat or Njoin {ID} to start a new chat");
            string response = Console.ReadLine();
            if (response.StartsWith("Ajoin"))
                ChatCLI(chats[int.Parse(response.Split(" ")[1])]);
            else if (response.StartsWith("Njoin"))
            {
                Sender sender = new Sender(settings.Nodes[int.Parse(response.Split(" ")[1])]);
                Chat chat = sender.MakeChat();
                ChatCLI(chat);
            }
            else
                InterfaceCLI();
        }

        public static void ChatCLI(Chat chat)
        {
            new Thread(() => 
            {
                int i = 0;
                while (true)
                {
                    if (chat.messages.Count > i)
                    {
                        Console.WriteLine($"[{chat.opponent.Name}] >> {chat.messages[i]}");
                        i++;
                    }
                    else
                        Thread.Sleep(100);
                }
            }).Start();

            while (true)
            {
                string message = Console.ReadLine();
                chat.SendMessage(message);
            }
        }

        public static void RefreshNodes()
        {
            List<Node> toAdd = new List<Node>();
            List<Node> toRemove = new List<Node>();

            foreach (var node in settings.Nodes)
            {
                try
                {
                    if (node.IP == settings.IP && node.Port == settings.Port)
                        continue;
                    
                    Sender sender = new Sender(node);
                    string nodes = sender.Send("GetNodes");
                    sender.Close();
                    List<Node> retrievedNodes = JsonSerializer.Deserialize<List<Node>>(nodes);
                    foreach (var rNode in retrievedNodes)
                        if (!ContainsNode(rNode))
                            toAdd.Add(rNode);
                    break;
                } catch (Exception)
                {
                    toRemove.Add(node);
                }
            }

            foreach (var remove in toRemove)
            {
                settings.Nodes.Remove(remove);
            }

            foreach (var add in toAdd)
            {
                if (new Sender(add).Send("Ping") == "Pong")
                    settings.Nodes.Add(add);
            }

            SaveSettings();
        }

        public static bool ContainsNode(Node node)
        {
            foreach (var snode in settings.Nodes)
                if (snode.IP == node.IP && snode.Port == node.Port && snode.Name == node.Name)
                    return true;
            return false;
        }

        public static void Write(NetworkStream stream, byte[] msg)
        {
            stream.Write(BitConverter.GetBytes(msg.Length), 0, 4);
            stream.Write(msg, 0, msg.Length);
        }

        public static byte[] Read(NetworkStream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int length = BitConverter.ToInt32(buffer);

            buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }

        public static void SaveSettings()
        {
            File.WriteAllText("settings.json", JsonSerializer.Serialize(settings, new JsonSerializerOptions(){ WriteIndented = true }));
        }

        public static byte[] TrimBytes(byte[] bytes)
        {
            int counter = bytes.Length - 1;
            while (bytes[counter] == 0x00)
                counter--;
            
            byte[] trimmed = new byte[counter+1];
            for (int i = 0; i <= counter; i++)
                trimmed[i] = bytes[i];
            
            return trimmed;
        }

    }
}
