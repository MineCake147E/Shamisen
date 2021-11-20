using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Reactive.Bindings;

using ReactiveUI;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Filters;
using Shamisen.Filters.Mixing;
using Shamisen.IO;
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

        public ObservableCollection<DeviceViewModel> Devices { get; } = new ObservableCollection<DeviceViewModel>();

        private readonly List<OpenALOutput> outputs = new();
        private readonly List<IFrequencyGeneratorSource> sinusoidSources = new();

        public MainWindowViewModel()
        {
            Initialize = ReactiveCommand.Create(InitializeInternal);
            Play = ReactiveCommand.Create(PlayInternal);
            Stop = ReactiveCommand.Create(StopInternal);
            Pause = ReactiveCommand.Create(PauseInternal);
            Resume = ReactiveCommand.Create(ResumeInternal);
            _ = Frequency.Subscribe((a) =>
              {
                  foreach (var item in sinusoidSources)
                  {
                      item.Frequency = a;
                  }
              });
            _ = Task.Run(() =>
            {
                foreach (var item in OpenALDeviceEnumerator.Instance.EnumerateDevices(DataFlow.Render))
                {
                    if (item is IAudioOutputDevice<OpenALOutput> device)
                    {
                        Devices.Add(new DeviceViewModel(device));
                    }
                }
            }).ConfigureAwait(false);
        }

        private void InitializeInternal()
        {
            var y = 1;
            foreach (var item in Devices.Where(a => a.Checked).Select(a => a.Device))
            {
                var t = item.CreateSoundOut();
                var source = new TriangleWaveSource(new SampleFormat(1, 192000)) { Frequency = 440 * y++ };
                //var resampler = new SplineResampler(source, 192000);
                //var biquad = new BiQuadFilter(resampler, BiQuadParameter.CreateNotchFilterParameterFromQuality(192000, 440, 3.0));
                if (item.CheckSupportStatus(new WaveFormat(192000, 32, 1, AudioEncoding.IeeeFloat)).IsSupported)
                {
                    t.Initialize(new SampleToFloat32Converter(source));
                }
                else
                {
                    t.Initialize(new SampleToPcm8Converter(source, false));
                }
                outputs.Add(t);
                sinusoidSources.Add(source);
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

        public DeviceViewModel(IAudioOutputDevice<OpenALOutput> device)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
        }

        public bool Checked
        {
            get => @checked;
            set => this.RaiseAndSetIfChanged(ref @checked, value);
        }

        public IAudioOutputDevice<OpenALOutput> Device { get; }

        public string Name => Device.Name;
    }
}
