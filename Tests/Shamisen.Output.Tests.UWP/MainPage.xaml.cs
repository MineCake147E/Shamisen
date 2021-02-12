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
using Shamisen.Filters;
using Shamisen.Filters.Mixing;
using Shamisen.Pipeline;
using Windows.Storage;
using Shamisen.Codecs.Waveform;
using Shamisen.Codecs.Waveform.Parsing;
using Shamisen.Data;
using Shamisen.Conversion.WaveToSampleConverters;

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
        private SinusoidSource source, source2;
        private StorageFile file;
        private IWaveSource waveSource;
        private ISampleSource sampleSource;
        private SplineResampler resampler;

        //private BiQuadFilter biQuadFilter;
        private SimpleMixer mixer;

        private async void Button_ClickAsync(object sender, RoutedEventArgs e) => soundOut.Play();

        private void Page_Unloaded(object sender, RoutedEventArgs e) => soundOut?.Dispose();

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!(source is null))
            {
                source.Frequency = e.NewValue;
                source2.Frequency = e.NewValue * 3;
            }
        }

        private async void Page_LoadedAsync(object sender, RoutedEventArgs e)
        {
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
            //source = new SinusoidSource(new SampleFormat(2, SampleRate)) { Frequency = 512 };
            /*resampler = new SplineResampler(source, SampleRate);
            biQuadFilter = new BiQuadFilter(resampler, BiQuadParameter.CreateLPFParameter(SampleRate, SampleRate / 2.0, 1));*/
            //source2 = new SinusoidSource(new SampleFormat(2, SampleRate)) { Frequency = 512 * 3 };
            //var itemA = new MixerItem(source, new AudioSourceProperties<float, SampleFormat>(source, true, 0.016))
            //{
            //    Volume = 0.5f
            //};
            //var itemB = new MixerItem(source2, new AudioSourceProperties<float, SampleFormat>(source2, true, 0.016))
            //{
            //    Volume = 0.5f / 3
            //};

            //mixer = new SimpleMixer(itemA, itemB);
        }

        private async void BSelect_ClickAsync(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add(".wav");

            file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                TBPath.Text = file.Name;
            }
            else
            {
                TBPath.Text = "";
            }
        }

        private async void BInit_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (file is null)
            {
                ContentDialog noFileDialog = new ContentDialog
                {
                    Title = "No file selected!",
                    Content = "Select file and click again.",
                    CloseButtonText = "Ok"
                };

                ContentDialogResult result = await noFileDialog.ShowAsync();
                return;
            }
            soundOut?.Dispose();
            soundOut = await AudioGraphOutput.CreateAudioGraphOutputAsync(2, SampleRate);
            var h = await file.OpenReadAsync();
            var f = new SimpleWaveParser(new SimpleChunkParserFactory(), new StreamDataSource(h.AsStream()));
            switch (f.Format.BitDepth)
            {
                case 8 when f.Format.Encoding == AudioEncoding.LinearPcm:
                    sampleSource = new Pcm8ToSampleConverter(f);
                    break;
                case 16 when f.Format.Encoding == AudioEncoding.LinearPcm:
                    sampleSource = new Pcm16ToSampleConverter(f);
                    break;
                case 32 when f.Format.Encoding == AudioEncoding.LinearPcm:
                    sampleSource = new Pcm32ToSampleConverter(f);
                    break;
                case 32 when f.Format.Encoding == AudioEncoding.IeeeFloat:
                    sampleSource = new Float32ToSampleConverter(f);
                    break;
                case 24 when f.Format.Encoding == AudioEncoding.LinearPcm:
                    sampleSource = new Pcm24ToSampleConverter(f);
                    break;
                default:
                    {
                        var notSupportedDialog = new ContentDialog
                        {
                            Title = "Not supported!",
                            Content = $"{f.Format} is not supported!",
                            CloseButtonText = "Ok"
                        };

                        _ = await notSupportedDialog.ShowAsync();
                        return;
                    }
            }
            soundOut.Initialize(new SampleToFloat32Converter(sampleSource));
        }
    }
}
