using System;
using System.Collections.ObjectModel;
using System.Threading;
using ReactiveUI;

namespace ElectronChat.ViewModels
{
    class NodesViewModel : ViewModelBase
    {
        public ObservableCollection<Node> Nodes
        {
            get => nodes;
            set => this.RaiseAndSetIfChanged(ref nodes, value);
        }
        public string Search
        { 
            get => search;
            set { search = value; UpdateNodes(); }
        }
        public bool OnlyChats
        {
            get => onlyChats;
            set { onlyChats = value; UpdateNodes(); }
        }
        public Node SelectedNode
        {
            get => selectedNode;
            set { selectedNode = value; mainWindow.ChooseNode(SelectedNode); }
        }

        private MainWindowViewModel mainWindow;
        private Node selectedNode;
        private ObservableCollection<Node> nodes;
        private string search;
        private bool onlyChats;

        public NodesViewModel(MainWindowViewModel mainWindow)
        {
            this.mainWindow = mainWindow;
            Nodes = new ObservableCollection<Node>(Program.settings.Nodes);
            RefreshNodes();
        }

        public void RefreshNodes()
        {
            new Thread(() =>
            {
                try
                {
                    Utils.RefreshNodes();
                }
                catch (Exception) {}
                UpdateNodes();
            }).Start();      
        }

        public void UpdateNodes()
        {
            if (Search == null)
                Search = "";
            
            Nodes = new ObservableCollection<Node>();
            foreach (var node in Program.settings.Nodes)
                if ((node.Name.Contains(Search) || (node.IP+":"+node.Port).Contains(Search)) && (!OnlyChats || Utils.IsChatting(node)))
                    Nodes.Add(node);
        }

    }
}