using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using DynamicData;

using Reactive.Bindings;

using ReactiveUI;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Conversion.WaveToSampleConverters;
using Shamisen.Filters;
using Shamisen.Filters.Mixing;
using Shamisen.IO;
using Shamisen.IO.Devices;
using Shamisen.Synthesis;

using ReactiveCommand = ReactiveUI.ReactiveCommand;

namespace Shamisen.Tests.IO.OpenTK
{
    public class MainWindowViewModel : ReactiveObject
    {
        public ICommand Initialize { get; }

        public ICommand Play { get; }

        public ICommand Pause { get; }

        public ICommand Resume { get; }

        public ICommand Stop { get; }

        public ReactiveProperty<double> Frequency { get; } = new(440);

        public ReactiveProperty<double> NyquistFrequency { get; } = new(SampleRate * 0.5);

        public ReactiveProperty<double> Volume { get; } = new(50);

        public ReactiveProperty<WaveformViewModel> SelectedWaveform { get; }

        public ObservableCollection<DeviceViewModel> Devices { get; } = new();

        public ObservableCollection<WaveformViewModel> Waveforms { get; } = new();

        private const int SampleRate = 192000;
        private readonly List<OpenALOutput> outputs = new();
        private readonly List<(IFrequencyGeneratorSource waveform, AudioSocket<float, SampleFormat> socket, Attenuator volume)> waveformSources = new();

        public MainWindowViewModel()
        {
            Initialize = ReactiveCommand.Create(InitializeInternal);
            Play = ReactiveCommand.Create(PlayInternal);
            Stop = ReactiveCommand.Create(StopInternal);
            Pause = ReactiveCommand.Create(PauseInternal);
            Resume = ReactiveCommand.Create(ResumeInternal);
            var sinusoid = new WaveformViewModel("Sinusoid", a => new SinusoidSource(a));
            var square = new WaveformViewModel("Square", a => new SquareWaveSource(a));
            var sawtooth = new WaveformViewModel("Sawtooth", a => new SawtoothWaveSource(a));
            var triangle = new WaveformViewModel("Triangle", a => new TriangleWaveSource(a));
            SelectedWaveform = new(sinusoid);
            Waveforms.Add(sinusoid);
            Waveforms.Add(square);
            Waveforms.Add(sawtooth);
            Waveforms.Add(triangle);
            _ = Frequency.Subscribe((a) =>
              {
                  foreach (var item in waveformSources)
                  {
                      item.waveform.Frequency = a;
                  }
              });
            _ = Volume.Subscribe(a =>
            {
                foreach (var item in waveformSources)
                {
                    item.volume.Scale = (float)(a * 0.01);
                }
            });
            _ = SelectedWaveform.Subscribe(a =>
            {
                for (var i = 0; i < waveformSources.Count; i++)
                {
                    var q = waveformSources[i];
                    var g = a.GenerateFunc(q.socket.Format);
                    if (g is not ISampleSource ss) throw new InvalidProgramException("");
                    g.Frequency = q.waveform.Frequency;
                    if (g is IPeriodicGeneratorSource<Fixed64> pg && q.waveform is IPeriodicGeneratorSource<Fixed64> pq)
                    {
                        pg.Theta = pq.Theta;
                    }
                    var t = q.socket.ReplaceSource(ss);
                    t?.Dispose();
                    q.waveform = g;
                    waveformSources[i] = q;
                }
            });
            _ = Task.Run(() =>
            {
                foreach (var item in OpenALDeviceEnumerator.Instance.EnumerateDevices(DataFlow.Render))
                {
                    if (item is OpenALDevice device)
                    {
                        Devices.Add(new DeviceViewModel(device));
                    }
                }
            }).ConfigureAwait(false);
        }

        private void InitializeInternal()
        {
            outputs.ForEach(a =>
            {
                if (a.PlaybackState != PlaybackState.Stopped) a.Stop();
                a.Dispose();
            });
            waveformSources.ForEach(a =>
            {
                (a.waveform as IDisposable)?.Dispose();
                a.volume.Dispose();
                a.socket.Dispose();
            });
            outputs.Clear();
            waveformSources.Clear();
            var conf = new OpenALOutputConfiguration(new(TimeSpan.Zero, ConfigurationPropertyPriority.BestEffort));
            foreach (var item in Devices.Where(a => a.Checked).Select(a => a.Device))
            {
                var t = item.CreateSoundOut(conf);
                var source = SelectedWaveform.Value.GenerateFunc(new SampleFormat(1, SampleRate));
                if (source is not ISampleSource ss) throw new InvalidProgramException("");
                source.Frequency = Frequency.Value;
                var socket = new AudioSocket<float, SampleFormat>(ss.Format);
                _ = socket.ReplaceSource(ss);
                var volume = new Attenuator(socket) { Scale = 0.5f };
                //var resampler = new SplineResampler(source, 192000);
                //var biquad = new BiQuadFilter(resampler, BiQuadParameter.CreateNotchFilterParameterFromQuality(192000, 440, 3.0));
                var f2a = new SampleToFloat32Converter(volume);
                var a2f = new Float32ToSampleConverter(f2a);
                if (item.CheckSupportStatus(new WaveFormat(SampleRate, 32, 1, AudioEncoding.IeeeFloat), conf) == FormatPropertySupportStatus.SupportedByBackend)
                {
                    t.Initialize(new SampleToFloat32Converter(a2f));
                }
                else
                {
                    t.Initialize(new SampleToPcm16Converter(a2f, false));
                }
                outputs.Add(t);
                waveformSources.Add((source, socket, volume));
            }
        }

        private void PlayInternal()
        {
            foreach (var item in outputs)
            {
                if (item.PlaybackState == PlaybackState.Stopped) item.Play();
            }
        }

        private void PauseInternal()
        {
            foreach (var item in outputs)
            {
                if (item.PlaybackState == PlaybackState.Playing) item.Pause();
            }
        }

        private void ResumeInternal()
        {
            foreach (var item in outputs)
            {
                if (item.PlaybackState == PlaybackState.Paused) item.Resume();
            }
        }

        private void StopInternal()
        {
            foreach (var item in outputs)
            {
                if (item.PlaybackState == PlaybackState.Playing) item.Stop();
            }
        }
    }

    public sealed class DeviceViewModel : ReactiveObject
    {
        private bool @checked;

        public DeviceViewModel(OpenALDevice device)
        {
            ArgumentNullException.ThrowIfNull(device);
            Device = device;
        }

        public bool Checked
        {
            get => @checked;
            set => this.RaiseAndSetIfChanged(ref @checked, value);
        }

        public OpenALDevice Device { get; }

        public string Name => Device.Name;
    }
}
