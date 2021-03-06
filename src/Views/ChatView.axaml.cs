using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ElectronChat.Views
{
    class ChatView : UserControl
    {

        public ChatView()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
