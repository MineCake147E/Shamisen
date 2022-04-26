using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using NAudio;
using NAudio.CoreAudioApi;
using NAudio.Wave;

using Reactive.Bindings;

using ReactiveUI;

using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Conversion.WaveToSampleConverters;
using Shamisen.IO;
using Shamisen.Synthesis;

using PlaybackState = Shamisen.IO.PlaybackState;
using ReactiveCommand = ReactiveUI.ReactiveCommand;

namespace Shamisen.Tests.IO.NAudio.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        public ICommand Initialize { get; }

        public ICommand Play { get; }

        public ICommand Pause { get; }

        public ICommand Resume { get; }

        public ICommand Stop { get; }

        public ReactiveProperty<double> Frequency { get; } = new(440);

        private readonly List<ISoundOut> outputs = new();
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
        }

        private void InitializeInternal()
        {
            var source = new SinusoidSource(new SampleFormat(1, 192000)) { Frequency = 440 };
            var f2a = new SampleToFloat32Converter(source);
            var t = new NAudioSoundOutput(new WasapiOut(AudioClientShareMode.Shared, true, 16), f2a);
            outputs.Add(t);
            sinusoidSources.Add(source);
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
}
