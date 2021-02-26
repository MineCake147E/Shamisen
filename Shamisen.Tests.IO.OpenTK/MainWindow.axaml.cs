using System;
using Shamisen;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Shamisen.IO;
using System.Text;
using System.Collections.ObjectModel;

namespace Shamisen.Tests.IO.OpenTK
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
