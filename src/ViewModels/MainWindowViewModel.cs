using System;
using System.Threading;
using ElectronChat.Networking;

namespace ElectronChat.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        public ChatViewModel ChatVM {get;}
        public NodesViewModel NodesVM {get;}
        

        public MainWindowViewModel()
        {
            NodesVM = new NodesViewModel(this);
            ChatVM = new ChatViewModel();
        }

        public void ChooseNode(Node node)
        {
            new Thread(() =>
            {
                try
                {
                    if (Utils.IsChatting(node))
                        ChatVM.ChangeChat(Utils.GetChat(node));
                    else
                        ChatVM.ChangeChat(new Sender(node, 0).MakeChat());
                }
                catch (Exception) {}
            }).Start();
        }
    }
}