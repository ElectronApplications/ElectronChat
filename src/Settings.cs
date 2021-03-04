using System.Collections.Generic;

namespace ElectronChat
{
    struct Settings
    {
        public string Name {get; set;}
        public string IP {get; set;}
        public int Port {get; set;}
        public List<Node> Nodes {get; set;}
    }

    struct Node
    {
        public string Name {get; set;}
        public string IP {get; set;}
        public int Port {get; set;}
    }
}