using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ElectronChat.Networking;

namespace ElectronChat
{
    class Utils
    {
        
        public static void RefreshNodes()
        {
            List<Node> toAdd = new List<Node>();
            List<Node> toRemove = new List<Node>();

            foreach (var node in Program.settings.Nodes)
            {
                try
                {
                    Sender sender = new Sender(node, 8000);
                    string nodes = sender.Send("GetNodes");
                    sender.Close();
                    List<Node> retrievedNodes = JsonSerializer.Deserialize<List<Node>>(nodes);
                    foreach (var rNode in retrievedNodes)
                        if (!ContainsNode(rNode.Name, rNode.IP, rNode.Port))
                            toAdd.Add(rNode);
                } catch (Exception)
                {
                    toRemove.Add(node);
                }
            }

            foreach (var remove in toRemove)
            {
                Program.settings.Nodes.Remove(remove);
            }

            foreach (var add in toAdd)
            {
                try
                {
                    new Sender(add, 8000).Send("Ping");
                    Program.settings.Nodes.Add(add);
                }
                catch (Exception) {}
            }

            SaveSettings();
        }

        public static bool ContainsNode(string Name, string IP, int Port)
        {
            foreach (var node in Program.settings.Nodes)
                if (node.Name == Name && node.IP == IP && node.Port == Port)
                    return true;
            return false;
        }

        public static Node GetNode(string Name, string IP, int Port)
        {
            foreach (var node in Program.settings.Nodes)
                if (node.Name == Name && node.IP == IP && node.Port == Port)
                    return node;
            throw new Exception("Node not found!");
        }

        public static bool IsChatting(Node node)
        {
            foreach (var chat in Program.chats)
                if (node.Name == chat.Opponent.Name && node.IP == chat.Opponent.IP && node.Port == chat.Opponent.Port)
                    return true;
            return false;
        }

        public static Chat GetChat(Node node)
        {
            foreach (var chat in Program.chats)
                if (node.Name == chat.Opponent.Name && node.IP == chat.Opponent.IP && node.Port == chat.Opponent.Port)
                    return chat;
            throw new Exception("Chat not found!");
        }

        public static void SaveSettings()
        {
            File.WriteAllText("settings.json", JsonSerializer.Serialize(Program.settings, new JsonSerializerOptions(){ WriteIndented = true }));
        }

        public static byte[] TrimBytes(byte[] bytes)
        {
            int counter = bytes.Length - 1;
            while (counter >= 0 && bytes[counter] == 0x00)
                counter--;
            
            byte[] trimmed = new byte[counter+1];
            for (int i = 0; i <= counter; i++)
                trimmed[i] = bytes[i];
            
            return trimmed;
        }

    }
}