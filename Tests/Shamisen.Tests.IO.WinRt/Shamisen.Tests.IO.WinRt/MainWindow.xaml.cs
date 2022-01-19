using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.IO.WinRt;
using Shamisen.Synthesis;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Audio;
using Windows.Media.Render;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Shamisen.Tests.IO.WinRt
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }
        AudioGraphOutput output;
        private async void MyButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
            var source = new SinusoidSource(new(2, 192000))
            {
                Frequency = 666.0
            };
            output = await AudioGraphOutput.CreateAudioGraphOutputAsync(new SampleToFloat32Converter(source), AudioRenderCategory.GameMedia, 2048);
            output.Play();
        }

        private void Window_Closed(object sender, WindowEventArgs args) => output.Dispose();
    }
}
