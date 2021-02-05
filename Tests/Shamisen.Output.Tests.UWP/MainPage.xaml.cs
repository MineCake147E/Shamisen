using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Shamisen.Synthesis;
using Shamisen.Formats;
using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.IO;
using Windows.Media.Devices;
using Windows.Devices.Enumeration;
using System.Text;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace Shamisen.Output.Tests.UWP
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private const int SampleRate = 192000;
        private ISoundOut soundOut;
        private ISampleSource source;
        private SplineResampler resampler;

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            //soundOut = await AudioGraphOutput.CreateAudioGraphOutput(2, SampleRate);
            //source = new SinusoidSource(new SampleFormat(2, 44100)) { Frequency = 436 };
            //resampler = new SplineResampler(source, SampleRate);
            //soundOut.Initialize(new SampleToFloat32Converter(resampler));
            //soundOut.Play();

            //var str = MediaDevice.GetAudioRenderSelector();
            var a = await DeviceInformation.FindAllAsync(DeviceClass.AudioRender);
            var sb = new StringBuilder();
            foreach (var item in a)
            {
                sb.AppendLine($"Defined properties in {item.Name}:");
                foreach (var prop in item.Properties)
                {
                    sb.AppendLine($"{prop.Key} = {prop.Value}");
                }
                sb.AppendLine();
            }
            TextBlock1.Text = sb.ToString();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            soundOut?.Dispose();
        }
    }
}
