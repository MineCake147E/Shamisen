using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

using Reactive.Bindings;

using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.IO;
using Shamisen.IO.WinRt;
using Shamisen.Synthesis;
using Shamisen.Utils;

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
            Level = new ReactiveProperty<double>(-120.0);
            InitializeComponent();
        }

        private AudioGraphOutput? output;
        private AudioGraphInput? input;
        private AudioGraph? ingraph;
        public ReactiveProperty<double> Level { get; private set; }

        private async void Window_ActivatedAsync(object sender, WindowActivatedEventArgs args)
        {
            var source = new SinusoidSource(new(2, 192000))
            {
                Frequency = 436.0
            };
            output = await AudioGraphOutput.CreateAudioGraphOutputAsync(new SampleToFloat32Converter(source), AudioRenderCategory.GameMedia);
            var prop = new AudioGraphSettings(AudioRenderCategory.Media)
            {
                EncodingProperties = new()
                {
                    BitsPerSample = 32,
                    ChannelCount = 1,
                    SampleRate = 192000,
                    Subtype = "Float"
                },
                QuantumSizeSelectionMode = QuantumSizeSelectionMode.ClosestToDesired,
                DesiredSamplesPerQuantum = (int)Math.Floor(192000.0 / 60.0)
            };
            var rg = await AudioGraph.CreateAsync(prop);
            if(rg.Status == AudioGraphCreationStatus.Success)
            {
                var g = rg.Graph;
                g.Stop();
                ingraph = g;
                var ri = await g.CreateDeviceInputNodeAsync(Windows.Media.Capture.MediaCategory.Media);
                if(ri.Status == AudioDeviceNodeCreationStatus.Success)
                {
                    var gi = new AudioGraphInput(g, ri.DeviceInputNode);
                    input = gi;
                    input.DataAvailable += Input_DataAvailable;
                }
            }
        }

        private void Input_DataAvailable(object sender, DataAvailableEventArgs e)
        {
            var max = float.MinValue;
            var data = MemoryMarshal.Cast<byte, float>(e.Data);
            for (int i = 0; i < data.Length; i++)
            {
                var v = data[i];
                max = FastMath.Max(v * v, max);
            }
            var maxInDb = Math.Max(Math.Min(10 * Math.Log10(max), 0), -120.0) + 120.0;
            if (double.IsNaN(maxInDb))
            {
                maxInDb = 0.0;
            }
            DispatcherQueue?.TryEnqueue(() => PbLevel.Value = maxInDb);
        }

        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Testing Audio Output...";
            output?.Play();
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            output?.Dispose();
            input?.Dispose();
            ingraph?.Dispose();
        }

        private void InputTest_Click(object sender, RoutedEventArgs e)
        {
            inputTest.Content = "Testing Audio Input...";
            ingraph?.Start();
            input?.Start();
        }

    }
}
