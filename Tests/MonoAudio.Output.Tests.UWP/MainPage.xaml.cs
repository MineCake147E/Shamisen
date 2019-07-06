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
using MonoAudio.SoundOut;
using MonoAudio.Synthesis;
using MonoAudio.Formats;
using MonoAudio.Conversion.Resampling.Sample;
using MonoAudio.Conversion.SampleToWaveConverters;
using MonoAudio.IO;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace MonoAudio.Output.Tests.UWP
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            soundOut = await AudioGraphOutput.CreateAudioGraphOutput(2, SampleRate);
            source = new SinusoidSource(new SampleFormat(2, 44100)) { Frequency = 436 };
            resampler = new SplineResampler(source, SampleRate);
            soundOut.Initialize(new SampleToFloat32Converter(resampler));
            soundOut.Play();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            soundOut.Dispose();
        }
    }
}
