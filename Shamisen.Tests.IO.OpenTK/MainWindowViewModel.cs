using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Reactive.Bindings;

using ReactiveUI;

using Shamisen.Conversion.SampleToWaveConverters;
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
            int y = 1;
            foreach (var item in Devices.ToArray().Where(a => a.Checked))
            {
                var t = item.Device.CreateSoundOut();
                var source = new SinusoidSource(new SampleFormat(2, 192000)) { Frequency = 440 * y++ };
                t.Initialize(new SampleToPcm16Converter(source, false));
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

        public DeviceViewModel(IAudioOutputDevice<OpenALOutput> device) => Device = device ?? throw new ArgumentNullException(nameof(device));

        public bool Checked
        {
            get => @checked;
            set => this.RaiseAndSetIfChanged(ref @checked, value);
        }

        public IAudioOutputDevice<OpenALOutput> Device { get; }

        public string Name => Device.Name;
    }
}
