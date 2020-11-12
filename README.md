![MonoAudio Logo](MonoAudio-Logo.png)

# MonoAudio - .NET Audio Library
A Cross-Platform Audio Library for .NET.

#### Usage of MonoAudio
- An audio output abstraction layer `MonoAudio.Core`

### Currently supported features
- Audio outputs
  - [CSCore](https://github.com/filoe/cscore) Inter-Operating output
  - UWP `AudioGraph` output
- Fast and smooth Up-sampling using Catmull-Rom Spline
  - Benchmarks on .Net Core, Intel Core i7 4790
    Note that the results are not inversely proportional due to differences in caching strategies.
    - About 170x faster than playback in 44.1kHz→192kHz **10ch**(e.g. 9.1ch).
    - About 520x faster than playback in 44.1kHz→192kHz **Stereo**.
    - About 830x faster than playback in 44.1kHz→192kHz **4ch**(e.g. 3.1ch).
    - About 1200x faster than playback in 44.1kHz→192kHz **Monaural**.
    - About 150x faster than playback in 48kHz→192kHz **10ch**(e.g. 9.1ch).
    - About 580x faster than playback in 48kHz→192kHz **Stereo**.
    - About 1200x faster than playback in 48kHz→192kHz **4ch**(e.g. 3.1ch).
    - About 1300x faster than playback in 48kHz→192kHz **Monaural**.
  - Uses `MemoryMarshal.Cast` so it doesn't copy while casting.
- `FastFill` for some types that fills quickly using `Vector<T>`.
- Optimized BiQuad Filters that supports some filtering
  - Uses `Vector2` and `Vector3` for filter calculations in each channels.

### Currently implemented features(not tested yet)

### Dependencies and system requirements
- The speed of `SplineResampler` depends on the fast C# Integer Division Library **[DivideSharp](https://github.com/MineCake147E/DivideSharp)**
  - Divides by "almost constant" number, about 2x faster than ordinal division(idiv instruction)!
  - Implements the same technology that is used in `RyuJIT` constant division optimization, ported to C#!
  - Improved `SplineResampler`'s performance greatly, about **1.5x** faster on Stereo!
- Currently, ***Unity IS NOT SUPPORTED AT ALL!***
  - Because Unity uses older version of `Mono`.
- Faster resampling requires `.NET Core` or later version of `Mono`.
  - Unfortunately, `.NET Framework` does not support Fast `Span<T>`s.
- The all processing in this library fully depends on SINGLE core.
  - Because `Span<T>` does not support multi-thread processing at all.

### Useful external library for MonoAudio
- [CSCodec](https://github.com/MineCake147E/CSCodec) that supports more signal processing like FFT and DWT.

### Features under development
- Xamarin.Android `AudioTrack` output
- Xamarin.iOS `AudioUnit` output
- OpenTK `AL` output
