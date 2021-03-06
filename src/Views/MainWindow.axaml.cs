using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ElectronChat.Views
{
    class MainWindow : Window
    {
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            Closing += onClose;
        }
        
        void onClose(object sender, CancelEventArgs e)
        {
            Utils.SaveSettings();
            Environment.Exit(0);
        }

    }
}
