![Shamisen Logo](Shamisen-Logo.svg)

# Shamisen - .NET Audio Library

A Cross-Platform Audio Library for:

- .NET 5
- .NET Core 3.1
- .NET Standard 2.1
- .NET Standard 2.0

## Usage of Shamisen ##

- Abstraction Layer for Audio I/O
- Digital Signal Processing

## Currently implemented features ##

### Audio I/O and bindings
#### Managed backends

| Name (Backend) | Author (Backend) | License (Binding) | Windows10 Win32 | Windows10 UWP | Android | Linux | iOS | Mac OSX | 
|--|--|--|:--:|:--:|:--:|:--:|:--:|:--:|
| [UWP](https://docs.microsoft.com/en-us/windows/uwp/get-started/universal-application-platform-guide) | [Microsoft](https://github.com/microsoft) | [Apache License 2.0](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) | ❎ | ✅ | ❎ | ❎ | ❎ | ❎ |
| [Xamarin.Android](https://github.com/xamarin/xamarin-android) | [Xamarin](https://github.com/xamarin) | [Apache License 2.0](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) | ❎ | ❎ | ✅ | ❎ | ❎ | ❎ |
| [NAudio](https://github.com/naudio/NAudio) | [NAudio](https://github.com/naudio) | [Apache License 2.0](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) | ✅ | ❎ | ❎ | ❎ | ❎ | ❎ |
| [CSCore](https://github.com/filoe/cscore) | [Florian](https://github.com/filoe) | [Ms-PL](https://github.com/filoe/cscore/blob/master/license.md) | ✅ | ❓ | ❎ | ❎ | ❎ | ❎ |
| [OpenTK](https://github.com/opentk/opentk) | [OpenTK](https://github.com/opentk) | [Apache License 2.0](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) | ✅ | ❓ | ❓ | ❓ | ❓ | ❓ |

❓: Not Tested or needs more information  
✅: Tested  
❎: Impossible  

### Digital Signal Processing (Cross-Platform)

- Fast and smooth Up-sampling using Catmull-Rom Spline
  - Utilizes `Vector4` for resampling calculation.
  - Uses Direct/Wrapped caching for Catmull-Rom spline coefficients. 
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
  - Unrolls channel loop for Monaural and <5ch filter calculation.
  - For some special cases, it utilizes SSEx.x and AVX(2) intrinsics for the calculation.

### File Formats and Codecs

#### Cross-Platform

| Container Name | Typical File Extensions | Implemented Codec | Library contains Decoder/Encoder | License | Decoding | Encoding |
|--|--|--|--|--|:--:|:--:|
| Waveform<br/>RF64 | `.wav` | Linear PCM, IEEE 754 Floating-Point PCM, A-Law | Shamisen | [Apache License 2.0](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) | ✅ | ✅ |
| FLAC | `.flac` | FLAC | Shamisen | [Apache License 2.0](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) | ✅ | ❎(Planned) |

Legends:  
✅: Supported by Shamisen itself  
✔: Supported by another library and its wrapper for Shamisen  
❎: Not supported by Shamisen without any custom integration  

#### Platform-Dependent

- Any formats supported by platform-dependent binding libraries

## Dependencies and system requirements ##

- The speed of `SplineResampler` depends on the fast C# Integer Division Library **[DivideSharp](https://github.com/MineCake147E/DivideSharp)**
  - Divides by "almost constant" number, about 2x faster than ordinal division(idiv instruction)!
  - Implements the same technology that is used in `RyuJIT` constant division optimization, ported to C#!
  - Improved `SplineResampler`'s performance greatly, about **1.5x** faster on Stereo!
- Currently, **_Unity IS NOT SUPPORTED AT ALL!_**
  - Because Unity uses older version of `Mono`.
- Faster resampling requires `.NET 5` or `.NET Core 3.1`.
  - Unfortunately, `.NET Framework` does not support Fast `Span<T>`s.
- The all processing in this library fully depends on SINGLE core.
  - Because `Span<T>` does not support multi-thread processing at all.

## Useful external library for Shamisen ##

- [CSCodec](https://github.com/MineCake147E/CSCodec) that supports more signal processing like FFT and DWT.

## Features planned or under development ##


### Audio I/O and bindings

#### Native backends

✅: Possible  
❓: Needs more information  
❎: Impossible   

| Name of Backend | Author of Backend | License (binding) | Target Platform | Status |
|--|--|--|--|--|
| [Oboe](https://github.com/google/oboe) | [Google](https://github.com/google) | [Apache License 2.0](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) | Android >10 | Gathering Information |

#### Managed backends

✅: Possible  
❓: Needs more information  
❎: Impossible  

| Name of Backend | Author of Backend | License (binding) | Target Platforms | Status |
|--|--|--|--|--|
| Xamarin.iOS | Microsoft | [Apache License 2.0](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) | iOS | Planned |


### File Formats and Codecs

#### Cross-Platform

✅: Shamisen will have Managed Implementation of decoder/encoder itself  
⭕: Shamisen will have Managed Wrapper for another library  
❎: Not included in plan currently  

| Container Name | Typical File Extensions | Target Codec | Planned Library containing Decoder/Encoder | Planned Library License | Decoding | Encoding | Status |
|--|--|--|--|--|:--:|:--:|--|
| FLAC | `.flac` | FLAC | Shamisen | [Apache License 2.0](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) | ✅ | ✅ | Implemented Decoder |
| Opus | `.opus` | Opus | Shamisen(Decoder)<br/>Shamisen.Codecs.Opus(Encoder) | [Apache License 2.0](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) | ✅ | ⭕ | Planned |
| Ogg | `.ogg` | Vorbis | Shamisen.Codecs.Ogg | [Apache License 2.0](https://github.com/MineCake147E/Shamisen/blob/develop/LICENSE.md) | ⭕ | ⭕ | Planned |
