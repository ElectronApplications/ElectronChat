using System;
using System.ComponentModel;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ElectronChat.Views
{
    class MainWindow : Window
    {
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            Closed += OnClose;
        }
        
        void OnClose(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                try
                {
                    Utils.SaveSettings();
                }
                catch (Exception) {}
                Environment.Exit(0);
            }).Start();
        }

    }
}
